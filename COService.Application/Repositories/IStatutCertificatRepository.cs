using COService.Domain.Entities;

namespace COService.Application.Repositories;

/// <summary>
/// Repository pour les statuts de certificats
/// </summary>
public interface IStatutCertificatRepository
{
    Task<StatutCertificat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StatutCertificat?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<StatutCertificat>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<StatutCertificat> AddAsync(StatutCertificat statut, CancellationToken cancellationToken = default);
}
