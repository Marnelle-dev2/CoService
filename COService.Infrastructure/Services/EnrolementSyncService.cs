using COService.Infrastructure.ExternalServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace COService.Infrastructure.Services;

/// <summary>
/// Service de synchronisation avec le microservice Enrolement.
///
/// NOTE: Les entités locales Partenaire/Exportateur ont été supprimées : les données
/// organisation (NIU, nom, etc.) sont désormais consommées en direct depuis le gateway
/// Enrôlement/Organisation au moment de la création/consultation d'un certificat, il n'y a
/// donc plus de table locale à synchroniser. Ce service est conservé comme no-op pour ne pas
/// casser les enregistrements DI / IHostedService existants.
/// </summary>
public class EnrolementSyncService : IHostedService, IEnrolementSyncService
{
    private readonly IEnrolementServiceClient _enrolementClient;
    private readonly ILogger<EnrolementSyncService> _logger;
    private readonly IOptions<EnrolementSyncOptions> _options;

    public EnrolementSyncService(
        IEnrolementServiceClient enrolementClient,
        ILogger<EnrolementSyncService> logger,
        IOptions<EnrolementSyncOptions> options)
    {
        _enrolementClient = enrolementClient;
        _logger = logger;
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Service de synchronisation Enrolement démarré en mode no-op (plus de table locale Partenaire/Exportateur, lecture live via le gateway).");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service de synchronisation Enrolement arrêté");
        return Task.CompletedTask;
    }

    public Task SynchroniserPartenairesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SynchroniserPartenairesAsync appelé mais no-op (plus de table locale Partenaire).");
        return Task.CompletedTask;
    }

    public Task SynchroniserExportateursAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SynchroniserExportateursAsync appelé mais no-op (plus de table locale Exportateur).");
        return Task.CompletedTask;
    }

    public Task SynchroniserPartenaireAsync(Guid partenaireId, CancellationToken cancellationToken = default)
    {
        return Task.FromException(new NotSupportedException(
            $"Synchronisation partenaire par GUID ({partenaireId}) non supportée : plus de table locale Partenaire."));
    }

    public Task SynchroniserExportateurAsync(Guid exportateurId, CancellationToken cancellationToken = default)
    {
        return Task.FromException(new NotSupportedException(
            $"Synchronisation exportateur par GUID ({exportateurId}) non supportée : plus de table locale Exportateur."));
    }
}

/// <summary>
/// Options de configuration pour la synchronisation Enrolement
/// </summary>
public class EnrolementSyncOptions
{
    public bool Enabled { get; set; } = true;
    public int IntervalMinutes { get; set; } = 60; // Synchronisation toutes les heures par défaut
}
