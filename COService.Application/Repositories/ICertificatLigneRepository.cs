using COService.Domain.Entities;

namespace COService.Application.Repositories;

/// <summary>
/// Repository pour les lignes de certificat
/// </summary>
public interface ICertificatLigneRepository
{
    Task<CertificatLigne?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CertificatLigne>> GetByCertificatIdAsync(Guid certificatId, CancellationToken cancellationToken = default);
    Task<CertificatLigne> AddAsync(CertificatLigne ligne, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<CertificatLigne> lignes, CancellationToken cancellationToken = default);
    void Update(CertificatLigne ligne);
    void Remove(CertificatLigne ligne);
    void RemoveRange(IEnumerable<CertificatLigne> lignes);
}
