using COService.Application.DTOs;

namespace COService.Application.Services;

/// <summary>
/// Service pour la gestion des zones de production
/// Gérées localement par COService
/// </summary>
public interface IZoneProductionService
{
    Task<ZoneProductionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ZoneProductionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<ZoneProductionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ZoneProductionDto>> GetByPartenaireNIUAsync(string partenaireNIU, CancellationToken cancellationToken = default);
    Task<ZoneProductionDto> CreerZoneProductionAsync(CreerZoneProductionDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);
    Task<ZoneProductionDto> ModifierZoneProductionAsync(Guid id, ModifierZoneProductionDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);
    Task SupprimerZoneProductionAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO pour modifier une zone de production
/// </summary>
public class ModifierZoneProductionDto
{
    public string? Nom { get; set; }
    public string? Description { get; set; }
    public string? PartenaireNIU { get; set; }
}
