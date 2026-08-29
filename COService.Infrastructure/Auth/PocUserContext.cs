using COService.Application.Auth;

namespace COService.Infrastructure.Auth;

public sealed class PocUserContext : IPocUserContext
{
    public bool IsEnabled { get; init; }

    public string? UserId { get; init; }

    public string? OrganisationCode { get; init; }

    public string Profile { get; init; } = "lecteur";

    public bool CanReadCertificats =>
        !IsEnabled || Profile is
            "exportateur" or "chambre" or "controleur" or "superviseur" or "president"
            or "transitaire" or "admin";

    public bool CanCreateCertificat =>
        !IsEnabled || Profile is "exportateur" or "admin";

    public bool CanModifyCertificat =>
        !IsEnabled || Profile is "exportateur" or "admin";

    public bool CanValidateCertificat =>
        !IsEnabled || Profile is
            "chambre" or "controleur" or "superviseur" or "president" or "admin";

    public bool CanViewAllCertificats =>
        !IsEnabled || Profile is
            "chambre" or "controleur" or "superviseur" or "president"
            or "transitaire" or "admin";
}
