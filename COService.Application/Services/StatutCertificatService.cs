using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Shared.Constants;

namespace COService.Application.Services;

public class StatutCertificatService : IStatutCertificatService
{
    private readonly IStatutCertificatRepository _repository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly (string Code, string Nom)[] WorkflowStatuts =
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

    public StatutCertificatService(
        IStatutCertificatRepository repository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<StatutCertificatDto>> GetAllStatutsAsync(CancellationToken cancellationToken = default)
    {
        var statuts = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<StatutCertificatDto>>(statuts);
    }

    public async Task<StatutCertificatDto?> GetStatutByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var statut = await _repository.GetByIdAsync(id, cancellationToken);
        return statut == null ? null : _mapper.Map<StatutCertificatDto>(statut);
    }

    public async Task<StatutCertificatDto?> GetStatutByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var statut = await _repository.GetByCodeAsync(code, cancellationToken);
        return statut == null ? null : _mapper.Map<StatutCertificatDto>(statut);
    }

    public async Task<StatutCertificatDto> CreerStatutAsync(
        CreerStatutCertificatDto dto,
        string? utilisateur = null,
        CancellationToken cancellationToken = default)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _repository.ExistsAsync(code, cancellationToken))
        {
            throw new InvalidOperationException($"Un statut avec le code '{code}' existe déjà.");
        }

        var statut = new StatutCertificat
        {
            Id = Guid.NewGuid(),
            Code = code,
            Nom = dto.Nom.Trim(),
            CreeLe = DateTime.UtcNow,
            CreePar = utilisateur ?? "SIMULATOR"
        };

        await _repository.AddAsync(statut, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<StatutCertificatDto>(statut);
    }

    public async Task<IEnumerable<StatutCertificatDto>> SeedStatutsWorkflowAsync(
        string? utilisateur = null,
        CancellationToken cancellationToken = default)
    {
        var createdAny = false;
        foreach (var (code, nom) in WorkflowStatuts)
        {
            if (await _repository.ExistsAsync(code, cancellationToken))
                continue;

            var statut = new StatutCertificat
            {
                Id = Guid.NewGuid(),
                Code = code,
                Nom = nom,
                CreeLe = DateTime.UtcNow,
                CreePar = utilisateur ?? "SIMULATOR"
            };
            await _repository.AddAsync(statut, cancellationToken);
            createdAny = true;
        }

        if (createdAny)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<IEnumerable<StatutCertificatDto>>(await _repository.GetAllAsync(cancellationToken));
    }
}
