using COService.Application.Auth;
using COService.Application.DTOs;
using COService.Shared.Constants;

namespace COService.API.Auth;

/// <summary>
/// Filtrage POC des certificats : exportateur (ses CO) vs CCIAM (soumis à sa chambre, par rôle workflow).
/// </summary>
public static class PocCertificatScope
{
    public static List<CertificatOrigineDto> ApplyListFilter(
        IEnumerable<CertificatOrigineDto> certificats,
        IPocUserContext user)
    {
        if (!user.IsEnabled)
        {
            return certificats.ToList();
        }

        return certificats.Where(c => CanAccessCertificat(c, user)).ToList();
    }

    public static bool CanAccessCertificat(CertificatOrigineDto certificat, IPocUserContext user)
    {
        if (!user.IsEnabled)
        {
            return true;
        }

        return user.Profile switch
        {
            "exportateur" => IsOwnedByExportateur(certificat, user),
            "controleur" or "superviseur" or "president" or "chambre"
                => IsSubmittedToChambre(certificat, user)
                   && StatutsCertificats.EstVisibleParProfilChambre(user.Profile, certificat.EtatCode),
            "transitaire" => StatutsCertificats.EstVisibleParTransitaire(certificat.EtatCode),
            "admin" => true,
            _ => false
        };
    }

    public static bool IsSubmittedToChambre(CertificatOrigineDto certificat, IPocUserContext user)
    {
        if (!StatutsCertificats.EstVisibleParChambre(certificat.EtatCode))
        {
            return false;
        }

        return MatchesOrganisation(
            certificat.PartenaireNIU,
            certificat.PartenaireNom,
            user.OrganisationCode);
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

        if (!string.IsNullOrWhiteSpace(userId)
            && string.Equals(certificat.CreePar, userId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool MatchesOrganisation(string? entityCode, string? entityNom, string? userOrg)
    {
        var org = userOrg?.Trim();
        if (string.IsNullOrWhiteSpace(org))
        {
            // POC CCIAM sans code org explicite : accepter si partenaire renseigné (tests locaux).
            return !string.IsNullOrWhiteSpace(entityCode) || !string.IsNullOrWhiteSpace(entityNom);
        }

        if (!string.IsNullOrWhiteSpace(entityCode)
            && (string.Equals(entityCode, org, StringComparison.OrdinalIgnoreCase)
                || entityCode.Contains(org, StringComparison.OrdinalIgnoreCase)
                || org.Contains(entityCode, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(entityNom)
            && (string.Equals(entityNom, org, StringComparison.OrdinalIgnoreCase)
                || entityNom.Contains(org, StringComparison.OrdinalIgnoreCase)
                || org.Contains(entityNom, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Alias courant POC : organisation CCIAM vs partenaire nommé « Chambre »
        if (org.Contains("CCIAM", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(entityNom)
            && entityNom.Contains("chambre", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
