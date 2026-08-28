using COService.Application.DTOs;

namespace COService.Application.Services;

/// <summary>
/// Lecture des états depuis ReferentielService (source de vérité).
/// </summary>
public interface IReferentielEtatsClient
{
    Task<IReadOnlyList<ReferentielEtatRemoteDto>> GetEtatsAsync(CancellationToken cancellationToken = default);
}
