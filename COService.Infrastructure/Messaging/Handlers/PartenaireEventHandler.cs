using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Messaging.Handlers;

/// <summary>
/// Handler pour les événements de partenaires (Chambres de Commerce).
/// L'entité Partenaire locale a été supprimée (les données organisation sont désormais
/// consommées en direct via le gateway Enrôlement/Organisation) : ce handler est un no-op
/// conservé uniquement pour ne pas casser la consommation RabbitMQ existante.
/// </summary>
public class PartenaireEventHandler
{
    private readonly ILogger<PartenaireEventHandler> _logger;

    public PartenaireEventHandler(ILogger<PartenaireEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandlePartenaireCreeOuModifieAsync(string messageBody, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Événement partenaire.creé/modifié reçu mais ignoré (plus de table locale Partenaire).");
        return Task.CompletedTask;
    }

    public Task HandlePartenaireSupprimeAsync(string messageBody, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Événement partenaire.supprimé reçu mais ignoré (plus de table locale Partenaire).");
        return Task.CompletedTask;
    }
}
