using COService.Application.DTOs;
using Refit;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Client MS Organisation via Gateway.
/// BaseAddress = ApiGateway:BaseUrl (ex. http://srv-guot-cont.gumar.local:5000)
/// </summary>
public interface IEnrolementServiceClient
{
    [Get("/organisation/Organisations")]
    Task<List<OrganisationRemoteDto>> GetAllOrganisationsAsync(CancellationToken cancellationToken = default);

    [Get("/organisation/Organisations/type/{type}")]
    Task<List<OrganisationRemoteDto>> GetOrganisationsByTypeAsync(string type, CancellationToken cancellationToken = default);

    [Get("/organisation/Organisations/{code}")]
    Task<OrganisationRemoteDto> GetOrganisationByCodeAsync(string code, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrat renvoyé par le MS Organisation.
/// Identification par <see cref="Code"/> (pas d'id GUID distant).
/// </summary>
public class OrganisationRemoteDto
{
    public string Code { get; set; } = string.Empty;
    public string? Sigle { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Niu { get; set; }
    public string? Adresse { get; set; }
    public string? Pays { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? Departement { get; set; }
    public bool IsActive { get; set; }
}

public static class OrganisationRemoteMapper
{
    public static Guid IdFromCode(string code)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes($"organisation:{code}"));
        return new Guid(bytes);
    }

    public static ExportateurDto ToExportateur(OrganisationRemoteDto o) => new()
    {
        Id = IdFromCode(o.Code),
        CodeExportateur = o.Code,
        Nom = o.Name,
        RaisonSociale = o.Sigle,
        NIU = o.Niu,
        Adresse = o.Adresse,
        Telephone = o.Telephone,
        Email = o.Email,
        Actif = o.IsActive,
        DepartementNom = o.Departement
    };

    public static PartenaireDto ToPartenaire(OrganisationRemoteDto o) => new()
    {
        Id = IdFromCode(o.Code),
        CodePartenaire = o.Code,
        Nom = o.Name,
        Adresse = o.Adresse,
        Telephone = o.Telephone,
        Email = o.Email,
        Actif = o.IsActive,
        DepartementNom = o.Departement
    };
}
