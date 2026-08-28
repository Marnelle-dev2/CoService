using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Client Référentiel en accès direct (:8290).
/// Pays/devises/… ouverts ; carnetadresses nécessite un Bearer si configuré.
/// </summary>
public class ReferentielServiceClientWrapper : IReferentielServiceClient
{
    private readonly ILogger<ReferentielServiceClientWrapper> _logger;
    private readonly IReferentielServiceClient _client;
    private readonly IMemoryCache _cache;

    public ReferentielServiceClientWrapper(
        ILogger<ReferentielServiceClientWrapper> logger,
        IConfiguration configuration,
        IMemoryCache cache,
        IGatewayTokenProvider tokenProvider)
    {
        _logger = logger;
        _cache = cache;

        var serviceConfig = configuration.GetSection("ExternalServices:ReferentielService");
        var useGateway = serviceConfig.GetValue("UseApiGateway", false);
        var timeout = serviceConfig.GetValue("Timeout", 120);

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

        var handler = new GatewayAuthorizationHandler(tokenProvider)
        {
            InnerHandler = new SocketsHttpHandler()
        };

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(timeout)
        };

        _client = RestService.For<IReferentielServiceClient>(httpClient);

        _logger.LogInformation(
            "Client Référentiel configuré ({Mode}): {BaseAddress} — Bearer gateway auto si disponible",
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

    public Task<List<ReferentielEtatDto>> GetEtatsAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:etats", () => _client.GetEtatsAsync(cancellationToken), TimeSpan.FromMinutes(5));

    public Task<List<ReferentielItemDto>> GetBureauxDouanesAsync(CancellationToken cancellationToken = default)
        => GetCachedAsync("ref:bureauxdouanes", () => _client.GetBureauxDouanesAsync(cancellationToken));

    public Task<List<ReferentielPositionTarifaireDto>> GetPositionTarifairesAsync(CancellationToken cancellationToken = default)
        => _client.GetPositionTarifairesAsync(cancellationToken);

    public Task<List<ReferentielCarnetAdresseDto>> GetCarnetAdressesAsync(CancellationToken cancellationToken = default)
        => _client.GetCarnetAdressesAsync(cancellationToken);

    public Task<ReferentielCarnetAdresseDto> GetCarnetAdresseByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _client.GetCarnetAdresseByIdAsync(id, cancellationToken);

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
