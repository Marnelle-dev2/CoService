namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Configuration du token JWT gateway (statique ou renouvelé via compte de service).
/// </summary>
public class GatewayTokenOptions
{
    public const string SectionName = "ApiGateway";

    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Token statique optionnel (fallback si pas de compte de service).</summary>
    public string? BearerToken { get; set; }

    public GatewayServiceAccountOptions ServiceAccount { get; set; } = new();
}

public class GatewayServiceAccountOptions
{
    /// <summary>Active le login password BFF et le renouvellement automatique.</summary>
    public bool Enabled { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Chemin relatif sur ApiGateway:BaseUrl (ex. /bff/auth/password-login).</summary>
    public string LoginPath { get; set; } = "/bff/auth/password-login";

    /// <summary>Renouveler le token N secondes avant expiration (défaut 5 min).</summary>
    public int RefreshBeforeExpirySeconds { get; set; } = 300;
}
