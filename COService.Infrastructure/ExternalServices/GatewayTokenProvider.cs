using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Obtient et renouvelle le JWT gateway via /bff/auth/password-login.
/// </summary>
public sealed class GatewayTokenProvider : BackgroundService, IGatewayTokenProvider
{
    private readonly ILogger<GatewayTokenProvider> _logger;
    private readonly GatewayTokenOptions _options;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly HttpClient _loginClient;

    private string? _accessToken;
    private DateTimeOffset _expiresAtUtc = DateTimeOffset.MinValue;
    private bool _loggedMissingCredentials;

    public GatewayTokenProvider(
        ILogger<GatewayTokenProvider> logger,
        IOptions<GatewayTokenOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _options = options.Value;
        _loginClient = httpClientFactory.CreateClient(nameof(GatewayTokenProvider));
    }

    public async Task<string?> GetBearerTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!IsServiceAccountConfigured())
        {
            return StaticBearerToken();
        }

        if (IsTokenValid())
        {
            return _accessToken;
        }

        try
        {
            await RefreshAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible d'obtenir un token gateway — utilisation du fallback statique si disponible.");
        }

        return _accessToken ?? StaticBearerToken();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.ServiceAccount.Enabled)
        {
            if (string.IsNullOrWhiteSpace(StaticBearerToken()))
            {
                _logger.LogWarning(
                    "Gateway : aucun compte de service ni BearerToken statique — Organisation/carnet protégés renverront 401.");
            }
            return;
        }

        if (!IsServiceAccountConfigured())
        {
            LogMissingCredentialsOnce();
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAsync(stoppingToken);

                var refreshAt = _expiresAtUtc.AddSeconds(-_options.ServiceAccount.RefreshBeforeExpirySeconds);
                var delay = refreshAt - DateTimeOffset.UtcNow;
                if (delay < TimeSpan.FromSeconds(30))
                {
                    delay = TimeSpan.FromSeconds(30);
                }

                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec renouvellement token gateway — nouvelle tentative dans 60 s");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (IsTokenValid())
            {
                return;
            }

            if (!_options.ServiceAccount.Enabled)
            {
                return;
            }

            if (!IsServiceAccountConfigured())
            {
                LogMissingCredentialsOnce();
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                throw new InvalidOperationException("ApiGateway:BaseUrl non configuré.");
            }

            var username = _options.ServiceAccount.Username!.Trim();
            var password = _options.ServiceAccount.Password;

            var loginUrl = $"{_options.BaseUrl.TrimEnd('/')}{NormalizeLoginPath(_options.ServiceAccount.LoginPath)}";
            using var response = await _loginClient.PostAsJsonAsync(
                loginUrl,
                new GatewayPasswordLoginRequest { Username = username, Password = password },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Login gateway échoué ({(int)response.StatusCode}) : {body}");
            }

            var payload = await response.Content.ReadFromJsonAsync<GatewayPasswordLoginResponse>(
                cancellationToken: cancellationToken);

            if (payload == null || string.IsNullOrWhiteSpace(payload.AccessToken))
            {
                throw new InvalidOperationException("Login gateway : accessToken absent dans la réponse.");
            }

            _accessToken = payload.AccessToken;
            var lifetimeSeconds = payload.ExpiresInSeconds > 0 ? payload.ExpiresInSeconds : 1800;
            _expiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(lifetimeSeconds);

            _logger.LogInformation(
                "Token gateway renouvelé pour {Username} — expiration dans {Seconds}s",
                username,
                lifetimeSeconds);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private bool IsServiceAccountConfigured()
        => _options.ServiceAccount.Enabled
           && !string.IsNullOrWhiteSpace(_options.ServiceAccount.Username)
           && !string.IsNullOrWhiteSpace(_options.ServiceAccount.Password);

    private void LogMissingCredentialsOnce()
    {
        if (_loggedMissingCredentials)
        {
            return;
        }

        _loggedMissingCredentials = true;
        _logger.LogWarning(
            "ApiGateway:ServiceAccount activé mais Username/Password manquants (Portainer : GATEWAY_SERVICE_PASSWORD). " +
            "Le MS CO démarre sans renouvellement auto — exportateurs/partenaires renverront 401 tant que le mot de passe n'est pas défini.");
    }

    private bool IsTokenValid()
        => !string.IsNullOrWhiteSpace(_accessToken)
           && DateTimeOffset.UtcNow <
           _expiresAtUtc.AddSeconds(-_options.ServiceAccount.RefreshBeforeExpirySeconds);

    private string? StaticBearerToken()
        => !string.IsNullOrWhiteSpace(_options.BearerToken)
            ? _options.BearerToken
            : null;

    private static string NormalizeLoginPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/bff/auth/password-login";
        }

        return path.StartsWith('/') ? path : $"/{path}";
    }

    private sealed class GatewayPasswordLoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private sealed class GatewayPasswordLoginResponse
    {
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expiresInSeconds")]
        public int ExpiresInSeconds { get; set; }
    }
}
