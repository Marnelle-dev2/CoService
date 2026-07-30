using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Messaging.Handlers;

/// <summary>
/// Handler pour les événements d'exportateurs.
/// L'entité Exportateur locale a été supprimée (les données organisation sont désormais
/// consommées en direct via le gateway Enrôlement/Organisation) : ce handler est un no-op
/// conservé uniquement pour ne pas casser la consommation RabbitMQ existante.
/// </summary>
public class ExportateurEventHandler
{
    private readonly ILogger<ExportateurEventHandler> _logger;

    public ExportateurEventHandler(ILogger<ExportateurEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleExportateurCreeOuModifieAsync(string messageBody, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Événement exportateur.creé/modifié reçu mais ignoré (plus de table locale Exportateur).");
        return Task.CompletedTask;
    }

    public Task HandleExportateurSupprimeAsync(string messageBody, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Événement exportateur.supprimé reçu mais ignoré (plus de table locale Exportateur).");
        return Task.CompletedTask;
    }
}
