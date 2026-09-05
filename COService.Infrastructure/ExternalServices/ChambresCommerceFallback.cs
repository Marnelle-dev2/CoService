using COService.Application.DTOs;
using COService.Shared.Constants;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Chambres CCIAM de secours quand Acteurs / Organisation ne renvoient pas de PARTENAIRE.
/// </summary>
public static class ChambresCommerceFallback
{
    public static IReadOnlyList<OrganisationRemoteDto> List { get; } =
    [
        new OrganisationRemoteDto
        {
            Code = ChambresCommerce.PointeNoire.CodePartenaire,
            Name = ChambresCommerce.PointeNoire.Nom,
            Sigle = "CCIAM-PNR",
            Type = "PARTENAIRE",
            Departement = ChambresCommerce.PointeNoire.CodeDepartement,
            IsActive = true
        },
        new OrganisationRemoteDto
        {
            Code = ChambresCommerce.Ouesso.CodePartenaire,
            Name = ChambresCommerce.Ouesso.Nom,
            Sigle = "CCIAM-OUE",
            Type = "PARTENAIRE",
            Departement = ChambresCommerce.Ouesso.CodeDepartement,
            IsActive = true
        }
    ];

    public static OrganisationRemoteDto? FindByCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return List.FirstOrDefault(c =>
            string.Equals(c.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
