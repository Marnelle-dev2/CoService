using COService.Application.Messaging;
using COService.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Messaging;

/// <summary>
/// Publisher pour les événements de certificats.
/// Dual publish : exchange legacy (routing keys) + MassTransit (déclenche la saga).
/// </summary>
public class CertificateEventPublisher : ICertificateEventPublisher
{
    private readonly IRabbitMQClient _rabbitMQClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CertificateEventPublisher> _logger;

    public CertificateEventPublisher(
        IRabbitMQClient rabbitMQClient,
        IPublishEndpoint publishEndpoint,
        ILogger<CertificateEventPublisher> logger)
    {
        _rabbitMQClient = rabbitMQClient;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishCertificatCreeAsync(CertificatCreeEvent evt, CancellationToken cancellationToken = default)
    {
        try
        {
            await _rabbitMQClient.PublishAsync("certificat.creé", evt, cancellationToken);
            await _publishEndpoint.Publish(evt, cancellationToken);
            _logger.LogInformation("Événement 'certificat.creé' publié pour le certificat {CertificatId}", evt.CertificatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la publication de l'événement 'certificat.creé' pour {CertificatId}", evt.CertificatId);
        }
    }

    public async Task PublishCertificatStatutChangeAsync(CertificatStatutChangeEvent evt, CancellationToken cancellationToken = default)
    {
        try
        {
            await _rabbitMQClient.PublishAsync("certificat.statut.changé", evt, cancellationToken);
            await _publishEndpoint.Publish(evt, cancellationToken);
            _logger.LogInformation(
                "Événement 'certificat.statut.changé' publié pour le certificat {CertificatId} : {AncienStatut} → {NouveauStatut}",
                evt.CertificatId, evt.AncienStatut, evt.NouveauStatut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la publication de l'événement 'certificat.statut.changé' pour {CertificatId}", evt.CertificatId);
        }
    }

    public async Task PublishCertificatValideAsync(CertificatValideEvent evt, CancellationToken cancellationToken = default)
    {
        try
        {
            // Legacy routing keys (interop existante)
            await _rabbitMQClient.PublishAsync("co.valide", new EvenementCOValide
            {
                IdentifiantCO = evt.CertificatId,
                NumeroCO = evt.CertificateNo,
                NIUExportateur = evt.ExportateurNIU,
                NIUPartenaire = evt.PartenaireNIU,
                DateValidationUtc = evt.Timestamp.ToUniversalTime()
            }, cancellationToken);

            await _rabbitMQClient.PublishAsync("certificat.valide", evt, cancellationToken);

            // MassTransit → démarre la saga post-validation
            await _publishEndpoint.Publish(evt, cancellationToken);

            _logger.LogInformation(
                "Événement 'co.valide' + saga MassTransit publiés pour le certificat {CertificatId} ({CertificateNo})",
                evt.CertificatId, evt.CertificateNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la publication de l'événement 'co.valide' pour {CertificatId}", evt.CertificatId);
        }
    }

    public async Task PublishCertificatRejeteAsync(CertificatRejeteEvent evt, CancellationToken cancellationToken = default)
    {
        try
        {
            await _rabbitMQClient.PublishAsync("certificat.rejeté", evt, cancellationToken);
            await _publishEndpoint.Publish(evt, cancellationToken);
            _logger.LogInformation("Événement 'certificat.rejeté' publié pour le certificat {CertificatId}", evt.CertificatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la publication de l'événement 'certificat.rejeté' pour {CertificatId}", evt.CertificatId);
        }
    }
}
