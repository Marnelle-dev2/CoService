namespace COService.Application.Auth;

/// <summary>
/// Contexte utilisateur POC (en-têtes gateway/client) en attendant le microservice Auth.
/// </summary>
public interface IPocUserContext
{
    bool IsEnabled { get; }

    string? UserId { get; }

    string? OrganisationCode { get; }

    /// <summary>exportateur | chambre | transitaire | admin | lecteur</summary>
    string Profile { get; }

    bool CanReadCertificats { get; }

    bool CanCreateCertificat { get; }

    bool CanModifyCertificat { get; }

    bool CanValidateCertificat { get; }

    /// <summary>Voir tous les certificats (chambre, admin) vs filtrés (exportateur).</summary>
    bool CanViewAllCertificats { get; }
}
