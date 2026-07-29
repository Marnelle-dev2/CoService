using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Infrastructure.ExternalServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace COService.Infrastructure.Services;

/// <summary>
/// Service de synchronisation avec le microservice Enrolement
/// Synchronise les partenaires et exportateurs
/// 
/// NOTE: La synchronisation périodique est remplacée par des événements RabbitMQ.
/// Ce service reste disponible pour les synchronisations manuelles via API.
/// Les événements RabbitMQ seront gérés par des handlers séparés.
/// </summary>
public class EnrolementSyncService : IHostedService, IEnrolementSyncService
{
    private readonly IEnrolementServiceClient _enrolementClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EnrolementSyncService> _logger;
    private readonly IOptions<EnrolementSyncOptions> _options;
    private Timer? _timer;

    public EnrolementSyncService(
        IEnrolementServiceClient enrolementClient,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<EnrolementSyncService> logger,
        IOptions<EnrolementSyncOptions> options)
    {
        _enrolementClient = enrolementClient;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // NOTE: La synchronisation périodique est désactivée par défaut
        // Les mises à jour arrivent via RabbitMQ en temps réel
        // La synchronisation périodique peut être activée pour des cas spécifiques (fallback, etc.)
        if (_options.Value.Enabled && _options.Value.IntervalMinutes > 0)
        {
            var interval = TimeSpan.FromMinutes(_options.Value.IntervalMinutes);
            _timer = new Timer(DoWork, null, TimeSpan.Zero, interval);
            _logger.LogInformation("Service de synchronisation Enrolement (périodique) démarré. Intervalle: {Interval} minutes", _options.Value.IntervalMinutes);
            _logger.LogWarning("ATTENTION: La synchronisation périodique est activée. Les événements RabbitMQ sont la méthode recommandée.");
        }
        else
        {
            _logger.LogInformation("Service de synchronisation Enrolement (périodique) désactivé. Utilisation des événements RabbitMQ.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        _logger.LogInformation("Service de synchronisation Enrolement arrêté");
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            _logger.LogInformation("Début de la synchronisation avec Enrolement");
            await SynchroniserPartenairesAsync();
            await SynchroniserExportateursAsync();
            _logger.LogInformation("Synchronisation avec Enrolement terminée avec succès");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation avec Enrolement");
        }
    }

    /// <summary>
    /// Synchronise tous les partenaires depuis Enrolement
    /// </summary>
    public async Task SynchroniserPartenairesAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var partenaireRepository = scope.ServiceProvider.GetRequiredService<IPartenaireRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        try
        {
            _logger.LogInformation("Synchronisation des partenaires...");
            var orgs = await _enrolementClient.GetOrganisationsByTypeAsync("PARTENAIRE", cancellationToken);
            var partenairesExternes = orgs.Select(OrganisationRemoteMapper.ToPartenaire).ToList();

            foreach (var partenaireDto in partenairesExternes)
            {
                var partenaireExistant = await partenaireRepository.GetByCodeAsync(partenaireDto.CodePartenaire, cancellationToken);

                if (partenaireExistant == null)
                {
                    // Créer nouveau partenaire
                    var nouveauPartenaire = mapper.Map<Partenaire>(partenaireDto);
                    nouveauPartenaire.DerniereSynchronisation = DateTime.UtcNow;
                    await partenaireRepository.AddAsync(nouveauPartenaire, cancellationToken);
                    _logger.LogInformation("Partenaire créé: {Code} - {Nom}", partenaireDto.CodePartenaire, partenaireDto.Nom);
                }
                else
                {
                    // Mettre à jour partenaire existant
                    mapper.Map(partenaireDto, partenaireExistant);
                    partenaireExistant.DerniereSynchronisation = DateTime.UtcNow;
                    partenaireExistant.ModifierLe = DateTime.UtcNow;
                    partenaireRepository.Update(partenaireExistant);
                    _logger.LogInformation("Partenaire mis à jour: {Code} - {Nom}", partenaireDto.CodePartenaire, partenaireDto.Nom);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Synchronisation des partenaires terminée. {Count} partenaire(s) synchronisé(s)", partenairesExternes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation des partenaires");
            throw;
        }
    }

    /// <summary>
    /// Synchronise tous les exportateurs depuis Enrolement
    /// </summary>
    public async Task SynchroniserExportateursAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var exportateurRepository = scope.ServiceProvider.GetRequiredService<IExportateurRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        try
        {
            _logger.LogInformation("Synchronisation des exportateurs...");
            var orgs = await _enrolementClient.GetOrganisationsByTypeAsync("EXPORTATEUR", cancellationToken);
            var exportateursExternes = orgs.Select(OrganisationRemoteMapper.ToExportateur).ToList();

            foreach (var exportateurDto in exportateursExternes)
            {
                var exportateurExistant = await exportateurRepository.GetByCodeAsync(exportateurDto.CodeExportateur, cancellationToken);

                if (exportateurExistant == null)
                {
                    // Créer nouveau exportateur
                    var nouvelExportateur = mapper.Map<Exportateur>(exportateurDto);
                    nouvelExportateur.DerniereSynchronisation = DateTime.UtcNow;
                    await exportateurRepository.AddAsync(nouvelExportateur, cancellationToken);
                    _logger.LogInformation("Exportateur créé: {Code} - {Nom}", exportateurDto.CodeExportateur, exportateurDto.Nom);
                }
                else
                {
                    // Mettre à jour exportateur existant
                    mapper.Map(exportateurDto, exportateurExistant);
                    exportateurExistant.DerniereSynchronisation = DateTime.UtcNow;
                    exportateurExistant.ModifierLe = DateTime.UtcNow;
                    exportateurRepository.Update(exportateurExistant);
                    _logger.LogInformation("Exportateur mis à jour: {Code} - {Nom}", exportateurDto.CodeExportateur, exportateurDto.Nom);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Synchronisation des exportateurs terminée. {Count} exportateur(s) synchronisé(s)", exportateursExternes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation des exportateurs");
            throw;
        }
    }

    public Task SynchroniserPartenaireAsync(Guid partenaireId, CancellationToken cancellationToken = default)
    {
        return Task.FromException(new NotSupportedException(
            $"Synchronisation partenaire par GUID ({partenaireId}) non supportée avec l'API Organisation (identifiant = code). Utiliser SynchroniserPartenairesAsync()."));
    }

    public Task SynchroniserExportateurAsync(Guid exportateurId, CancellationToken cancellationToken = default)
    {
        return Task.FromException(new NotSupportedException(
            $"Synchronisation exportateur par GUID ({exportateurId}) non supportée avec l'API Organisation (identifiant = code). Utiliser SynchroniserExportateursAsync()."));
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
