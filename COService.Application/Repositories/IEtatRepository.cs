using COService.Domain.Entities;

namespace COService.Application.Repositories;

/// <summary>
/// Repository pour les états (statuts) de certificats
/// </summary>
public interface IEtatRepository
{
    Task<Etat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Etat?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Etat>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<Etat> AddAsync(Etat etat, CancellationToken cancellationToken = default);
    void Update(Etat etat);
}
