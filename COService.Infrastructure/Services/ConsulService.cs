using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace COService.Infrastructure.Services;

public class ConsulServiceOptions
{
    public bool Enabled { get; set; } = true;
    public string Address { get; set; } = "http://srv-guot-cont.gumar.local:8500";
    public string ServiceName { get; set; } = "cert-origine";
    public string ServiceId { get; set; } = "cert-origine-1";
    public string ServiceAddress { get; set; } = "http://localhost:8700";
    public HealthCheckOptions HealthCheck { get; set; } = new();
}

public class HealthCheckOptions
{
    public string Endpoint { get; set; } = "/sante";
    public int Interval { get; set; } = 10; // secondes
    public int Timeout { get; set; } = 5; // secondes
    public int DeregisterCriticalServiceAfter { get; set; } = 30; // secondes
}

/// <summary>
/// Service pour l'enregistrement et la découverte de services via Consul
/// </summary>
public class ConsulService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly ConsulServiceOptions _options;
    private readonly ILogger<ConsulService> _logger;
    private string? _serviceId;

    public ConsulService(
        IConsulClient consulClient,
        IOptions<ConsulServiceOptions> options,
        ILogger<ConsulService> logger)
    {
        _consulClient = consulClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Si Consul est désactivé, on ne fait rien
        if (!_options.Enabled)
        {
            _logger.LogInformation("Consul est désactivé. Le service ne sera pas enregistré.");
            return;
        }

        _serviceId = _options.ServiceId;

        var serviceUri = new Uri(_options.ServiceAddress);
        var registration = new AgentServiceRegistration
        {
            ID = _serviceId,
            Name = _options.ServiceName,
            Address = serviceUri.Host,
            Port = serviceUri.Port,
            // Tags alignés sur les autres MS SEG (Assurance, Référentiel, DI…) pour le gateway
            Tags = new[]
            {
                "api",
                "microservice",
                "certificat-origine",
                "route-prefix:co",
                "downstream-path:/api/{everything}"
            },
            Check = new AgentServiceCheck
            {
                HTTP = $"{serviceUri.GetLeftPart(UriPartial.Authority)}{_options.HealthCheck.Endpoint}",
                Interval = TimeSpan.FromSeconds(_options.HealthCheck.Interval),
                Timeout = TimeSpan.FromSeconds(_options.HealthCheck.Timeout),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(_options.HealthCheck.DeregisterCriticalServiceAfter)
            }
        };

        try
        {
            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
            _logger.LogInformation(
                "Service {ServiceName} (ID: {ServiceId}) enregistré dans Consul à l'adresse {Address}",
                _options.ServiceName,
                _serviceId,
                _options.ServiceAddress);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Impossible de s'enregistrer dans Consul. L'application continue sans Consul. " +
                "Assurez-vous que Consul est démarré sur {ConsulAddress}",
                _options.Address);
            // On ne lance pas l'exception pour permettre à l'application de démarrer sans Consul
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Si Consul est désactivé ou si le service n'a pas été enregistré, on ne fait rien
        if (!_options.Enabled || string.IsNullOrEmpty(_serviceId))
        {
            return;
        }

        try
        {
            await _consulClient.Agent.ServiceDeregister(_serviceId, cancellationToken);
            _logger.LogInformation("Service {ServiceId} désenregistré de Consul", _serviceId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors du désenregistrement du service dans Consul");
            // On ne lance pas l'exception pour ne pas bloquer l'arrêt de l'application
        }
    }
}

