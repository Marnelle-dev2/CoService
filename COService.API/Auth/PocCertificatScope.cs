using COService.Application.Auth;
using COService.Application.DTOs;

namespace COService.API.Auth;

/// <summary>
/// Filtrage POC exportateur : organisation + certificats créés par l'utilisateur.
/// </summary>
public static class PocCertificatScope
{
    public static List<CertificatOrigineDto> ApplyListFilter(
        IEnumerable<CertificatOrigineDto> certificats,
        IPocUserContext user)
    {
        if (!user.IsEnabled || user.CanViewAllCertificats || user.Profile != "exportateur")
        {
            return certificats.ToList();
        }

        return certificats.Where(c => IsOwnedByExportateur(c, user)).ToList();
    }

    public static bool IsOwnedByExportateur(CertificatOrigineDto certificat, IPocUserContext user)
    {
        var org = user.OrganisationCode?.Trim();
        var userId = user.UserId?.Trim();

        if (!string.IsNullOrWhiteSpace(org))
        {
            if (string.Equals(certificat.ExportateurNIU, org, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(certificat.ExportateurNom)
                && (string.Equals(certificat.ExportateurNom, org, StringComparison.OrdinalIgnoreCase)
                    || certificat.ExportateurNom.Contains(org, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        // Certificats POC créés sans exportateur renseigné mais par ce compte
        if (!string.IsNullOrWhiteSpace(userId)
            && string.Equals(certificat.CreePar, userId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
