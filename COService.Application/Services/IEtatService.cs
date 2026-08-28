using COService.Application.DTOs;

namespace COService.Application.Services;

public interface IEtatService
{
    Task<IEnumerable<EtatDto>> GetAllEtatsAsync(CancellationToken cancellationToken = default);
    Task<EtatDto?> GetEtatByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EtatDto?> GetEtatByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<EtatDto> CreerEtatAsync(CreerEtatDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bootstrap local (noyau V2 + états CO) si le référentiel n'est pas encore alimenté.
    /// </summary>
    Task<IEnumerable<EtatDto>> SeedEtatsWorkflowAsync(string? utilisateur = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronise la copie locale depuis ReferentielService (/api/etats).
    /// </summary>
    Task<SyncEtatsResultDto> SyncFromReferentielAsync(CancellationToken cancellationToken = default);
}
