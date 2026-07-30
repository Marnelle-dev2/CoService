using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Messaging;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Shared.Constants;
using COService.Shared.Events;
using Microsoft.Extensions.Logging;

namespace COService.Application.Services;

/// <summary>
/// Service pour la gestion des Formules A (spécifique à Ouesso)
/// </summary>
public class FormuleAService : IFormuleAService
{
    private readonly ICertificatOrigineRepository _certificatRepository;
    private readonly IEtatRepository _etatRepository;
    private readonly ICommentaireRepository _commentaireRepository;
    private readonly IAuthService _authService;
    private readonly ICertificateEventPublisher _eventPublisher;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FormuleAService> _logger;

    public FormuleAService(
        ICertificatOrigineRepository certificatRepository,
        IEtatRepository etatRepository,
        ICommentaireRepository commentaireRepository,
        IAuthService authService,
        ICertificateEventPublisher eventPublisher,
        INotificationService notificationService,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<FormuleAService> logger)
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

    public async Task<CertificatOrigineDto> CreerFormuleAAsync(Guid certificatOrigineId, string userId, string password, CancellationToken cancellationToken = default)
    {
        // 1. Récupérer le certificat
        var certificat = await _certificatRepository.GetByIdAsync(certificatOrigineId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatOrigineId} introuvable");

        // 2. Vérifier qu'on peut créer une Formule A
        var peutCreer = await PeutCreerFormuleAAsync(certificatOrigineId, userId, cancellationToken);
        if (!peutCreer)
        {
            throw new InvalidOperationException("Impossible de créer une Formule A pour ce certificat. Vérifiez que le CO est validé, appartient à Ouesso et que vous avez les autorisations nécessaires.");
        }

        // 3. Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // 4. Récupérer le statut "Formule A soumise"
        var etatFormuleASoumise = await _etatRepository.GetByCodeAsync(StatutsCertificats.FormuleASoumise, cancellationToken)
            ?? throw new InvalidOperationException("Statut 'Formule A soumise' introuvable dans la base de données");

        // 5. Mettre à jour le certificat
        certificat.EtatCode = etatFormuleASoumise.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Formule A créée pour le certificat {CertificatId} par l'utilisateur {UserId}",
            certificatOrigineId, userId);

        // Envoyer notification Formule A
        await _notificationService.EnvoyerNotificationFormuleAAsync(certificatOrigineId, StatutsCertificats.FormuleASoumise, cancellationToken);

        var certificatDto = await _certificatRepository.GetByIdAsync(certificatOrigineId, cancellationToken);
        return _mapper.Map<CertificatOrigineDto>(certificatDto!);
    }

    public async Task<CertificatOrigineDto> ControleFormuleAAsync(Guid id, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {id} introuvable");

        // Vérifier que c'est une Formule A au statut "soumise"
        if (certificat.EtatCode != StatutsCertificats.FormuleASoumise)
        {
            throw new InvalidOperationException($"Le certificat doit être au statut '{StatutsCertificats.FormuleASoumise}' pour être contrôlé");
        }

        // Vérifier le rôle (Contrôleur ou Superviseur)
        var estControleur = await _authService.VerifierRoleAsync(userId, RolesUtilisateurs.Controleur, cancellationToken);
        var estSuperviseur = await _authService.VerifierRoleAsync(userId, RolesUtilisateurs.Superviseur, cancellationToken);

        if (!estControleur && !estSuperviseur)
        {
            throw new UnauthorizedAccessException("Seuls les contrôleurs et superviseurs peuvent contrôler une Formule A");
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Vérifier que le certificat appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            throw new InvalidOperationException("Seule la chambre de commerce d'Ouesso peut gérer les Formules A");
        }

        // Récupérer le statut "Formule A contrôlée"
        var etatControlee = await _etatRepository.GetByCodeAsync(StatutsCertificats.FormuleAControlee, cancellationToken)
            ?? throw new InvalidOperationException("Statut 'Formule A contrôlée' introuvable");

        // Mettre à jour le statut
        certificat.EtatCode = etatControlee.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Formule A {CertificatId} contrôlée par {UserId}", id, userId);

        // Envoyer notification Formule A
        await _notificationService.EnvoyerNotificationFormuleAAsync(id, StatutsCertificats.FormuleAControlee, cancellationToken);

        var certificatDto = await _certificatRepository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<CertificatOrigineDto>(certificatDto!);
    }

    public async Task<CertificatOrigineDto> ApprouverFormuleAAsync(Guid id, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {id} introuvable");

        // Vérifier que c'est une Formule A au statut "contrôlée"
        if (certificat.EtatCode != StatutsCertificats.FormuleAControlee)
        {
            throw new InvalidOperationException($"Le certificat doit être au statut '{StatutsCertificats.FormuleAControlee}' pour être approuvé");
        }

        // Vérifier le rôle
        var estControleur = await _authService.VerifierRoleAsync(userId, RolesUtilisateurs.Controleur, cancellationToken);
        var estSuperviseur = await _authService.VerifierRoleAsync(userId, RolesUtilisateurs.Superviseur, cancellationToken);

        if (!estControleur && !estSuperviseur)
        {
            throw new UnauthorizedAccessException("Seuls les contrôleurs et superviseurs peuvent approuver une Formule A");
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Récupérer le statut "Formule A approuvée"
        var etatApprouvee = await _etatRepository.GetByCodeAsync(StatutsCertificats.FormuleAApprouvee, cancellationToken)
            ?? throw new InvalidOperationException("Statut 'Formule A approuvée' introuvable");

        certificat.EtatCode = etatApprouvee.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Formule A {CertificatId} approuvée par {UserId}", id, userId);

        // Envoyer notification Formule A
        await _notificationService.EnvoyerNotificationFormuleAAsync(id, StatutsCertificats.FormuleAApprouvee, cancellationToken);

        var certificatDto = await _certificatRepository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<CertificatOrigineDto>(certificatDto!);
    }

    public async Task<CertificatOrigineDto> ValiderFormuleAAsync(Guid id, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {id} introuvable");

        // Vérifier que c'est une Formule A au statut "approuvée"
        if (certificat.EtatCode != StatutsCertificats.FormuleAApprouvee)
        {
            throw new InvalidOperationException($"Le certificat doit être au statut '{StatutsCertificats.FormuleAApprouvee}' pour être validé");
        }

        // Vérifier le rôle (Président uniquement)
        var estPresident = await _authService.VerifierRoleAsync(userId, RolesUtilisateurs.President, cancellationToken);
        if (!estPresident)
        {
            throw new UnauthorizedAccessException("Seul le Président peut valider définitivement une Formule A");
        }

        // Vérifier que l'utilisateur appartient à la même organisation que le certificat
        var userInfo = await _authService.GetUserInfoAsync(userId, cancellationToken);
        if (string.IsNullOrEmpty(certificat.PartenaireNIU) || !string.Equals(userInfo?.OrganisationCode, certificat.PartenaireNIU, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Le Président doit appartenir à la même chambre de commerce que le certificat");
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Récupérer le statut "Formule A validée"
        var etatValidee = await _etatRepository.GetByCodeAsync(StatutsCertificats.FormuleAValidee, cancellationToken)
            ?? throw new InvalidOperationException("Statut 'Formule A validée' introuvable");

        certificat.EtatCode = etatValidee.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Formule A {CertificatId} validée définitivement par {UserId}", id, userId);

        // Publier l'événement pour la facturation
        await _eventPublisher.PublishCertificatStatutChangeAsync(new CertificatStatutChangeEvent
        {
            CertificatId = id,
            AncienStatut = StatutsCertificats.FormuleAApprouvee,
            NouveauStatut = StatutsCertificats.FormuleAValidee
        }, cancellationToken);

        await _eventPublisher.PublishCertificatValideAsync(new CertificatValideEvent
        {
            CertificatId = id,
            CertificateNo = certificat.CertificateNo,
            ExportateurNIU = certificat.ExportateurNIU,
            PartenaireNIU = certificat.PartenaireNIU
        }, cancellationToken);

        // Envoyer notification Formule A validée
        await _notificationService.EnvoyerNotificationFormuleAAsync(id, StatutsCertificats.FormuleAValidee, cancellationToken);

        var certificatDto = await _certificatRepository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<CertificatOrigineDto>(certificatDto!);
    }

    public async Task<CertificatOrigineDto> RejeterFormuleAAsync(Guid id, string userId, string password, string commentaire, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(commentaire))
        {
            throw new ArgumentException("Un commentaire est obligatoire pour rejeter une Formule A", nameof(commentaire));
        }

        var certificat = await _certificatRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {id} introuvable");

        // Vérifier que c'est une Formule A (statuts 12, 13 ou 14)
        var codeStatut = certificat.EtatCode;
        if (codeStatut != StatutsCertificats.FormuleASoumise &&
            codeStatut != StatutsCertificats.FormuleAControlee &&
            codeStatut != StatutsCertificats.FormuleAApprouvee)
        {
            throw new InvalidOperationException("Seules les Formules A aux statuts soumise, contrôlée ou approuvée peuvent être rejetées");
        }

        // Vérifier le rôle selon le statut
        if (codeStatut == StatutsCertificats.FormuleAApprouvee)
        {
            // Seul le Président peut rejeter depuis le statut approuvé
            var estPresident = await _authService.VerifierRoleAsync(userId, RolesUtilisateurs.President, cancellationToken);
            if (!estPresident)
            {
                throw new UnauthorizedAccessException("Seul le Président peut rejeter une Formule A approuvée");
            }
        }
        else
        {
            // Contrôleur ou Superviseur pour les autres statuts
            var estControleur = await _authService.VerifierRoleAsync(userId, RolesUtilisateurs.Controleur, cancellationToken);
            var estSuperviseur = await _authService.VerifierRoleAsync(userId, RolesUtilisateurs.Superviseur, cancellationToken);
            if (!estControleur && !estSuperviseur)
            {
                throw new UnauthorizedAccessException("Seuls les contrôleurs et superviseurs peuvent rejeter une Formule A à ce stade");
            }
        }

        // Vérifier le mot de passe
        var motDePasseValide = await _authService.VerifierMotDePasseAsync(userId, password, cancellationToken);
        if (!motDePasseValide)
        {
            throw new UnauthorizedAccessException("Mot de passe incorrect");
        }

        // Récupérer le statut "Rejeté"
        var etatRejete = await _etatRepository.GetByCodeAsync(StatutsCertificats.Rejete, cancellationToken)
            ?? throw new InvalidOperationException("Statut 'Rejeté' introuvable");

        certificat.EtatCode = etatRejete.Code;
        certificat.ModifierLe = DateTime.UtcNow;
        certificat.ModifiePar = userId;

        _certificatRepository.Update(certificat);

        // Ajouter un commentaire
        var commentaireEntity = new Commentaire
        {
            Id = Guid.NewGuid(),
            CertificateId = id,
            CommentaireText = commentaire,
            CreeLe = DateTime.UtcNow,
            CreePar = userId
        };
        await _commentaireRepository.AddAsync(commentaireEntity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Formule A {CertificatId} rejetée par {UserId} avec commentaire", id, userId);

        // Envoyer notification de rejet
        await _notificationService.EnvoyerNotificationRejetAsync(id, commentaire, cancellationToken);

        var certificatDto = await _certificatRepository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<CertificatOrigineDto>(certificatDto!);
    }

    public async Task<bool> PeutCreerFormuleAAsync(Guid certificatId, string userId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            return false;
        }

        // 1. Vérifier que le CO est validé (statut 8)
        if (certificat.EtatCode != StatutsCertificats.Valide)
        {
            _logger.LogWarning("Certificat {CertificatId} n'est pas validé (statut actuel: {Statut})", certificatId, certificat.EtatCode);
            return false;
        }

        // 2. Vérifier que le CO appartient à Ouesso
        if (!ChambresCommerce.EstOuesso(certificat.PartenaireNIU))
        {
            _logger.LogWarning("Certificat {CertificatId} n'appartient pas à Ouesso", certificatId);
            return false;
        }

        // 3. Vérifier l'autorisation de l'exportateur
        if (string.IsNullOrEmpty(certificat.ExportateurNIU))
        {
            return false;
        }

        var autorise = await VerifierAutorisationFormuleAAsync(certificatId, certificat.ExportateurNIU, cancellationToken);
        if (!autorise)
        {
            _logger.LogWarning("Exportateur {ExportateurNIU} n'est pas autorisé à créer une Formule A pour le certificat {CertificatId}", certificat.ExportateurNIU, certificatId);
            return false;
        }

        return true;
    }

    public Task<bool> VerifierChambreAutoriseeFormuleAAsync(string partenaireNIU, CancellationToken cancellationToken = default)
    {
        // Vérifier que c'est Ouesso via le NIU du partenaire (chambre de commerce)
        return Task.FromResult(ChambresCommerce.EstOuesso(partenaireNIU));
    }

    public async Task<bool> VerifierAutorisationFormuleAAsync(Guid certificatId, string exportateurNIU, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null || string.IsNullOrEmpty(certificat.ExportateurNIU))
        {
            return false;
        }

        // Vérifier que l'exportateur est bien celui du certificat
        if (!string.Equals(certificat.ExportateurNIU, exportateurNIU, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Le propriétaire du certificat peut toujours créer une Formule A
        // (le type d'exportateur est désormais géré côté Enrôlement/Organisation)
        return true;
    }
}
