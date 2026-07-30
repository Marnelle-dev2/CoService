using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace COService.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository pour les lignes de certificat
/// </summary>
public class CertificatLigneRepository : Repository<CertificatLigne>, ICertificatLigneRepository
{
    public CertificatLigneRepository(COServiceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CertificatLigne>> GetByCertificatIdAsync(Guid certificatId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cl => cl.CertificatId == certificatId)
            .ToListAsync(cancellationToken);
    }

    public override async Task AddRangeAsync(IEnumerable<CertificatLigne> lignes, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(lignes, cancellationToken);
    }

    public override void RemoveRange(IEnumerable<CertificatLigne> lignes)
    {
        _dbSet.RemoveRange(lignes);
    }
}
