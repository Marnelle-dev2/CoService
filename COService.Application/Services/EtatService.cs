using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace COService.Application.Services;

public class EtatService : IEtatService
{
    private readonly IEtatRepository _repository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReferentielEtatsClient _referentielEtats;
    private readonly ILogger<EtatService> _logger;

    /// <summary>
    /// Noyau V2 + états domaine CO (bootstrap tant que Ref n'est pas complet).
    /// </summary>
    private static readonly (string Code, string Libelle, string? Description, string? CodeEcran, string Domaine)[] WorkflowEtats =
    [
        (StatutsCertificats.Elabore, "Élaboré", "Demande en cours de saisie", "E", StatutsCertificats.Domaines.Commun),
        (StatutsCertificats.Soumis, "Visa demandé", "Demande soumise aux signataires", "VD", StatutsCertificats.Domaines.Commun),
        (StatutsCertificats.Controle, "Contrôlé", "Certificat contrôlé (circuit CO)", "CC", StatutsCertificats.Domaines.CertificatOrigine),
        (StatutsCertificats.Approuve, "Controller", "Certificat d'origine approuvé", "CO", StatutsCertificats.Domaines.Commun),
        (StatutsCertificats.Valide, "Ouvert", "Demande signée/validée", "O", StatutsCertificats.Domaines.Commun),
        (StatutsCertificats.Rejete, "Visas refusés", "Demande refusée", "VR", StatutsCertificats.Domaines.Commun),
        (StatutsCertificats.Modification, "Modification demandée", "Correction demandée par un signataire", "MD", StatutsCertificats.Domaines.Commun),
        (StatutsCertificats.ModificationSoumise, "Modification soumise", "Correction renvoyée au circuit de signature", "MS", StatutsCertificats.Domaines.Commun),
        (StatutsCertificats.Annule, "Annulé", "Demande annulée", "DA", StatutsCertificats.Domaines.Commun),
        (StatutsCertificats.Cloture, "Clôturé", "Processus fermé", "CL", StatutsCertificats.Domaines.Commun),
        (StatutsCertificats.FormuleASoumise, "Formule A soumise", "Formule A soumise (Ouesso)", "FAS", StatutsCertificats.Domaines.CertificatOrigine),
        (StatutsCertificats.FormuleAControlee, "Formule A contrôlée", "Formule A contrôlée (Ouesso)", "FAC", StatutsCertificats.Domaines.CertificatOrigine),
        (StatutsCertificats.FormuleAApprouvee, "Formule A approuvée", "Formule A approuvée (Ouesso)", "FAA", StatutsCertificats.Domaines.CertificatOrigine),
        (StatutsCertificats.FormuleAValidee, "Formule A validée", "Formule A validée (Ouesso)", "FAV", StatutsCertificats.Domaines.CertificatOrigine)
    ];

    /// <summary>Alias CodeEcran / libellés Ref → code métier V2.</summary>
    private static readonly Dictionary<string, string> CodeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["EL"] = StatutsCertificats.Elabore,
        ["E"] = StatutsCertificats.Elabore,
        ["ELABORER"] = StatutsCertificats.Elabore,
        ["ELABORE"] = StatutsCertificats.Elabore,
        ["VD"] = StatutsCertificats.Soumis,
        ["SOUMIS"] = StatutsCertificats.Soumis,
        ["O"] = StatutsCertificats.Valide,
        ["OUVERT"] = StatutsCertificats.Valide,
        ["VALIDE"] = StatutsCertificats.Valide,
        ["MD"] = StatutsCertificats.Modification,
        ["MS"] = StatutsCertificats.ModificationSoumise,
        ["VR"] = StatutsCertificats.Rejete,
        ["REJETE"] = StatutsCertificats.Rejete,
        ["DA"] = StatutsCertificats.Annule,
        ["CL"] = StatutsCertificats.Cloture,
        ["CO"] = StatutsCertificats.Approuve,
        ["AP"] = StatutsCertificats.Approuve
    };

    public EtatService(
        IEtatRepository repository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IReferentielEtatsClient referentielEtats,
        ILogger<EtatService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _referentielEtats = referentielEtats;
        _logger = logger;
    }

    public async Task<IEnumerable<EtatDto>> GetAllEtatsAsync(CancellationToken cancellationToken = default)
    {
        var etats = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<EtatDto>>(etats);
    }

    public async Task<EtatDto?> GetEtatByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var etat = await _repository.GetByIdAsync(id, cancellationToken);
        return etat == null ? null : _mapper.Map<EtatDto>(etat);
    }

    public async Task<EtatDto?> GetEtatByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var etat = await _repository.GetByCodeAsync(code, cancellationToken);
        return etat == null ? null : _mapper.Map<EtatDto>(etat);
    }

    public async Task<EtatDto> CreerEtatAsync(
        CreerEtatDto dto,
        string? utilisateur = null,
        CancellationToken cancellationToken = default)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _repository.ExistsAsync(code, cancellationToken))
        {
            throw new InvalidOperationException($"Un état avec le code '{code}' existe déjà.");
        }

        var etat = new Etat
        {
            Id = Guid.NewGuid(),
            Code = code,
            Libelle = dto.Libelle.Trim(),
            Description = dto.Description,
            CodeEcran = dto.CodeEcran,
            Domaine = dto.Domaine,
            TypeEtat = dto.TypeEtat ?? StatutsCertificats.Types.Metier,
            Actif = dto.Actif,
            CreeLe = DateTime.UtcNow,
            CreePar = utilisateur ?? "SIMULATOR"
        };

        await _repository.AddAsync(etat, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<EtatDto>(etat);
    }

    public async Task<IEnumerable<EtatDto>> SeedEtatsWorkflowAsync(
        string? utilisateur = null,
        CancellationToken cancellationToken = default)
    {
        var createdAny = false;
        foreach (var (code, libelle, description, codeEcran, domaine) in WorkflowEtats)
        {
            if (await _repository.ExistsAsync(code, cancellationToken))
                continue;

            var etat = new Etat
            {
                Id = Guid.NewGuid(),
                Code = code,
                Libelle = libelle,
                Description = description,
                CodeEcran = codeEcran,
                Domaine = domaine,
                TypeEtat = StatutsCertificats.Types.Metier,
                Actif = true,
                CreeLe = DateTime.UtcNow,
                CreePar = utilisateur ?? "SIMULATOR"
            };
            await _repository.AddAsync(etat, cancellationToken);
            createdAny = true;
        }

        if (createdAny)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<IEnumerable<EtatDto>>(await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<SyncEtatsResultDto> SyncFromReferentielAsync(CancellationToken cancellationToken = default)
    {
        var remote = await _referentielEtats.GetEtatsAsync(cancellationToken);
        var upserted = 0;
        var skipped = 0;

        foreach (var item in remote)
        {
            var code = ResolveCode(item);
            if (string.IsNullOrWhiteSpace(code))
            {
                skipped++;
                _logger.LogWarning(
                    "État référentiel {Id} ignoré : aucun Code / CodeEcran exploitable (Libelle={Libelle})",
                    item.Id, item.Libelle);
                continue;
            }

            var existingById = await _repository.GetByIdAsync(item.Id, cancellationToken);
            var existingByCode = existingById == null
                ? await _repository.GetByCodeAsync(code, cancellationToken)
                : null;

            if (existingById != null)
            {
                ApplyRemote(existingById, item, code, isUpdate: true);
                _repository.Update(existingById);
                upserted++;
            }
            else if (existingByCode != null)
            {
                // Même code métier, Id différent : on met à jour les métadonnées sans changer la PK locale
                ApplyRemote(existingByCode, item, code, isUpdate: true, keepLocalId: true);
                _repository.Update(existingByCode);
                upserted++;
            }
            else
            {
                var etat = new Etat { Id = item.Id };
                ApplyRemote(etat, item, code, isUpdate: false);
                await _repository.AddAsync(etat, cancellationToken);
                upserted++;
            }
        }

        if (upserted > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Sync états référentiel terminée : remote={Remote}, upserted={Upserted}, skipped={Skipped}",
            remote.Count, upserted, skipped);

        return new SyncEtatsResultDto
        {
            RemoteCount = remote.Count,
            Upserted = upserted,
            Skipped = skipped,
            Etats = _mapper.Map<IEnumerable<EtatDto>>(await _repository.GetAllAsync(cancellationToken))
        };
    }

    private static string? ResolveCode(ReferentielEtatRemoteDto item)
    {
        if (!string.IsNullOrWhiteSpace(item.Code))
            return item.Code.Trim();

        foreach (var candidate in new[] { item.CodeEcran, item.Libelle, item.Description })
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;
            var key = candidate.Trim();
            if (CodeAliases.TryGetValue(key, out var mapped))
                return mapped;
            // Code numérique déjà prêt
            if (key.All(char.IsDigit))
                return key;
        }

        return null;
    }

    private static void ApplyRemote(
        Etat etat,
        ReferentielEtatRemoteDto item,
        string code,
        bool isUpdate,
        bool keepLocalId = false)
    {
        if (!keepLocalId && !isUpdate)
            etat.Id = item.Id;

        etat.Code = code;
        etat.Libelle = string.IsNullOrWhiteSpace(item.Libelle) ? code : item.Libelle.Trim();
        etat.Description = item.Description;
        etat.CodeEcran = item.CodeEcran;
        etat.Domaine = string.IsNullOrWhiteSpace(item.Domaine)
            ? StatutsCertificats.Domaines.Commun
            : item.Domaine.Trim();
        etat.TypeEtat = string.IsNullOrWhiteSpace(item.TypeEtat)
            ? StatutsCertificats.Types.Metier
            : item.TypeEtat.Trim();
        etat.Actif = item.Actif;

        if (isUpdate)
        {
            etat.ModifierLe = item.ModifierLe ?? DateTime.UtcNow;
            etat.ModifiePar = item.ModifierPar ?? "REFERENTIEL";
        }
        else
        {
            etat.CreeLe = item.CreerLe ?? DateTime.UtcNow;
            etat.CreePar = item.CreerPar ?? "REFERENTIEL";
            etat.ModifierLe = item.ModifierLe;
            etat.ModifiePar = item.ModifierPar;
        }
    }
}
