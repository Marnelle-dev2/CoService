using COService.Domain.Entities;

namespace COService.Application.Repositories;

/// <summary>
/// Repository pour les types de partenaires
/// </summary>
public interface ITypePartenaireRepository
{
    Task<TypePartenaire?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TypePartenaire?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<TypePartenaire>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TypePartenaire>> GetActifsAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<TypePartenaire> AddAsync(TypePartenaire typePartenaire, CancellationToken cancellationToken = default);
}
