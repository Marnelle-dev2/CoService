using COService.Application.Repositories;
using COService.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace COService.Infrastructure.Messaging.Handlers;

/// <summary>
/// Handler pour les événements de référentiels
/// Consomme les événements : referentiel.*.mis-a-jour
/// </summary>
public class ReferentielEventHandler
{
    private readonly IPaysRepository _paysRepository;
    private readonly IPortRepository _portRepository;
    private readonly IAeroportRepository _aeroportRepository;
    private readonly IDeviseRepository _deviseRepository;
    private readonly IModuleRepository _moduleRepository;
    private readonly IIncotermRepository _incotermRepository;
    private readonly IBureauDedouanementRepository _bureauDedouanementRepository;
    private readonly IUniteStatistiqueRepository _uniteStatistiqueRepository;
    private readonly IDepartementRepository _departementRepository;
    private readonly IEtatRepository _etatRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReferentielEventHandler> _logger;

    public ReferentielEventHandler(
        IPaysRepository paysRepository,
        IPortRepository portRepository,
        IAeroportRepository aeroportRepository,
        IDeviseRepository deviseRepository,
        IModuleRepository moduleRepository,
        IIncotermRepository incotermRepository,
        IBureauDedouanementRepository bureauDedouanementRepository,
        IUniteStatistiqueRepository uniteStatistiqueRepository,
        IDepartementRepository departementRepository,
        IEtatRepository etatRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReferentielEventHandler> logger)
    {
        _paysRepository = paysRepository;
        _portRepository = portRepository;
        _aeroportRepository = aeroportRepository;
        _deviseRepository = deviseRepository;
        _moduleRepository = moduleRepository;
        _incotermRepository = incotermRepository;
        _bureauDedouanementRepository = bureauDedouanementRepository;
        _uniteStatistiqueRepository = uniteStatistiqueRepository;
        _departementRepository = departementRepository;
        _etatRepository = etatRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Traite un événement référentiel selon son routing key
    /// </summary>
    public async Task HandleReferentielEventAsync(string routingKey, string messageBody, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (routingKey)
            {
                case "referentiel.pays.mis-a-jour":
                    await HandlePaysMisAJourAsync(messageBody, cancellationToken);
                    break;
                case "referentiel.port.mis-a-jour":
                    await HandlePortMisAJourAsync(messageBody, cancellationToken);
                    break;
                case "referentiel.aeroport.mis-a-jour":
                    await HandleAeroportMisAJourAsync(messageBody, cancellationToken);
                    break;
                case "referentiel.devise.mis-a-jour":
                    await HandleDeviseMisAJourAsync(messageBody, cancellationToken);
                    break;
                case "referentiel.module.mis-a-jour":
                    await HandleModuleMisAJourAsync(messageBody, cancellationToken);
                    break;
                case "referentiel.incoterm.mis-a-jour":
                    await HandleIncotermMisAJourAsync(messageBody, cancellationToken);
                    break;
                case "referentiel.bureau-dedouanement.mis-a-jour":
                    await HandleBureauDedouanementMisAJourAsync(messageBody, cancellationToken);
                    break;
                case "referentiel.unite-statistique.mis-a-jour":
                    await HandleUniteStatistiqueMisAJourAsync(messageBody, cancellationToken);
                    break;
                case "referentiel.departement.mis-a-jour":
                    await HandleDepartementMisAJourAsync(messageBody, cancellationToken);
                    break;
                case "referentiel.etat.mis-a-jour":
                    await HandleEtatMisAJourAsync(messageBody, cancellationToken);
                    break;
                default:
                    _logger.LogWarning("Routing key référentiel non géré : {RoutingKey}", routingKey);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de l'événement référentiel {RoutingKey}", routingKey);
            throw;
        }
    }

    private async Task HandlePaysMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        var eventData = JsonSerializer.Deserialize<ReferentielEventData<PaysData>>(messageBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (eventData?.Pays == null || !eventData.Pays.Any())
        {
            _logger.LogWarning("Aucun pays dans l'événement référentiel.pays.mis-a-jour");
            return;
        }

        foreach (var paysData in eventData.Pays)
        {
            var paysExistant = await _paysRepository.GetByIdAsync(paysData.Id, cancellationToken);

            if (paysExistant == null)
            {
                var nouveauPays = new Pays
                {
                    Id = paysData.Id,
                    Code = paysData.Code,
                    Nom = paysData.Nom,
                    Actif = paysData.Actif,
                    CreeLe = DateTime.UtcNow,
                    CreePar = "SYSTEM"
                };
                await _paysRepository.AddAsync(nouveauPays, cancellationToken);
            }
            else
            {
                paysExistant.Code = paysData.Code;
                paysExistant.Nom = paysData.Nom;
                paysExistant.Actif = paysData.Actif;
                paysExistant.ModifierLe = DateTime.UtcNow;
                paysExistant.ModifiePar = "SYSTEM";
                _paysRepository.Update(paysExistant);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Synchronisé {Count} pays depuis événement RabbitMQ", eventData.Pays.Count());
    }

    private async Task HandlePortMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        var eventData = JsonSerializer.Deserialize<ReferentielEventData<PortData>>(messageBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (eventData?.Ports == null || !eventData.Ports.Any())
        {
            _logger.LogWarning("Aucun port dans l'événement référentiel.port.mis-a-jour");
            return;
        }

        foreach (var portData in eventData.Ports)
        {
            var portExistant = await _portRepository.GetByIdAsync(portData.Id, cancellationToken);

            if (portExistant == null)
            {
                var nouveauPort = new Port
                {
                    Id = portData.Id,
                    Code = portData.Code,
                    Nom = portData.Nom,
                    PaysId = portData.PaysId,
                    Type = portData.Type,
                    Actif = portData.Actif,
                    CreeLe = DateTime.UtcNow,
                    CreePar = "SYSTEM"
                };
                await _portRepository.AddAsync(nouveauPort, cancellationToken);
            }
            else
            {
                portExistant.Code = portData.Code;
                portExistant.Nom = portData.Nom;
                portExistant.PaysId = portData.PaysId;
                portExistant.Type = portData.Type;
                portExistant.Actif = portData.Actif;
                portExistant.ModifierLe = DateTime.UtcNow;
                portExistant.ModifiePar = "SYSTEM";
                _portRepository.Update(portExistant);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Synchronisé {Count} ports depuis événement RabbitMQ", eventData.Ports.Count());
    }

    private async Task HandleAeroportMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        // Structure similaire à HandlePortMisAJourAsync
        _logger.LogInformation("Événement referentiel.aeroport.mis-a-jour reçu (à implémenter selon structure exacte)");
        await Task.CompletedTask;
    }

    private async Task HandleDeviseMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        var eventData = JsonSerializer.Deserialize<ReferentielEventData<DeviseData>>(messageBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (eventData?.Devises == null || !eventData.Devises.Any())
        {
            _logger.LogWarning("Aucune devise dans l'événement referentiel.devise.mis-a-jour");
            return;
        }

        foreach (var deviseData in eventData.Devises)
        {
            var deviseExistant = await _deviseRepository.GetByIdAsync(deviseData.Id, cancellationToken);

            if (deviseExistant == null)
            {
                var nouvelleDevise = new Devise
                {
                    Id = deviseData.Id,
                    Code = deviseData.Code,
                    Nom = deviseData.Nom,
                    Actif = deviseData.Actif,
                    CreeLe = DateTime.UtcNow,
                    CreePar = "SYSTEM"
                };
                await _deviseRepository.AddAsync(nouvelleDevise, cancellationToken);
            }
            else
            {
                deviseExistant.Code = deviseData.Code;
                deviseExistant.Nom = deviseData.Nom;
                deviseExistant.Actif = deviseData.Actif;
                deviseExistant.ModifierLe = DateTime.UtcNow;
                deviseExistant.ModifiePar = "SYSTEM";
                _deviseRepository.Update(deviseExistant);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Synchronisé {Count} devises depuis événement RabbitMQ", eventData.Devises.Count());
    }

    private async Task HandleModuleMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Événement referentiel.module.mis-a-jour reçu (à implémenter selon structure exacte)");
        await Task.CompletedTask;
    }

    private async Task HandleIncotermMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Événement referentiel.incoterm.mis-a-jour reçu (à implémenter selon structure exacte)");
        await Task.CompletedTask;
    }

    private async Task HandleBureauDedouanementMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Événement referentiel.bureau-dedouanement.mis-a-jour reçu (à implémenter selon structure exacte)");
        await Task.CompletedTask;
    }

    private async Task HandleUniteStatistiqueMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Événement referentiel.unite-statistique.mis-a-jour reçu (à implémenter selon structure exacte)");
        await Task.CompletedTask;
    }

    private async Task HandleDepartementMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Événement referentiel.departement.mis-a-jour reçu (à implémenter selon structure exacte)");
        await Task.CompletedTask;
    }

    private async Task HandleEtatMisAJourAsync(string messageBody, CancellationToken cancellationToken)
    {
        var eventData = JsonSerializer.Deserialize<ReferentielEventData<EtatData>>(messageBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var etats = eventData?.Etats ?? eventData?.Items;
        if (etats == null || !etats.Any())
        {
            _logger.LogWarning("Aucun état dans l'événement referentiel.etat.mis-a-jour");
            return;
        }

        foreach (var data in etats)
        {
            if (string.IsNullOrWhiteSpace(data.Code))
            {
                _logger.LogWarning("État référentiel {Id} sans Code — ignoré", data.Id);
                continue;
            }

            var existing = await _etatRepository.GetByIdAsync(data.Id, cancellationToken)
                ?? await _etatRepository.GetByCodeAsync(data.Code, cancellationToken);

            if (existing == null)
            {
                await _etatRepository.AddAsync(new Etat
                {
                    Id = data.Id == Guid.Empty ? Guid.NewGuid() : data.Id,
                    Code = data.Code.Trim(),
                    Libelle = string.IsNullOrWhiteSpace(data.Libelle) ? data.Code : data.Libelle,
                    Description = data.Description,
                    CodeEcran = data.CodeEcran,
                    Domaine = data.Domaine ?? "COMMUN",
                    TypeEtat = data.TypeEtat ?? "METIER",
                    Actif = data.Actif,
                    CreeLe = DateTime.UtcNow,
                    CreePar = "SYSTEM"
                }, cancellationToken);
            }
            else
            {
                existing.Code = data.Code.Trim();
                existing.Libelle = string.IsNullOrWhiteSpace(data.Libelle) ? existing.Libelle : data.Libelle;
                existing.Description = data.Description ?? existing.Description;
                existing.CodeEcran = data.CodeEcran ?? existing.CodeEcran;
                existing.Domaine = data.Domaine ?? existing.Domaine;
                existing.TypeEtat = data.TypeEtat ?? existing.TypeEtat;
                existing.Actif = data.Actif;
                existing.ModifierLe = DateTime.UtcNow;
                existing.ModifiePar = "SYSTEM";
                _etatRepository.Update(existing);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("États synchronisés via referentiel.etat.mis-a-jour ({Count})", etats.Count());
    }

    // DTOs pour les événements
    private class ReferentielEventData<T>
    {
        public IEnumerable<T>? Pays { get; set; }
        public IEnumerable<T>? Ports { get; set; }
        public IEnumerable<T>? Devises { get; set; }
        public IEnumerable<T>? Etats { get; set; }
        public IEnumerable<T>? Items { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    private class PaysData
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public bool Actif { get; set; }
    }

    private class PortData
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public Guid? PaysId { get; set; }
        public string? Type { get; set; }
        public bool Actif { get; set; }
    }

    private class DeviseData
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public bool Actif { get; set; }
    }

    private class EtatData
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Libelle { get; set; }
        public string? Description { get; set; }
        public string? CodeEcran { get; set; }
        public string? Domaine { get; set; }
        public string? TypeEtat { get; set; }
        public bool Actif { get; set; } = true;
    }
}
