using COService.Application.DTOs;

namespace COService.Application.Services;

/// <summary>
/// Service pour la gestion des lignes de certificat
/// </summary>
public interface ICertificatLigneService
{
    Task<CertificatLigneDto> CreerLigneAsync(Guid certificatId, CreerCertificatLigneDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<CertificatLigneDto>> GetLignesByCertificatIdAsync(Guid certificatId, CancellationToken cancellationToken = default);
    Task<CertificatLigneDto?> GetLigneByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CertificatLigneDto> ModifierLigneAsync(Guid id, ModifierCertificatLigneDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);
    Task SupprimerLigneAsync(Guid id, CancellationToken cancellationToken = default);
}
