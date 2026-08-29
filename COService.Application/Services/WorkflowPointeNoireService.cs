using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Application.Services;
using COService.Application.Messaging;
using COService.Shared.Events;
using COService.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace COService.Application.Services;

/// <summary>
/// Service de workflow spécifique pour la Chambre de Commerce de Pointe-Noire
/// Logique hardcodée selon les spécifications du workflow Pointe-Noire
/// </summary>
internal class WorkflowPointeNoireService : IWorkflowChambreService
{
    private readonly ICertificatOrigineRepository _certificatRepository;
    private readonly IEtatRepository _etatRepository;
    private readonly ICommentaireRepository _commentaireRepository;
    private readonly IAuthService _authService;
    private readonly ICertificateEventPublisher _eventPublisher;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowPointeNoireService(
        ICertificatOrigineRepository certificatRepository,
        IEtatRepository etatRepository,
        ICommentaireRepository commentaireRepository,
        IAuthService authService,
        ICertificateEventPublisher eventPublisher,
        INotificationService notificationService,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<WorkflowService> logger)
    {
        _certificatRepository = certificatRepository;
        _etatRepository = etatRepository;
        _commentaireRepository = commentaireRepository;
        _authService = authService;
        _eventPublisher = eventPublisher;
        _notificationService = notificationService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CertificatOrigineDto> SoumettreCertificatAsync(Guid certificatId, string userId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        // Vérifier que le certificat est au statut Élaboré
        var codeStatutActuel = certificat.EtatCode ?? "NULL";
        if (codeStatutActuel != StatutsCertificats.Elabore)
        {
            throw new InvalidOperationException($"Le certificat doit être au statut 'Élaboré' pour être soumis. Statut actuel: {codeStatutActuel}");
        }

        // Vérifier qu'il y a au moins une ligne dans le certificat
        if (certificat.CertificatLignes == null || !certificat.CertificatLignes.Any())
        {
            throw new InvalidOperationException("Un certificat doit contenir au moins une ligne avant d'être soumis.");
        }

        // Vérifier que l'utilisateur est l'exportateur propriétaire
        // TODO: Vérifier via Auth Service si nécessaire

        // Récupérer le statut "Soumis"
        var etatSoumis = await _etatRepository.GetByCodeAsync(StatutsCertificats.Soumis, cancellationToken)
            ?? throw new InvalidOperationException($"Statut '{StatutsCertificats.Soumis}' introuvable");

        // Effectuer la transition
        certificat.EtatCode = etatSoumis.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Certificat {CertificatId} soumis par l'utilisateur {UserId}", certificatId, userId);

        // Envoyer notification de soumission
        await _notificationService.EnvoyerNotificationSoumissionAsync(certificatId, cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto> ControleCertificatAsync(Guid certificatId, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        // Vérifier que le certificat est au statut Soumis
        if (certificat.EtatCode != StatutsCertificats.Soumis)
        {
            throw new InvalidOperationException($"Le certificat doit être au statut 'Soumis' pour être contrôlé. Statut actuel: {certificat.EtatCode}");
        }

        // Contrôleur uniquement (rôle 3) — le superviseur intervient à l'étape suivante.
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);
        if (!WorkflowRoleRules.PeutControler(roles))
        {
            throw new UnauthorizedAccessException("Seul le Contrôleur (rôle 3) peut contrôler un certificat au stade Visa demandé (VD).");
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Vérifier que l'utilisateur appartient à la chambre de commerce de Pointe-Noire
        if (!string.IsNullOrEmpty(certificat.PartenaireNIU))
        {
            var appartientOrganisation = await _authService.VerifierOrganisationAsync(
                userId, 
                certificat.PartenaireNIU, 
                cancellationToken);
            
            if (!appartientOrganisation)
            {
                throw new UnauthorizedAccessException("L'utilisateur doit appartenir à la chambre de commerce de Pointe-Noire");
            }
        }

        // Récupérer le statut "Contrôlé"
        var etatControle = await _etatRepository.GetByCodeAsync(StatutsCertificats.Controle, cancellationToken)
            ?? throw new InvalidOperationException($"Statut '{StatutsCertificats.Controle}' introuvable");

        // Effectuer la transition
        certificat.EtatCode = etatControle.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Certificat {CertificatId} contrôlé par l'utilisateur {UserId}", certificatId, userId);

        // Envoyer notification de contrôle
        await _notificationService.EnvoyerNotificationControleAsync(certificatId, true, cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto> ApprouverCertificatAsync(Guid certificatId, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        // Vérifier que le certificat est au statut Contrôlé
        if (certificat.EtatCode != StatutsCertificats.Controle)
        {
            _logger.LogWarning("Tentative d'approbation du certificat {CertificatId} avec un statut invalide. Statut actuel: {StatutActuel}", certificatId, certificat.EtatCode);
            throw new InvalidOperationException($"Le certificat doit être au statut 'Contrôlé' (Code: {StatutsCertificats.Controle}) pour être approuvé. Statut actuel: {certificat.EtatCode}");
        }

        // Superviseur uniquement (rôle 4) — après contrôle.
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);
        if (!WorkflowRoleRules.PeutApprouver(roles))
        {
            throw new UnauthorizedAccessException("Seul le Superviseur (rôle 4) peut approuver un certificat contrôlé.");
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Récupérer le statut "Approuvé"
        var etatApprouve = await _etatRepository.GetByCodeAsync(StatutsCertificats.Approuve, cancellationToken)
            ?? throw new InvalidOperationException($"Statut '{StatutsCertificats.Approuve}' introuvable");

        // Effectuer la transition
        certificat.EtatCode = etatApprouve.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Certificat {CertificatId} approuvé par l'utilisateur {UserId}", certificatId, userId);

        // Envoyer notification d'approbation
        await _notificationService.EnvoyerNotificationApprobationAsync(certificatId, cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto> ValiderCertificatAsync(Guid certificatId, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var ancienStatut = certificat.EtatCode ?? string.Empty;

        // Vérifier que le certificat est au statut Approuvé
        if (certificat.EtatCode != StatutsCertificats.Approuve)
        {
            throw new InvalidOperationException($"Le certificat doit être au statut 'Approuvé' pour être validé. Statut actuel: {certificat.EtatCode}");
        }

        // Vérifier le rôle (Président - rôle 6)
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);
        if (!roles.Contains(RolesUtilisateurs.President))
        {
            throw new UnauthorizedAccessException("Seul le Président (rôle 6) peut valider définitivement un certificat");
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Vérifier que l'utilisateur appartient à la même organisation que le certificat
        if (!string.IsNullOrEmpty(certificat.PartenaireNIU))
        {
            var appartientOrganisation = await _authService.VerifierOrganisationAsync(
                userId, 
                certificat.PartenaireNIU, 
                cancellationToken);
            
            if (!appartientOrganisation)
            {
                throw new UnauthorizedAccessException("Le Président doit appartenir à la même organisation que le certificat");
            }
        }

        // Récupérer le statut "Validé"
        var etatValide = await _etatRepository.GetByCodeAsync(StatutsCertificats.Valide, cancellationToken)
            ?? throw new InvalidOperationException($"Statut '{StatutsCertificats.Valide}' introuvable");

        // Effectuer la transition
        certificat.EtatCode = etatValide.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Certificat {CertificatId} validé définitivement par le Président {UserId}", certificatId, userId);

        // Publier l'événement pour la facturation
        await _eventPublisher.PublishCertificatStatutChangeAsync(new CertificatStatutChangeEvent
        {
            CertificatId = certificatId,
            AncienStatut = ancienStatut,
            NouveauStatut = StatutsCertificats.Valide
        }, cancellationToken);

        await _eventPublisher.PublishCertificatValideAsync(new CertificatValideEvent
        {
            CertificatId = certificatId,
            CertificateNo = certificat.CertificateNo,
            ExportateurNIU = certificat.ExportateurNIU,
            PartenaireNIU = certificat.PartenaireNIU
        }, cancellationToken);

        // Envoyer notification de validation
        await _notificationService.EnvoyerNotificationValidationAsync(certificatId, cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto> RejeterCertificatAsync(Guid certificatId, string userId, string password, string commentaire, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(commentaire))
        {
            throw new ArgumentException("Un commentaire est obligatoire pour rejeter un certificat", nameof(commentaire));
        }

        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        // Vérifier que le certificat peut être rejeté (Soumis, Contrôlé ou Approuvé)
        var statutActuel = certificat.EtatCode;
        var peutEtreRejete = statutActuel == StatutsCertificats.Soumis 
                          || statutActuel == StatutsCertificats.Controle 
                          || statutActuel == StatutsCertificats.Approuve;
        
        if (!peutEtreRejete)
        {
            throw new InvalidOperationException($"Le certificat ne peut pas être rejeté depuis le statut '{certificat.EtatCode}'");
        }

        var roles = await _authService.GetRolesAsync(userId, cancellationToken);

        if (!WorkflowRoleRules.PeutRejeter(statutActuel, roles))
        {
            throw new UnauthorizedAccessException(
                "Rejet non autorisé : contrôleur (VD), superviseur (contrôlé) ou président (approuvé) selon l'étape.");
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Récupérer le statut « Modification demandée » (GECO statut 5 → V2 MD 66)
        var etatModification = await _etatRepository.GetByCodeAsync(StatutsCertificats.Modification, cancellationToken)
            ?? throw new InvalidOperationException($"Statut '{StatutsCertificats.Modification}' introuvable");

        // Effectuer la transition
        certificat.EtatCode = etatModification.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);

        // Ajouter un commentaire de rejet
        var commentaireRejet = new Commentaire
        {
            Id = Guid.NewGuid(),
            CertificateId = certificatId,
            CommentaireText = commentaire,
            CreeLe = DateTime.UtcNow,
            CreePar = userId
        };
        await _commentaireRepository.AddAsync(commentaireRejet, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Certificat {CertificatId} renvoyé en modification demandée (MD) par l'utilisateur {UserId} : {Commentaire}",
            certificatId, userId, commentaire);

        // Envoyer notification de rejet
        await _notificationService.EnvoyerNotificationRejetAsync(certificatId, commentaire, cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto> DemanderModificationAsync(Guid certificatId, string userId, string commentaire, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(commentaire))
        {
            throw new ArgumentException("Un commentaire est obligatoire pour demander une modification", nameof(commentaire));
        }

        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        // Vérifier que le certificat est au statut Validé
        if (certificat.EtatCode != StatutsCertificats.Valide)
        {
            throw new InvalidOperationException($"Le certificat doit être au statut 'Validé' pour demander une modification. Statut actuel: {certificat.EtatCode}");
        }

        // Récupérer le statut "Modification"
        var etatModification = await _etatRepository.GetByCodeAsync(StatutsCertificats.Modification, cancellationToken)
            ?? throw new InvalidOperationException($"Statut '{StatutsCertificats.Modification}' introuvable");

        // Effectuer la transition
        certificat.EtatCode = etatModification.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);

        // Ajouter un commentaire
        var commentaireModification = new Commentaire
        {
            Id = Guid.NewGuid(),
            CertificateId = certificatId,
            CommentaireText = commentaire,
            CreeLe = DateTime.UtcNow,
            CreePar = userId
        };
        await _commentaireRepository.AddAsync(commentaireModification, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Modification demandée pour le certificat {CertificatId} par l'utilisateur {UserId}", certificatId, userId);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<bool> EstTransitionValideAsync(Guid certificatId, string codeNouveauStatut, string userId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null) return false;

        var statutActuel = certificat.EtatCode;
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);

        // Logique de validation selon le workflow Pointe-Noire
        return (statutActuel, codeNouveauStatut) switch
        {
            (StatutsCertificats.Elabore, StatutsCertificats.Soumis) => true,
            (StatutsCertificats.Soumis, StatutsCertificats.Controle) => WorkflowRoleRules.PeutControler(roles),
            (StatutsCertificats.Soumis, StatutsCertificats.Modification) => WorkflowRoleRules.PeutControler(roles),
            (StatutsCertificats.Controle, StatutsCertificats.Approuve) => WorkflowRoleRules.PeutApprouver(roles),
            (StatutsCertificats.Controle, StatutsCertificats.Modification) => WorkflowRoleRules.PeutApprouver(roles),
            (StatutsCertificats.Approuve, StatutsCertificats.Valide) => WorkflowRoleRules.PeutValiderFinal(roles),
            (StatutsCertificats.Approuve, StatutsCertificats.Modification) => WorkflowRoleRules.PeutValiderFinal(roles),
            (StatutsCertificats.Valide, StatutsCertificats.Modification) => true,
            _ => false
        };
    }

    public async Task<List<string>> GetTransitionsPossiblesAsync(Guid certificatId, string userId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null) return new List<string>();

        var statutActuel = certificat.EtatCode;
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);
        var transitions = new List<string>();

        // Logique selon le workflow Pointe-Noire
        switch (statutActuel)
        {
            case StatutsCertificats.Elabore:
                transitions.Add(StatutsCertificats.Soumis);
                break;

            case StatutsCertificats.Soumis:
                if (WorkflowRoleRules.PeutControler(roles))
                {
                    transitions.Add(StatutsCertificats.Controle);
                    transitions.Add(StatutsCertificats.Modification);
                }
                break;

            case StatutsCertificats.Controle:
                if (WorkflowRoleRules.PeutApprouver(roles))
                {
                    transitions.Add(StatutsCertificats.Approuve);
                    transitions.Add(StatutsCertificats.Modification);
                }
                break;

            case StatutsCertificats.Approuve:
                if (WorkflowRoleRules.PeutValiderFinal(roles))
                {
                    transitions.Add(StatutsCertificats.Valide);
                    transitions.Add(StatutsCertificats.Modification);
                }
                break;

            case StatutsCertificats.Valide:
                transitions.Add(StatutsCertificats.Modification);
                break;
        }

        return transitions;
    }
}
