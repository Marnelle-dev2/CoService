using COService.Application.DTOs;

namespace COService.Application.Services;

public interface IStatutCertificatService
{
    Task<IEnumerable<StatutCertificatDto>> GetAllStatutsAsync(CancellationToken cancellationToken = default);
    Task<StatutCertificatDto?> GetStatutByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StatutCertificatDto?> GetStatutByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<StatutCertificatDto> CreerStatutAsync(CreerStatutCertificatDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<StatutCertificatDto>> SeedStatutsWorkflowAsync(string? utilisateur = null, CancellationToken cancellationToken = default);
}
