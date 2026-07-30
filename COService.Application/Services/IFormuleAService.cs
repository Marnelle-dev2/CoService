using COService.Application.DTOs;

namespace COService.Application.Services;

/// <summary>
/// Service pour la gestion des Formules A (spécifique à Ouesso)
/// </summary>
public interface IFormuleAService
{
    /// <summary>
    /// Crée une Formule A à partir d'un certificat d'origine validé
    /// </summary>
    Task<CertificatOrigineDto> CreerFormuleAAsync(Guid certificatOrigineId, string userId, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Contrôle une Formule A (12 → 13)
    /// </summary>
    Task<CertificatOrigineDto> ControleFormuleAAsync(Guid id, string userId, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approuve une Formule A (13 → 14)
    /// </summary>
    Task<CertificatOrigineDto> ApprouverFormuleAAsync(Guid id, string userId, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valide définitivement une Formule A (14 → 15)
    /// </summary>
    Task<CertificatOrigineDto> ValiderFormuleAAsync(Guid id, string userId, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejette une Formule A (vers Rejeté)
    /// </summary>
    Task<CertificatOrigineDto> RejeterFormuleAAsync(Guid id, string userId, string password, string commentaire, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si on peut créer une Formule A depuis un certificat
    /// </summary>
    Task<bool> PeutCreerFormuleAAsync(Guid certificatId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si une chambre de commerce est autorisée à délivrer des Formules A
    /// </summary>
    Task<bool> VerifierChambreAutoriseeFormuleAAsync(string partenaireNIU, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un exportateur est autorisé à créer une Formule A
    /// </summary>
    Task<bool> VerifierAutorisationFormuleAAsync(Guid certificatId, string exportateurNIU, CancellationToken cancellationToken = default);
}
