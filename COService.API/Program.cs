using Consul;
using COService.API.Auth;
using COService.API.Endpoints;
using COService.Application.Auth;
using COService.Application.Mappings;
using COService.Application.Messaging;
using COService.Application.Repositories;
using COService.Application.Services;
using COService.Application.Interfaces;
using COService.Infrastructure.Data;
using COService.Infrastructure.DependencyInjection;
using COService.Infrastructure.ExternalServices;
using COService.Infrastructure.Messaging;
using COService.Infrastructure.Messaging.Handlers;
using COService.Infrastructure.Repositories;
using COService.Infrastructure.Services;
using COService.Infrastructure.Configuration;
using COService.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Refit;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — client Angular (:4200) localhost ou IP réseau (POC Portainer)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevClient", policy =>
        policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin))
                    return false;

                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return false;

                if (uri.Port != 4200)
                    return false;

                return uri.Host is "localhost"
                    or "127.0.0.1"
                    || uri.Host.StartsWith("192.168.", StringComparison.Ordinal);
            })
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Configuration Entity Framework Core
builder.Services.AddDbContext<COServiceDbContext>(options =>
{
    // Utiliser la chaîne de connexion "chaine" (authentification SQL avec User Id/Password)
    // Cette chaîne fonctionne aussi bien en local qu'en Docker/Portainer
    var connectionString = builder.Configuration.GetConnectionString("chaine");

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});

// Configuration AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPocUserContext>(sp =>
{
    var accessor = sp.GetRequiredService<IHttpContextAccessor>();
    var context = accessor.HttpContext;
    if (context != null && context.Items.TryGetValue(nameof(IPocUserContext), out var value) && value is IPocUserContext user)
    {
        return user;
    }

    return new COService.Infrastructure.Auth.PocUserContext { IsEnabled = false };
});

// Configuration Consul
var consulConfig = builder.Configuration.GetSection("Consul");
builder.Services.Configure<ConsulServiceOptions>(consulConfig);

// Client Consul
builder.Services.AddSingleton<IConsulClient>(sp =>
{
    var consulAddress = consulConfig.GetValue<string>("Address") ?? "http://localhost:8500";
    return new ConsulClient(config =>
    {
        config.Address = new Uri(consulAddress);
    });
});

// Services Consul
builder.Services.AddSingleton<IServiceDiscovery, ServiceDiscovery>();
builder.Services.AddHostedService<ConsulService>();

// Configuration des services externes
builder.Services.Configure<GatewayTokenOptions>(
    builder.Configuration.GetSection(GatewayTokenOptions.SectionName));
builder.Services.AddHttpClient(nameof(GatewayTokenProvider));
builder.Services.AddSingleton<GatewayTokenProvider>();
builder.Services.AddSingleton<IGatewayTokenProvider>(sp => sp.GetRequiredService<GatewayTokenProvider>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<GatewayTokenProvider>());

// Client ActeursService (:8300) — priorité sur legacy EnrolementService/gateway
var acteursConfig = builder.Configuration.GetSection("ExternalServices:ActeursService");
var legacyEnrolementConfig = builder.Configuration.GetSection("ExternalServices:EnrolementService");
var acteursBaseUrl = acteursConfig.GetValue<string>("BaseUrl")
    ?? legacyEnrolementConfig.GetValue<string>("BaseUrl");
var useActeursDirect = !acteursConfig.GetValue("UseApiGateway", false)
    && !string.IsNullOrWhiteSpace(acteursBaseUrl);

if (useActeursDirect)
{
    var acteursTimeout = acteursConfig.GetValue("Timeout", 60);

    builder.Services.AddHttpClient<IEnrolementServiceClient, EnrolementActeursServiceClient>(client =>
    {
        client.BaseAddress = new Uri(acteursBaseUrl!.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(acteursTimeout);
    });
}
else
{
    builder.Services.AddSingleton<IEnrolementServiceClient, EnrolementServiceClientWrapper>();
}
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IReferentielServiceClient, ReferentielServiceClientWrapper>();

// Client Auth Service avec wrapper
builder.Services.AddSingleton<COService.Infrastructure.ExternalServices.IAuthServiceClient, COService.Infrastructure.ExternalServices.AuthServiceClientWrapper>();
builder.Services.AddScoped<COService.Application.Services.IAuthService, COService.Infrastructure.Services.AuthService>();

// Configuration des options de synchronisation Enrolement
builder.Services.Configure<EnrolementSyncOptions>(options =>
{
    options.Enabled = builder.Configuration.GetValue<bool>("EnrolementSync:Enabled", true);
    options.IntervalMinutes = builder.Configuration.GetValue<int>("EnrolementSync:IntervalMinutes", 60);
});

// Configuration RabbitMQ
builder.Services.Configure<RabbitMQOptions>(
    builder.Configuration.GetSection(RabbitMQOptions.SectionName));

// Configuration MinIO
builder.Services.Configure<MinIOOptions>(
    builder.Configuration.GetSection("MinIO"));

// Debug : Vérifier ce qui est lu
var minioConfig = builder.Configuration.GetSection("MinIO").Get<MinIOOptions>();
var secretKeyDisplay = minioConfig?.SecretKey != null ? "***" : "NULL";
Console.WriteLine($"MinIO config lue: Endpoint={minioConfig?.Endpoint}, AccessKey={minioConfig?.AccessKey}, SecretKey={secretKeyDisplay}");

// Service MinIO
builder.Services.AddSingleton<IMinIOService, MinIOService>();

// RabbitMQ Client (singleton pour maintenir la connexion) — legacy exchange evenements.co
builder.Services.AddSingleton<IRabbitMQClient, RabbitMQClient>();

// MassTransit + Saga post-validation (RabbitMQ ou InMemory selon config)
builder.Services.AddCoMassTransit(builder.Configuration);

// Event Publishers
builder.Services.AddScoped<ICertificateEventPublisher, CertificateEventPublisher>();
builder.Services.AddScoped<INotificationEventPublisher, NotificationEventPublisher>();

// RabbitMQ Event Handlers
// NOTE: PartenaireEventHandler/ExportateurEventHandler sont des no-ops (plus de tables locales
// Partenaire/Exportateur) mais restent enregistrés pour ne pas casser la consommation RabbitMQ.
builder.Services.AddScoped<PartenaireEventHandler>();
builder.Services.AddScoped<ExportateurEventHandler>();
builder.Services.AddScoped<ReferentielEventHandler>();

// RabbitMQ Consumer Service (Background Service)
builder.Services.AddHostedService<RabbitMQConsumerService>();

// Service de synchronisation Enrolement
builder.Services.AddScoped<IEnrolementSyncService, EnrolementSyncService>();
builder.Services.AddHostedService<EnrolementSyncService>();

// Repositories
builder.Services.AddScoped<ICertificatOrigineRepository, CertificatOrigineRepository>();
builder.Services.AddScoped<ICertificatLigneRepository, CertificatLigneRepository>();
builder.Services.AddScoped<IAbonnementRepository, AbonnementRepository>();
builder.Services.AddScoped<ICommentaireRepository, CommentaireRepository>();
builder.Services.AddScoped<ICertificateTypeRepository, CertificateTypeRepository>();
builder.Services.AddScoped<IEtatRepository, EtatRepository>();
// Repositories référentiels (synchronisés depuis Referentiel Service)
builder.Services.AddScoped<IPaysRepository, PaysRepository>();
builder.Services.AddScoped<IPortRepository, PortRepository>();
builder.Services.AddScoped<IAeroportRepository, AeroportRepository>();
builder.Services.AddScoped<IDeviseRepository, DeviseRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IIncotermRepository, IncotermRepository>();
builder.Services.AddScoped<IBureauDedouanementRepository, BureauDedouanementRepository>();
builder.Services.AddScoped<IUniteStatistiqueRepository, UniteStatistiqueRepository>();
builder.Services.AddScoped<IDepartementRepository, DepartementRepository>();
builder.Services.AddScoped<IZoneProductionRepository, ZoneProductionRepository>();
builder.Services.AddScoped<IZoneProductionService, ZoneProductionService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services Application
builder.Services.AddScoped<ICertificatOrigineService, CertificatOrigineService>();
builder.Services.AddScoped<ICertificatLigneService, CertificatLigneService>();
builder.Services.AddScoped<IAbonnementService, AbonnementService>();
builder.Services.AddScoped<ICommentaireService, CommentaireService>();
builder.Services.AddScoped<ICertificateTypeService, CertificateTypeService>();
builder.Services.AddScoped<IEtatService, EtatService>();
builder.Services.AddScoped<IReferentielEtatsClient, ReferentielEtatsClient>();
builder.Services.AddScoped<INumeroGenerationService, NumeroGenerationService>();
builder.Services.AddScoped<IFormuleAService, FormuleAService>();
builder.Services.AddScoped<IPDFGenerationService, PDFGenerationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

var app = builder.Build();

// Appliquer les migrations (ex. drop FK codes référentiel qui bloquent la création de CO)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<COService.Infrastructure.Data.COServiceDbContext>();
        db.Database.Migrate();

        // Réaligner libellés workflow (ex. 45 « Controller » → « Approuvé »)
        var etatService = scope.ServiceProvider.GetRequiredService<IEtatService>();
        await etatService.SeedEtatsWorkflowAsync("startup");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Échec application des migrations EF / seed états au démarrage");
    }
}

// Configure the HTTP request pipeline.
// Swagger activé en Development et Production
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "COService API v1");
    c.RoutePrefix = "swagger"; // Swagger UI à /swagger
});

// Redirection de la racine vers Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Middleware de gestion d'erreur
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        var detail = exception?.Message ?? "Une erreur inattendue s'est produite";
        if (exception?.InnerException != null)
        {
            detail = $"{detail} | {exception.InnerException.Message}";
        }

        var response = new
        {
            status = StatusCodes.Status500InternalServerError,
            title = "Une erreur s'est produite",
            detail,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    });
});

app.UseHttpsRedirection();
app.UseCors("DevClient");
app.UsePocAuthorization();

// Endpoints de vérification de santé
app.MapHealthEndpoints();

// Endpoints CRUD
app.MapCertificatEndpoints();
app.MapCertificatLigneEndpoints();
app.MapAbonnementEndpoints();
app.MapCommentaireEndpoints();
app.MapCertificateTypeEndpoints();
app.MapPartenaireEndpoints();
app.MapExportateurEndpoints();
app.MapEtatEndpoints();
app.MapWorkflowEndpoints();
app.MapFormuleAEndpoints();
app.MapPDFEndpoints();
app.MapEnrolementSyncEndpoints();
app.MapDocumentEndpoints();
app.MapReferentielEndpoints();
app.MapZoneProductionEndpoints();

app.Run();

// Fonctions helper pour les policies Polly
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                // Log retry si nécessaire
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}
