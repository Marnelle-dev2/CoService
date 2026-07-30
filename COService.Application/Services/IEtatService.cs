using COService.Application.DTOs;

namespace COService.Application.Services;

public interface IEtatService
{
    Task<IEnumerable<EtatDto>> GetAllEtatsAsync(CancellationToken cancellationToken = default);
    Task<EtatDto?> GetEtatByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EtatDto?> GetEtatByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<EtatDto> CreerEtatAsync(CreerEtatDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<EtatDto>> SeedEtatsWorkflowAsync(string? utilisateur = null, CancellationToken cancellationToken = default);
}
