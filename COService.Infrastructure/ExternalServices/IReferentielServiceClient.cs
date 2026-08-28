using Refit;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Client MS Référentiel (accès direct, sans auth).
/// BaseAddress = ExternalServices:ReferentielService:BaseUrl
/// ex. http://srv-guot-cont.gumar.local:8290
/// </summary>
public interface IReferentielServiceClient
{
    [Get("/api/pays")]
    Task<List<ReferentielItemDto>> GetPaysAsync(CancellationToken cancellationToken = default);

    [Get("/api/ports")]
    Task<List<ReferentielPortDto>> GetPortsAsync(CancellationToken cancellationToken = default);

    [Get("/api/aeroports")]
    Task<List<ReferentielPortDto>> GetAeroportsAsync(CancellationToken cancellationToken = default);

    [Get("/api/devises")]
    Task<List<ReferentielItemDto>> GetDevisesAsync(CancellationToken cancellationToken = default);

    [Get("/api/incoterms")]
    Task<List<ReferentielIncotermDto>> GetIncotermsAsync(CancellationToken cancellationToken = default);

    [Get("/api/departements")]
    Task<List<ReferentielItemDto>> GetDepartementsAsync(CancellationToken cancellationToken = default);

    [Get("/api/ModeDeTransports")]
    Task<List<ReferentielItemDto>> GetModeDeTransportsAsync(CancellationToken cancellationToken = default);

    [Get("/api/corridors")]
    Task<List<ReferentielItemDto>> GetCorridorsAsync(CancellationToken cancellationToken = default);

    [Get("/api/UniteStatistiques")]
    Task<List<ReferentielItemDto>> GetUniteStatistiquesAsync(CancellationToken cancellationToken = default);

    [Get("/api/etats")]
    Task<List<ReferentielEtatDto>> GetEtatsAsync(CancellationToken cancellationToken = default);

    [Get("/api/carnetadresses")]
    Task<List<ReferentielCarnetAdresseDto>> GetCarnetAdressesAsync(CancellationToken cancellationToken = default);

    [Get("/api/carnetadresses/{id}")]
    Task<ReferentielCarnetAdresseDto> GetCarnetAdresseByIdAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/bureauxdouanes")]
    Task<List<ReferentielItemDto>> GetBureauxDouanesAsync(CancellationToken cancellationToken = default);

    [Get("/api/positiontarifaires")]
    Task<List<ReferentielPositionTarifaireDto>> GetPositionTarifairesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO MS Référentiel /api/carnetadresses — scoped par organisation.
/// </summary>
public class ReferentielCarnetAdresseDto
{
    public Guid Id { get; set; }
    public string? Organisation { get; set; }
    public string? Nom { get; set; }
    public string? Niu { get; set; }
    public string? Adresse { get; set; }
    public string? Pays { get; set; }
    public string? Coordonnees { get; set; }
    public string? RaisonSociale { get; set; }
    public string? NoRC { get; set; }
    public string? NoCodeActivite { get; set; }
    public bool Actif { get; set; } = true;
    public string? CreerPar { get; set; }
    public string? ModifierPar { get; set; }
    public DateTime? CreerLe { get; set; }
    public DateTime? ModifierLe { get; set; }
}

/// <summary>
/// DTO MS Référentiel /api/etats (schéma V2 : Code int, UsageUI).
/// </summary>
public class ReferentielEtatDto
{
    public Guid Id { get; set; }
    /// <summary>Code métier numérique (42, 79, …).</summary>
    public int? Code { get; set; }
    public string? Libelle { get; set; }
    public string? Description { get; set; }
    public string? CodeEcran { get; set; }
    public string? UsageUI { get; set; }
    public string? Domaine { get; set; }
    public string? TypeEtat { get; set; }
    public bool Actif { get; set; } = true;
    public string? CreerPar { get; set; }
    public string? ModifierPar { get; set; }
    public DateTime? CreerLe { get; set; }
    public DateTime? ModifierLe { get; set; }
}

public class ReferentielItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Nom { get; set; }
    public bool Actif { get; set; }
    public string? CreerPar { get; set; }
    public string? ModifierPar { get; set; }
    public DateTime? CreerLe { get; set; }
    public DateTime? ModifierLe { get; set; }
}

public class ReferentielPortDto : ReferentielItemDto
{
    public Guid? PaysId { get; set; }
    public string? Type { get; set; }
}

public class ReferentielIncotermDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Actif { get; set; }
}

public class ReferentielPositionTarifaireDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Regime { get; set; }
    public Guid? UniteStatistiqueId { get; set; }
    public bool Actif { get; set; } = true;
}
