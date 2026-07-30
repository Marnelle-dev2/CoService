using Refit;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Interface Refit pour le microservice d'authentification
/// </summary>
public interface IAuthServiceClient
{
    /// <summary>
    /// Récupère les informations d'un utilisateur
    /// </summary>
    [Get("/api/users/{userId}")]
    Task<UserInfoDto> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un utilisateur a un rôle spécifique
    /// </summary>
    [Get("/api/users/{userId}/roles/{role}/verify")]
    Task<bool> VerifierRoleAsync(string userId, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère tous les rôles d'un utilisateur
    /// </summary>
    [Get("/api/users/{userId}/roles")]
    Task<List<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie le mot de passe d'un utilisateur
    /// </summary>
    [Post("/api/users/{userId}/verify-password")]
    Task<bool> VerifierMotDePasseAsync(string userId, [Body] VerifyPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un utilisateur appartient à une organisation
    /// </summary>
    [Get("/api/users/{userId}/organisations/{organisationCode}/verify")]
    Task<bool> VerifierOrganisationAsync(string userId, string organisationCode, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO pour les informations utilisateur
/// </summary>
public class UserInfoDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? OrganisationId { get; set; }
    public string? OrganisationCode { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// DTO pour la vérification de mot de passe
/// </summary>
public class VerifyPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}
