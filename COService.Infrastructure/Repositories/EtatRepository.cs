using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace COService.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository pour les états (statuts) de certificats
/// </summary>
public class EtatRepository : Repository<Etat>, IEtatRepository
{
    public EtatRepository(COServiceDbContext context) : base(context)
    {
    }

    public async Task<Etat?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.Code == code, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(e => e.Code == code, cancellationToken);
    }
}
