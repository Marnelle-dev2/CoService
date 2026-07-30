using COService.Application.Messaging;
using COService.Application.Repositories;
using COService.Shared.Events;
using Microsoft.Extensions.Logging;

namespace COService.Application.Services;

/// <summary>
/// Service pour la gestion des notifications
/// Publie des événements RabbitMQ vers le microservice Notification
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ICertificatOrigineRepository _certificatRepository;
    private readonly INotificationEventPublisher _notificationPublisher;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ICertificatOrigineRepository certificatRepository,
        INotificationEventPublisher notificationPublisher,
        ILogger<NotificationService> logger)
    {
        _certificatRepository = certificatRepository;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task EnvoyerNotificationChangementStatutAsync(Guid certificatId, string ancienStatut, string nouveauStatut, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            _logger.LogWarning("Certificat {CertificatId} introuvable pour notification de changement de statut", certificatId);
            return;
        }

        // Récupérer l'email de l'exportateur si disponible
        var destinataire = string.Empty ?? string.Empty;
        if (string.IsNullOrEmpty(destinataire))
        {
            _logger.LogWarning("Aucun email trouvé pour l'exportateur du certificat {CertificatId}", certificatId);
            return;
        }

        var sujet = $"Changement de statut - Certificat {certificat.CertificateNo}";
        var corps = $"Le statut de votre certificat {certificat.CertificateNo} a changé de '{ancienStatut}' à '{nouveauStatut}'.";

        await _notificationPublisher.PublishNotificationDemandeAsync(new NotificationDemandeEvent
        {
            Type = "email",
            Destinataire = destinataire,
            Sujet = sujet,
            Corps = corps
        }, cancellationToken);

        _logger.LogInformation("Notification de changement de statut envoyée pour le certificat {CertificatId}", certificatId);
    }

    public async Task EnvoyerNotificationRejetAsync(Guid certificatId, string commentaire, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            _logger.LogWarning("Certificat {CertificatId} introuvable pour notification de rejet", certificatId);
            return;
        }

        var destinataire = string.Empty ?? string.Empty;
        if (string.IsNullOrEmpty(destinataire))
        {
            _logger.LogWarning("Aucun email trouvé pour l'exportateur du certificat {CertificatId}", certificatId);
            return;
        }

        var sujet = $"Rejet du certificat {certificat.CertificateNo}";
        var corps = $"Votre certificat {certificat.CertificateNo} a été rejeté.\n\nMotif : {commentaire}";

        await _notificationPublisher.PublishNotificationDemandeAsync(new NotificationDemandeEvent
        {
            Type = "email",
            Destinataire = destinataire,
            Sujet = sujet,
            Corps = corps
        }, cancellationToken);

        _logger.LogInformation("Notification de rejet envoyée pour le certificat {CertificatId}", certificatId);
    }

    public async Task EnvoyerNotificationValidationAsync(Guid certificatId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            _logger.LogWarning("Certificat {CertificatId} introuvable pour notification de validation", certificatId);
            return;
        }

        var destinataire = string.Empty ?? string.Empty;
        if (string.IsNullOrEmpty(destinataire))
        {
            _logger.LogWarning("Aucun email trouvé pour l'exportateur du certificat {CertificatId}", certificatId);
            return;
        }

        var sujet = $"Certificat validé - {certificat.CertificateNo}";
        var corps = $"Votre certificat {certificat.CertificateNo} a été validé. Vous pouvez maintenant générer le PDF.";

        await _notificationPublisher.PublishNotificationDemandeAsync(new NotificationDemandeEvent
        {
            Type = "email",
            Destinataire = destinataire,
            Sujet = sujet,
            Corps = corps
        }, cancellationToken);

        _logger.LogInformation("Notification de validation envoyée pour le certificat {CertificatId}", certificatId);
    }

    public async Task EnvoyerNotificationSoumissionAsync(Guid certificatId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            _logger.LogWarning("Certificat {CertificatId} introuvable pour notification de soumission", certificatId);
            return;
        }

        // Notification à l'exportateur
        var destinataireExportateur = string.Empty ?? string.Empty;
        if (!string.IsNullOrEmpty(destinataireExportateur))
        {
            await _notificationPublisher.PublishNotificationDemandeAsync(new NotificationDemandeEvent
            {
                Type = "email",
                Destinataire = destinataireExportateur,
                Sujet = $"Certificat soumis - {certificat.CertificateNo}",
                Corps = $"Votre certificat {certificat.CertificateNo} a été soumis avec succès."
            }, cancellationToken);
        }

        // Notification à la chambre de commerce
        var destinataireChambre = string.Empty ?? string.Empty;
        if (!string.IsNullOrEmpty(destinataireChambre))
        {
            await _notificationPublisher.PublishNotificationDemandeAsync(new NotificationDemandeEvent
            {
                Type = "email",
                Destinataire = destinataireChambre,
                Sujet = $"Nouveau certificat soumis - {certificat.CertificateNo}",
                Corps = $"Un nouveau certificat {certificat.CertificateNo} a été soumis par {certificat.ExportateurNom}."
            }, cancellationToken);
        }

        _logger.LogInformation("Notifications de soumission envoyées pour le certificat {CertificatId}", certificatId);
    }

    public async Task EnvoyerNotificationControleAsync(Guid certificatId, bool approuve, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            _logger.LogWarning("Certificat {CertificatId} introuvable pour notification de contrôle", certificatId);
            return;
        }

        if (approuve)
        {
            var destinataire = string.Empty ?? string.Empty;
            if (!string.IsNullOrEmpty(destinataire))
            {
                await _notificationPublisher.PublishNotificationDemandeAsync(new NotificationDemandeEvent
                {
                    Type = "email",
                    Destinataire = destinataire,
                    Sujet = $"Certificat contrôlé - {certificat.CertificateNo}",
                    Corps = $"Votre certificat {certificat.CertificateNo} a été contrôlé et approuvé."
                }, cancellationToken);
            }
        }

        _logger.LogInformation("Notification de contrôle envoyée pour le certificat {CertificatId}", certificatId);
    }

    public async Task EnvoyerNotificationApprobationAsync(Guid certificatId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            _logger.LogWarning("Certificat {CertificatId} introuvable pour notification d'approbation", certificatId);
            return;
        }

        // Notification à l'exportateur
        var destinataireExportateur = string.Empty ?? string.Empty;
        if (!string.IsNullOrEmpty(destinataireExportateur))
        {
            await _notificationPublisher.PublishNotificationDemandeAsync(new NotificationDemandeEvent
            {
                Type = "email",
                Destinataire = destinataireExportateur,
                Sujet = $"Certificat approuvé - {certificat.CertificateNo}",
                Corps = $"Votre certificat {certificat.CertificateNo} a été approuvé et est en attente de validation finale par le Président."
            }, cancellationToken);
        }

        // Notification au Président
        var destinataireChambre = string.Empty ?? string.Empty;
        if (!string.IsNullOrEmpty(destinataireChambre))
        {
            await _notificationPublisher.PublishNotificationDemandeAsync(new NotificationDemandeEvent
            {
                Type = "email",
                Destinataire = destinataireChambre,
                Sujet = $"Certificat en attente de validation - {certificat.CertificateNo}",
                Corps = $"Le certificat {certificat.CertificateNo} a été approuvé et nécessite votre validation finale."
            }, cancellationToken);
        }

        _logger.LogInformation("Notifications d'approbation envoyées pour le certificat {CertificatId}", certificatId);
    }

    public async Task EnvoyerNotificationFormuleAAsync(Guid certificatId, string statut, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            _logger.LogWarning("Certificat {CertificatId} introuvable pour notification Formule A", certificatId);
            return;
        }

        var destinataire = string.Empty ?? string.Empty;
        if (string.IsNullOrEmpty(destinataire))
        {
            _logger.LogWarning("Aucun email trouvé pour l'exportateur du certificat {CertificatId}", certificatId);
            return;
        }

        var sujet = $"Formule A - Statut : {statut} - {certificat.CertificateNo}";
        var corps = $"Le statut de votre Formule A (certificat {certificat.CertificateNo}) est maintenant : {statut}.";

        await _notificationPublisher.PublishNotificationDemandeAsync(new NotificationDemandeEvent
        {
            Type = "email",
            Destinataire = destinataire,
            Sujet = sujet,
            Corps = corps
        }, cancellationToken);

        _logger.LogInformation("Notification Formule A envoyée pour le certificat {CertificatId} (statut: {Statut})", certificatId, statut);
    }
}
