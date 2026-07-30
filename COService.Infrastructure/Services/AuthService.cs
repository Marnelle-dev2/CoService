using COService.Application.Services;
using COService.Infrastructure.ExternalServices;

namespace COService.Infrastructure.Services;

/// <summary>
/// Implémentation du service d'authentification utilisant le client API
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAuthServiceClient _authServiceClient;

    public AuthService(IAuthServiceClient authServiceClient)
    {
        _authServiceClient = authServiceClient;
    }

    public async Task<UserInfo> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userInfoDto = await _authServiceClient.GetUserInfoAsync(userId, cancellationToken);
        return new UserInfo
        {
            UserId = userInfoDto.UserId,
            Username = userInfoDto.Username,
            Email = userInfoDto.Email,
            OrganisationId = userInfoDto.OrganisationId,
            OrganisationCode = userInfoDto.OrganisationCode,
            Roles = userInfoDto.Roles
        };
    }

    public async Task<bool> VerifierRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        return await _authServiceClient.VerifierRoleAsync(userId, role, cancellationToken);
    }

    public async Task<List<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _authServiceClient.GetRolesAsync(userId, cancellationToken);
    }

    public async Task<bool> VerifierMotDePasseAsync(string userId, string password, CancellationToken cancellationToken = default)
    {
        var request = new VerifyPasswordRequest { Password = password };
        return await _authServiceClient.VerifierMotDePasseAsync(userId, request, cancellationToken);
    }

    public async Task<bool> VerifierOrganisationAsync(string userId, string organisationCode, CancellationToken cancellationToken = default)
    {
        return await _authServiceClient.VerifierOrganisationAsync(userId, organisationCode, cancellationToken);
    }
}
