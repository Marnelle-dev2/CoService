using COService.Application.DTOs;

namespace COService.API.Auth;

/// <summary>
/// Filtre partenaires CO : chambres de commerce uniquement.
/// </summary>
public static class PartenaireFilters
{
    public static bool IsChambreCommerce(PartenaireDto partenaire)
    {
        if (!partenaire.Actif)
        {
            return false;
        }

        var haystack = $"{partenaire.CodePartenaire} {partenaire.Nom} {partenaire.DepartementNom}"
            .ToUpperInvariant();

        return haystack.Contains("CCI")
               || haystack.Contains("CHAMBRE")
               || haystack.Contains("COMMERCE");
    }
}
