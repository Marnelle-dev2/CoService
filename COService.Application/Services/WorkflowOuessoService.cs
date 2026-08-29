using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Application.Services;
using COService.Application.Messaging;
using COService.Shared.Events;
using COService.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace COService.Application.Services;

/// <summary>
/// Service de workflow spécifique pour la Chambre de Commerce d'Ouesso
/// Logique hardcodée selon les spécifications du workflow Ouesso
/// </summary>
internal class WorkflowOuessoService : IWorkflowChambreService
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

    public WorkflowOuessoService(
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

        // Vérifier que le certificat appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            throw new InvalidOperationException("Ce certificat n'appartient pas à la chambre de commerce d'Ouesso");
        }

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

        // Récupérer le statut "Soumis"
        var etatSoumis = await _etatRepository.GetByCodeAsync(StatutsCertificats.Soumis, cancellationToken)
            ?? throw new InvalidOperationException($"Statut '{StatutsCertificats.Soumis}' introuvable");

        // Effectuer la transition
        certificat.EtatCode = etatSoumis.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publier l'événement de changement de statut
        await _eventPublisher.PublishCertificatStatutChangeAsync(new CertificatStatutChangeEvent
        {
            CertificatId = certificatId,
            AncienStatut = StatutsCertificats.Elabore,
            NouveauStatut = StatutsCertificats.Soumis
        }, cancellationToken);

        _logger.LogInformation("Certificat {CertificatId} soumis par l'utilisateur {UserId} (Ouesso)", certificatId, userId);

        // Envoyer notification de soumission
        await _notificationService.EnvoyerNotificationSoumissionAsync(certificatId, cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto> ControleCertificatAsync(Guid certificatId, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        // Vérifier que le certificat appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            throw new InvalidOperationException("Ce certificat n'appartient pas à la chambre de commerce d'Ouesso");
        }

        // Vérifier que le certificat est au statut Soumis
        if (certificat.EtatCode != StatutsCertificats.Soumis)
        {
            throw new InvalidOperationException($"Le certificat doit être au statut 'Soumis' pour être contrôlé. Statut actuel: {certificat.EtatCode}");
        }

        // Vérifier le rôle (Contrôleur ou Superviseur - rôles 3 ou 4)
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);
        var peutControler = roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur);
        
        if (!peutControler)
        {
            throw new UnauthorizedAccessException("Seuls les Contrôleurs (rôle 3) et Superviseurs (rôle 4) peuvent contrôler un certificat");
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Vérifier que l'utilisateur appartient à la chambre de commerce d'Ouesso
        if (!string.IsNullOrEmpty(certificat.PartenaireNIU))
        {
            var appartientOrganisation = await _authService.VerifierOrganisationAsync(
                userId, 
                certificat.PartenaireNIU, 
                cancellationToken);
            
            if (!appartientOrganisation)
            {
                throw new UnauthorizedAccessException("L'utilisateur doit appartenir à la chambre de commerce d'Ouesso");
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

        // Publier l'événement de changement de statut
        await _eventPublisher.PublishCertificatStatutChangeAsync(new CertificatStatutChangeEvent
        {
            CertificatId = certificatId,
            AncienStatut = StatutsCertificats.Soumis,
            NouveauStatut = StatutsCertificats.Controle
        }, cancellationToken);

        _logger.LogInformation("Certificat {CertificatId} contrôlé par l'utilisateur {UserId} (Ouesso)", certificatId, userId);

        // Envoyer notification de contrôle
        await _notificationService.EnvoyerNotificationControleAsync(certificatId, true, cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto> ApprouverCertificatAsync(Guid certificatId, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        // Vérifier que le certificat appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            throw new InvalidOperationException("Ce certificat n'appartient pas à la chambre de commerce d'Ouesso");
        }

        // Vérifier que le certificat est au statut Contrôlé
        if (certificat.EtatCode != StatutsCertificats.Controle)
        {
            _logger.LogWarning("Tentative d'approbation du certificat {CertificatId} avec un statut invalide. Statut actuel: {StatutActuel}", certificatId, certificat.EtatCode);
            throw new InvalidOperationException($"Le certificat doit être au statut 'Contrôlé' (Code: {StatutsCertificats.Controle}) pour être approuvé. Statut actuel: {certificat.EtatCode}");
        }

        // Vérifier le rôle (Contrôleur ou Superviseur - rôles 3 ou 4)
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);
        var peutApprouver = roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur);
        
        if (!peutApprouver)
        {
            throw new UnauthorizedAccessException("Seuls les Contrôleurs (rôle 3) et Superviseurs (rôle 4) peuvent approuver un certificat");
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

        // Publier l'événement de changement de statut
        await _eventPublisher.PublishCertificatStatutChangeAsync(new CertificatStatutChangeEvent
        {
            CertificatId = certificatId,
            AncienStatut = StatutsCertificats.Controle,
            NouveauStatut = StatutsCertificats.Approuve
        }, cancellationToken);

        _logger.LogInformation("Certificat {CertificatId} approuvé par l'utilisateur {UserId} (Ouesso)", certificatId, userId);

        // Envoyer notification d'approbation
        await _notificationService.EnvoyerNotificationApprobationAsync(certificatId, cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto> ValiderCertificatAsync(Guid certificatId, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var ancienStatut = certificat.EtatCode ?? string.Empty;

        // Vérifier que le certificat appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            throw new InvalidOperationException("Ce certificat n'appartient pas à la chambre de commerce d'Ouesso");
        }

        // Vérifier que le certificat est au statut Approuvé
        if (certificat.EtatCode != StatutsCertificats.Approuve)
        {
            throw new InvalidOperationException($"Le certificat doit être au statut 'Approuvé' pour être validé. Statut actuel: {certificat.EtatCode}");
        }

        // Vérifier le rôle (Président - rôle 6)
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);
        var estPresident = roles.Contains(RolesUtilisateurs.President);
        
        if (!estPresident)
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

        _logger.LogInformation("Certificat {CertificatId} validé définitivement par le Président {UserId} (Ouesso)", certificatId, userId);

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

        // Vérifier que le certificat appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            throw new InvalidOperationException("Ce certificat n'appartient pas à la chambre de commerce d'Ouesso");
        }

        // Vérifier que le certificat peut être rejeté (Soumis, Contrôlé ou Approuvé)
        var codeStatut = certificat.EtatCode;
        if (codeStatut != StatutsCertificats.Soumis &&
            codeStatut != StatutsCertificats.Controle &&
            codeStatut != StatutsCertificats.Approuve)
        {
            throw new InvalidOperationException($"Le certificat ne peut être rejeté qu'aux statuts Soumis, Contrôlé ou Approuvé. Statut actuel: {certificat.EtatCode}");
        }

        // Vérifier le rôle selon le statut
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);
        bool peutRejeter = false;

        if (codeStatut == StatutsCertificats.Approuve)
        {
            // Seul le Président peut rejeter depuis le statut approuvé
            peutRejeter = roles.Contains(RolesUtilisateurs.President);
        }
        else
        {
            // Contrôleur ou Superviseur pour les autres statuts
            peutRejeter = roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur);
        }

        if (!peutRejeter)
        {
            throw new UnauthorizedAccessException("Vous n'avez pas l'autorisation de rejeter ce certificat à ce stade");
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Récupérer le statut « Modification demandée » (GECO statut 5)
        var etatModification = await _etatRepository.GetByCodeAsync(StatutsCertificats.Modification, cancellationToken)
            ?? throw new InvalidOperationException($"Statut '{StatutsCertificats.Modification}' introuvable");

        certificat.EtatCode = etatModification.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);

        // Ajouter un commentaire
        var commentaireEntity = new Domain.Entities.Commentaire
        {
            Id = Guid.NewGuid(),
            CertificateId = certificatId,
            CommentaireText = commentaire,
            CreeLe = DateTime.UtcNow,
            CreePar = userId
        };
        await _commentaireRepository.AddAsync(commentaireEntity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publier l'événement de rejet
        await _eventPublisher.PublishCertificatRejeteAsync(new CertificatRejeteEvent
        {
            CertificatId = certificatId,
            CertificateNo = certificat.CertificateNo,
            Raison = commentaire
        }, cancellationToken);

        _logger.LogInformation("Certificat {CertificatId} rejeté par {UserId} avec commentaire (Ouesso)", certificatId, userId);

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

        // Vérifier que le certificat appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            throw new InvalidOperationException("Ce certificat n'appartient pas à la chambre de commerce d'Ouesso");
        }

        // Vérifier que le certificat est validé
        if (certificat.EtatCode != StatutsCertificats.Valide)
        {
            throw new InvalidOperationException($"Seuls les certificats validés peuvent faire l'objet d'une demande de modification. Statut actuel: {certificat.EtatCode}");
        }

        // Récupérer le statut "Modification"
        var etatModification = await _etatRepository.GetByCodeAsync(StatutsCertificats.Modification, cancellationToken)
            ?? throw new InvalidOperationException($"Statut '{StatutsCertificats.Modification}' introuvable");

        certificat.EtatCode = etatModification.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);

        // Ajouter un commentaire
        var commentaireEntity = new Domain.Entities.Commentaire
        {
            Id = Guid.NewGuid(),
            CertificateId = certificatId,
            CommentaireText = commentaire,
            CreeLe = DateTime.UtcNow,
            CreePar = userId
        };
        await _commentaireRepository.AddAsync(commentaireEntity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publier l'événement de changement de statut
        await _eventPublisher.PublishCertificatStatutChangeAsync(new CertificatStatutChangeEvent
        {
            CertificatId = certificatId,
            AncienStatut = StatutsCertificats.Valide,
            NouveauStatut = StatutsCertificats.Modification
        }, cancellationToken);

        _logger.LogInformation("Demande de modification pour le certificat {CertificatId} par {UserId} (Ouesso)", certificatId, userId);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<bool> EstTransitionValideAsync(Guid certificatId, string codeNouveauStatut, string userId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            return false;
        }

        // Vérifier que le certificat appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            return false;
        }

        var codeStatutActuel = certificat.EtatCode ?? string.Empty;
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);

        // Transitions valides pour Ouesso (identique à Pointe-Noire)
        return (codeStatutActuel, codeNouveauStatut) switch
        {
            (StatutsCertificats.Elabore, StatutsCertificats.Soumis) => true,
            (StatutsCertificats.Soumis, StatutsCertificats.Controle) => roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur),
            (StatutsCertificats.Soumis, StatutsCertificats.Modification) => roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur),
            (StatutsCertificats.Controle, StatutsCertificats.Approuve) => roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur),
            (StatutsCertificats.Controle, StatutsCertificats.Modification) => roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur),
            (StatutsCertificats.Approuve, StatutsCertificats.Valide) => roles.Contains(RolesUtilisateurs.President),
            (StatutsCertificats.Approuve, StatutsCertificats.Modification) => roles.Contains(RolesUtilisateurs.President),
            (StatutsCertificats.Valide, StatutsCertificats.Modification) => true,
            (StatutsCertificats.Modification, StatutsCertificats.Approuve) => roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur),
            (StatutsCertificats.Modification, StatutsCertificats.Valide) => roles.Contains(RolesUtilisateurs.President),
            _ => false
        };
    }

    public async Task<List<string>> GetTransitionsPossiblesAsync(Guid certificatId, string userId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            return new List<string>();
        }

        // Vérifier que le certificat appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            return new List<string>();
        }

        var codeStatut = certificat.EtatCode ?? string.Empty;
        var roles = await _authService.GetRolesAsync(userId, cancellationToken);
        var transitions = new List<string>();

        switch (codeStatut)
        {
            case StatutsCertificats.Elabore:
                transitions.Add(StatutsCertificats.Soumis);
                break;

            case StatutsCertificats.Soumis:
                if (roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur))
                {
                    transitions.Add(StatutsCertificats.Controle);
                    transitions.Add(StatutsCertificats.Modification);
                }
                break;

            case StatutsCertificats.Controle:
                if (roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur))
                {
                    transitions.Add(StatutsCertificats.Approuve);
                    transitions.Add(StatutsCertificats.Modification);
                }
                break;

            case StatutsCertificats.Approuve:
                if (roles.Contains(RolesUtilisateurs.President))
                {
                    transitions.Add(StatutsCertificats.Valide);
                    transitions.Add(StatutsCertificats.Modification);
                }
                break;

            case StatutsCertificats.Valide:
                transitions.Add(StatutsCertificats.Modification);
                break;

            case StatutsCertificats.Modification:
                if (roles.Contains(RolesUtilisateurs.Controleur) || roles.Contains(RolesUtilisateurs.Superviseur))
                {
                    transitions.Add(StatutsCertificats.Approuve);
                }
                if (roles.Contains(RolesUtilisateurs.President))
                {
                    transitions.Add(StatutsCertificats.Valide);
                }
                break;
        }

        return transitions;
    }
}
