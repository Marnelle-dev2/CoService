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
