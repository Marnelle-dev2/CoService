using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace COService.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository pour les certificats d'origine
/// </summary>
public class CertificatOrigineRepository : Repository<CertificatOrigine>, ICertificatOrigineRepository
{
    public CertificatOrigineRepository(COServiceDbContext context) : base(context)
    {
    }

    public override async Task<CertificatOrigine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.CertificatLignes)
            .Include(c => c.CertificateValidations)
            .Include(c => c.Commentaires)
            .Include(c => c.Abonnement)
            .Include(c => c.PaysDestination)
            .Include(c => c.PortSortie)
            .Include(c => c.PortCongo)
            .Include(c => c.ZoneProduction)
            .Include(c => c.BureauDedouanement)
            .Include(c => c.Module)
            .Include(c => c.Devise)
            .Include(c => c.Type)
            .Include(c => c.Etat)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CertificatOrigine?> GetByCertificateNoAsync(string certificateNo, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.CertificatLignes)
            .Include(c => c.CertificateValidations)
            .Include(c => c.Commentaires)
            .Include(c => c.Abonnement)
            .Include(c => c.PaysDestination)
            .Include(c => c.PortSortie)
            .Include(c => c.PortCongo)
            .Include(c => c.ZoneProduction)
            .Include(c => c.BureauDedouanement)
            .Include(c => c.Module)
            .Include(c => c.Devise)
            .Include(c => c.Type)
            .Include(c => c.Etat)
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
        return await _dbSet
            .Include(c => c.PaysDestination)
            .Where(c => c.PaysDestination != null && c.PaysDestination.Nom.Contains(paysDestination))
            .OrderByDescending(c => c.CreeLe)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string certificateNo, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.CertificateNo == certificateNo, cancellationToken);
    }
}
