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
            // GECO ListeCertificat : exportateur = mandataire, tous les statuts.
            "exportateur" => IsVisibleForExportateur(certificat, user),
            "controleur" or "superviseur" or "president" or "chambre"
                => IsSubmittedToChambre(certificat, user)
                   && StatutsCertificats.EstVisibleParProfilChambre(user.Profile, certificat.EtatCode),
            "transitaire" => StatutsCertificats.EstVisibleParTransitaire(certificat.EtatCode),
            "admin" => true,
            _ => false
        };
    }

    /// <summary>
    /// Exportateur : tous ses CO (tous états). Aligné GECO mandataire = organisation exportateur.
    /// </summary>
    public static bool IsVisibleForExportateur(CertificatOrigineDto certificat, IPocUserContext user)
    {
        if (IsOwnedByExportateur(certificat, user))
        {
            return true;
        }

        // POC mono-exportateur : org simulée EXPGLOBAL → voir tous les CO avec exportateur renseigné.
        if (IsGenericExportateurOrganisation(user.OrganisationCode)
            && (!string.IsNullOrWhiteSpace(certificat.ExportateurNIU)
                || !string.IsNullOrWhiteSpace(certificat.ExportateurNom)))
        {
            return true;
        }

        return false;
    }

    private static bool IsGenericExportateurOrganisation(string? organisationCode)
    {
        var org = organisationCode?.Trim();
        if (string.IsNullOrWhiteSpace(org))
        {
            return false;
        }

        return org.Contains("EXP", StringComparison.OrdinalIgnoreCase)
            || org.Contains("GLOBAL", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsOwnedByExportateur(CertificatOrigineDto certificat, IPocUserContext user)
    {
        var org = user.OrganisationCode?.Trim();
        var userId = user.UserId?.Trim();

        if (MatchesCreator(certificat, userId))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(org))
        {
            if (OrgMatches(certificat.ExportateurNIU, org) || OrgMatches(certificat.ExportateurNom, org))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesCreator(CertificatOrigineDto certificat, string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        return string.Equals(certificat.CreePar, userId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(certificat.ModifiePar, userId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool OrgMatches(string? value, string org)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var v = value.Trim();
        return string.Equals(v, org, StringComparison.OrdinalIgnoreCase)
            || v.Contains(org, StringComparison.OrdinalIgnoreCase)
            || org.Contains(v, StringComparison.OrdinalIgnoreCase);
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
