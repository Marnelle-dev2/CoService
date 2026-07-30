using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Shared.Constants;

namespace COService.Application.Services;

public class EtatService : IEtatService
{
    private readonly IEtatRepository _repository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly (string Code, string Libelle)[] WorkflowEtats =
    [
        (StatutsCertificats.Elabore, "Élaboré"),
        (StatutsCertificats.Soumis, "Soumis"),
        (StatutsCertificats.Controle, "Contrôlé"),
        (StatutsCertificats.Approuve, "Approuvé"),
        (StatutsCertificats.Valide, "Validé"),
        (StatutsCertificats.Rejete, "Rejeté"),
        (StatutsCertificats.Modification, "Modification"),
        (StatutsCertificats.FormuleASoumise, "Formule A soumise"),
        (StatutsCertificats.FormuleAControlee, "Formule A contrôlée"),
        (StatutsCertificats.FormuleAApprouvee, "Formule A approuvée"),
        (StatutsCertificats.FormuleAValidee, "Formule A validée")
    ];

    public EtatService(
        IEtatRepository repository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
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
        foreach (var (code, libelle) in WorkflowEtats)
        {
            if (await _repository.ExistsAsync(code, cancellationToken))
                continue;

            var etat = new Etat
            {
                Id = Guid.NewGuid(),
                Code = code,
                Libelle = libelle,
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
}
