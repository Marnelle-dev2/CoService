using COService.Application.Auth;
using COService.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Wrapper Auth Service — en BypassMode, les rôles POC suivent X-Poc-Profile (CCIAM).
/// </summary>
public class AuthServiceClientWrapper : IAuthServiceClient
{
    private readonly ILogger<AuthServiceClientWrapper> _logger;
    private readonly IAuthServiceClient? _client;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly bool _bypassMode;

    public AuthServiceClientWrapper(
        ILogger<AuthServiceClientWrapper> logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        var authConfig = configuration.GetSection("ExternalServices:AuthService");
        _bypassMode = authConfig.GetValue<bool>("BypassMode", false);

        _logger.LogInformation("Configuration AuthService - BypassMode: {BypassMode}", _bypassMode);

        if (_bypassMode)
        {
            _logger.LogWarning("MODE BYPASS AuthService : rôles dérivés du profil POC (X-Poc-Profile).");
            _client = null;
            return;
        }

        var apiGatewayUrl = configuration.GetValue<string>("ApiGateway:BaseUrl")
            ?? throw new InvalidOperationException("ApiGateway:BaseUrl non configuré");

        var authPath = authConfig.GetValue<string>("Path") ?? "/api/auth";
        var timeout = authConfig.GetValue<int>("Timeout", 30);
        var baseAddress = $"{apiGatewayUrl.TrimEnd('/')}{authPath}";

        _client = RestService.For<IAuthServiceClient>(
            new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(timeout)
            });

        _logger.LogInformation("Client Auth Service configuré via API Gateway: {BaseAddress}", baseAddress);
    }

    public async Task<UserInfoDto> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (_bypassMode)
        {
            var roles = ResolvePocRoles();
            return new UserInfoDto
            {
                UserId = userId,
                Username = userId,
                Email = $"{userId}@poc.local",
                OrganisationId = Guid.Empty,
                OrganisationCode = ResolveOrganisationCode(),
                Roles = roles
            };
        }

        try
        {
            return await _client!.GetUserInfoAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des informations utilisateur {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> VerifierRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        if (_bypassMode)
        {
            var roles = ResolvePocRoles();
            var ok = roles.Contains(role, StringComparer.OrdinalIgnoreCase);
            _logger.LogDebug("POC bypass VerifierRole {UserId}/{Role} => {Ok} (profil roles=[{Roles}])",
                userId, role, ok, string.Join(',', roles));
            return ok;
        }

        try
        {
            return await _client!.VerifierRoleAsync(userId, role, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification du rôle {Role} pour l'utilisateur {UserId}", role, userId);
            return false;
        }
    }

    public async Task<List<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (_bypassMode || _client == null)
        {
            var roles = ResolvePocRoles();
            _logger.LogInformation("POC bypass GetRoles {UserId} => [{Roles}]", userId, string.Join(',', roles));
            return roles;
        }

        try
        {
            return await _client.GetRolesAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur GetRoles {UserId} — fallback profil POC.", userId);
            return ResolvePocRoles();
        }
    }

    public async Task<bool> VerifierMotDePasseAsync(string userId, VerifyPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (_bypassMode || _client == null)
        {
            // POC : tout mot de passe non vide accepté (les écrans CCIAM enverront un MDP de confirmation).
            return !string.IsNullOrWhiteSpace(request.Password);
        }

        try
        {
            return await _client.VerifierMotDePasseAsync(userId, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur VerifierMotDePasse {UserId} — accepté en fallback POC.", userId);
            return !string.IsNullOrWhiteSpace(request.Password);
        }
    }

    public async Task<bool> VerifierOrganisationAsync(string userId, string organisationCode, CancellationToken cancellationToken = default)
    {
        if (_bypassMode || _client == null)
        {
            return true;
        }

        try
        {
            return await _client.VerifierOrganisationAsync(userId, organisationCode, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur VerifierOrganisation {UserId}/{OrganisationCode}", userId, organisationCode);
            return true;
        }
    }

    private List<string> ResolvePocRoles()
    {
        var profile = ResolveCurrentProfile();
        return profile switch
        {
            "controleur" => new List<string> { RolesUtilisateurs.Controleur },
            "superviseur" => new List<string> { RolesUtilisateurs.Superviseur, RolesUtilisateurs.Controleur },
            "president" => new List<string> { RolesUtilisateurs.President },
            // Chambre générique : tous les rôles CCIAM (pratique pour tests rapides)
            "chambre" => new List<string>
            {
                RolesUtilisateurs.Controleur,
                RolesUtilisateurs.Superviseur,
                RolesUtilisateurs.President
            },
            "admin" => new List<string>
            {
                RolesUtilisateurs.Controleur,
                RolesUtilisateurs.Superviseur,
                RolesUtilisateurs.President,
                RolesUtilisateurs.Exportateur
            },
            "exportateur" => new List<string> { RolesUtilisateurs.Exportateur },
            _ => new List<string>()
        };
    }

    private string ResolveCurrentProfile()
    {
        var http = _httpContextAccessor.HttpContext;
        if (http?.Items.TryGetValue(nameof(IPocUserContext), out var raw) == true
            && raw is IPocUserContext poc
            && !string.IsNullOrWhiteSpace(poc.Profile))
        {
            return poc.Profile.Trim().ToLowerInvariant();
        }

        var header = http?.Request.Headers["X-Poc-Profile"].FirstOrDefault();
        return string.IsNullOrWhiteSpace(header) ? "lecteur" : header.Trim().ToLowerInvariant();
    }

    private string ResolveOrganisationCode()
    {
        var http = _httpContextAccessor.HttpContext;
        if (http?.Items.TryGetValue(nameof(IPocUserContext), out var raw) == true
            && raw is IPocUserContext poc
            && !string.IsNullOrWhiteSpace(poc.OrganisationCode))
        {
            return poc.OrganisationCode;
        }

        return http?.Request.Headers["X-Organisation-Code"].FirstOrDefault() ?? "CCIAM";
    }
}
