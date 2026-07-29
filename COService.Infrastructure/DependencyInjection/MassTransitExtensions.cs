using COService.Infrastructure.Data;
using COService.Infrastructure.Messaging;
using COService.Infrastructure.Messaging.Consumers;
using COService.Infrastructure.Sagas;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace COService.Infrastructure.DependencyInjection;

public static class MassTransitExtensions
{
    public static IServiceCollection AddCoMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        var mtSection = configuration.GetSection("MassTransit");
        if (!mtSection.GetValue("Enabled", true))
        {
            // Bus minimal pour satisfaire IPublishEndpoint (pas de saga).
            services.AddMassTransit(x => x.UsingInMemory((_, _) => { }));
            return services;
        }

        var rabbit = configuration.GetSection(RabbitMQOptions.SectionName).Get<RabbitMQOptions>()
                     ?? new RabbitMQOptions();

        var useInMemorySaga = mtSection.GetValue("UseInMemorySaga", true);
        // Si RabbitMQ legacy est off, le bus MassTransit tourne en mémoire (stubs locaux).
        var useInMemoryTransport = mtSection.GetValue("UseInMemoryTransport", !rabbit.Enabled);

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumer<GenererFactureConsumer>();
            x.AddConsumer<GenererPdfConsumer>();
            x.AddConsumer<EnvoyerNotificationConsumer>();
            x.AddConsumer<CertificatPretPourEchangeConsumer>();

            var sagaRegistration = x.AddSagaStateMachine<CertificatPostValidationStateMachine, CertificatPostValidationState>();

            if (useInMemorySaga)
                sagaRegistration.InMemoryRepository();
            else
            {
                sagaRegistration.EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.ExistingDbContext<COServiceDbContext>();
                });
            }

            if (useInMemoryTransport)
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
            else
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbit.HostName, (ushort)rabbit.Port, rabbit.VirtualHost, h =>
                    {
                        h.Username(rabbit.UserName);
                        h.Password(rabbit.Password);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}
