namespace COService.Application.Services;

/// <summary>
/// Interface pour le service d'authentification (abstraction)
/// </summary>
public interface IAuthService
{
    Task<UserInfo> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> VerifierRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task<List<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> VerifierMotDePasseAsync(string userId, string password, CancellationToken cancellationToken = default);
    Task<bool> VerifierOrganisationAsync(string userId, string organisationCode, CancellationToken cancellationToken = default);
}

/// <summary>
/// Informations utilisateur
/// </summary>
public class UserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? OrganisationId { get; set; }
    public string? OrganisationCode { get; set; }
    public List<string> Roles { get; set; } = new();
}
