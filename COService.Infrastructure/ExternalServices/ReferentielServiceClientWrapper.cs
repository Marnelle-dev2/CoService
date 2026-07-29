using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Client Référentiel en accès direct (sans JWT).
/// Exemple : http://srv-guot-cont.gumar.local:8290/api/pays
/// </summary>
public class ReferentielServiceClientWrapper : IReferentielServiceClient
{
    private readonly ILogger<ReferentielServiceClientWrapper> _logger;
    private readonly IReferentielServiceClient _client;
    private readonly IMemoryCache _cache;

    public ReferentielServiceClientWrapper(
        ILogger<ReferentielServiceClientWrapper> logger,
        IConfiguration configuration,
        IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;

        var serviceConfig = configuration.GetSection("ExternalServices:ReferentielService");
        var useGateway = serviceConfig.GetValue("UseApiGateway", false);
        var timeout = serviceConfig.GetValue("Timeout", 120);

        var requireAuth = serviceConfig.GetValue("RequireAuth", false);
        var bearerToken = requireAuth
            ? (configuration.GetValue<string>("ApiGateway:BearerToken")
               ?? configuration.GetValue<string>("ExternalServices:ReferentielService:BearerToken")
               ?? configuration.GetValue<string>("ExternalServices:EnrolementService:BearerToken"))
            : null;

        // Priorité : BaseUrl dédié du service (ex. :8290), sinon gateway si UseApiGateway=true
        string? dedicatedBase = serviceConfig.GetValue<string>("BaseUrl");
        string baseAddress;
        if (!string.IsNullOrWhiteSpace(dedicatedBase))
        {
            baseAddress = dedicatedBase.TrimEnd('/');
            useGateway = false;
        }
        else if (useGateway)
        {
            baseAddress = configuration.GetValue<string>("ApiGateway:BaseUrl")
                ?? throw new InvalidOperationException("ApiGateway:BaseUrl non configuré");
        }
        else
        {
            baseAddress = "http://srv-guot-cont.gumar.local:8290";
        }

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(timeout)
        };

        if (requireAuth && !string.IsNullOrWhiteSpace(bearerToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        else if (requireAuth)
        {
            _logger.LogWarning("Referentiel RequireAuth=true mais BearerToken vide");
        }
        else
        {
            _logger.LogInformation("Référentiel en accès ouvert (pas d'Authorization envoyé)");
        }

        _client = RestService.For<IReferentielServiceClient>(httpClient);

        _logger.LogInformation(
            "Client Référentiel configuré ({Mode}): {BaseAddress}",
            useGateway ? "API Gateway" : "direct",
            baseAddress);
    }

    public Task<List<ReferentielItemDto>> GetPaysAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:pays", () => _client.GetPaysAsync(cancellationToken));

    public Task<List<ReferentielPortDto>> GetPortsAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:ports", () => _client.GetPortsAsync(cancellationToken), TimeSpan.FromMinutes(30));

    public Task<List<ReferentielPortDto>> GetAeroportsAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:aeroports", () => _client.GetAeroportsAsync(cancellationToken), TimeSpan.FromMinutes(30));

    public Task<List<ReferentielItemDto>> GetDevisesAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:devises", () => _client.GetDevisesAsync(cancellationToken));

    public Task<List<ReferentielIncotermDto>> GetIncotermsAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:incoterms", () => _client.GetIncotermsAsync(cancellationToken));

    public Task<List<ReferentielItemDto>> GetDepartementsAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:departements", () => _client.GetDepartementsAsync(cancellationToken));

    public Task<List<ReferentielItemDto>> GetModeDeTransportsAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:modes", () => _client.GetModeDeTransportsAsync(cancellationToken));

    public Task<List<ReferentielItemDto>> GetCorridorsAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:corridors", () => _client.GetCorridorsAsync(cancellationToken));

    public Task<List<ReferentielItemDto>> GetUniteStatistiquesAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:unites", () => _client.GetUniteStatistiquesAsync(cancellationToken));

    private async Task<List<T>> GetCachedAsync<T>(
        string key,
        Func<Task<List<T>>> factory,
        TimeSpan? ttl = null)
    {
        if (_cache.TryGetValue(key, out List<T>? cached) && cached != null)
            return cached;

        var data = await factory();
        _cache.Set(key, data, ttl ?? TimeSpan.FromMinutes(15));
        return data;
    }
}
