using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace COService.Infrastructure.Repositories;

/// <summary>
/// Repository certificats — navigations référentiels (Pays, Ports, Module…) ignorées côté EF
/// (codes stockés en clair, données live via MS Référentiel).
/// </summary>
public class CertificatOrigineRepository : Repository<CertificatOrigine>, ICertificatOrigineRepository
{
    public CertificatOrigineRepository(COServiceDbContext context) : base(context)
    {
    }

    public override async Task<CertificatOrigine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await QueryWithLocalIncludes()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CertificatOrigine?> GetByCertificateNoAsync(string certificateNo, CancellationToken cancellationToken = default)
    {
        return await QueryWithLocalIncludes()
            .FirstOrDefaultAsync(c => c.CertificateNo == certificateNo, cancellationToken);
    }

    public async Task<IEnumerable<CertificatOrigine>> GetByExportateurAsync(string exportateur, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ExportateurNom != null && c.ExportateurNom.Contains(exportateur))
            .OrderByDescending(c => c.CreeLe)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CertificatOrigine>> GetByExportateurNIUAsync(string exportateurNIU, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ExportateurNIU == exportateurNIU)
            .OrderByDescending(c => c.CreeLe)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CertificatOrigine>> GetByEtatCodeAsync(string etatCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Etat)
            .Where(c => c.EtatCode == etatCode)
            .OrderByDescending(c => c.CreeLe)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CertificatOrigine>> GetByPaysDestinationAsync(string paysDestination, CancellationToken cancellationToken = default)
    {
        var term = paysDestination.Trim();
        return await _dbSet
            .Where(c => c.PaysDestinationCode != null && c.PaysDestinationCode.Contains(term))
            .OrderByDescending(c => c.CreeLe)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string certificateNo, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.CertificateNo == certificateNo, cancellationToken);
    }

    private IQueryable<CertificatOrigine> QueryWithLocalIncludes() =>
        _dbSet
            .Include(c => c.CertificatLignes)
            .Include(c => c.CertificateValidations)
            .Include(c => c.Commentaires)
            .Include(c => c.Abonnement)
            .Include(c => c.ZoneProduction)
            .Include(c => c.Type)
            .Include(c => c.Etat);
}
