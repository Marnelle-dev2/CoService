using COService.Application.DTOs;

namespace COService.Application.Services;

/// <summary>
/// Service pour la gestion des certificats d'origine
/// </summary>
public interface ICertificatOrigineService
{
    Task<CertificatOrigineDto> CreerCertificatAsync(CreerCertificatOrigineDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<CertificatOrigineDto>> GetAllCertificatsAsync(CancellationToken cancellationToken = default);
    Task<CertificatOrigineDto?> GetCertificatByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CertificatOrigineDto?> GetCertificatByNoAsync(string certificateNo, CancellationToken cancellationToken = default);
    Task<CertificatOrigineDto> ModifierCertificatAsync(Guid id, ModifierCertificatOrigineDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);
    Task SupprimerCertificatAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CertificatOrigineDto>> GetCertificatsByExportateurAsync(string exportateur, CancellationToken cancellationToken = default);
    Task<IEnumerable<CertificatOrigineDto>> GetCertificatsByStatutAsync(string statut, CancellationToken cancellationToken = default);
    Task<IEnumerable<CertificatOrigineDto>> GetCertificatsByPaysDestinationAsync(string paysDestination, CancellationToken cancellationToken = default);
}
