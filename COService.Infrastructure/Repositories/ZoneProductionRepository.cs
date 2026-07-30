using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace COService.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository pour les zones de production
/// </summary>
public class ZoneProductionRepository : Repository<ZoneProduction>, IZoneProductionRepository
{
    public ZoneProductionRepository(COServiceDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<ZoneProduction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(z => z.Nom)
            .ToListAsync(cancellationToken);
    }

    public async Task<ZoneProduction?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(z => z.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<ZoneProduction>> GetByPartenaireNIUAsync(string partenaireNIU, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(z => z.PartenaireNIU == partenaireNIU)
            .OrderBy(z => z.Nom)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(z => z.Id == id, cancellationToken);
    }
}
