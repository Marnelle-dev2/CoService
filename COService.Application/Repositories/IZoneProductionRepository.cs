using COService.Domain.Entities;

namespace COService.Application.Repositories;

/// <summary>
/// Repository pour les zones de production
/// Gérées localement par COService
/// </summary>
public interface IZoneProductionRepository
{
    Task<ZoneProduction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ZoneProduction?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<ZoneProduction>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ZoneProduction>> GetByPartenaireNIUAsync(string partenaireNIU, CancellationToken cancellationToken = default);
    Task<ZoneProduction> AddAsync(ZoneProduction zoneProduction, CancellationToken cancellationToken = default);
    void Update(ZoneProduction zoneProduction);
    void Remove(ZoneProduction zoneProduction);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
