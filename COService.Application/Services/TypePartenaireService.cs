using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;

namespace COService.Application.Services;

public class TypePartenaireService : ITypePartenaireService
{
    private readonly ITypePartenaireRepository _repository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public TypePartenaireService(
        ITypePartenaireRepository repository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<TypePartenaireDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var typePartenaire = await _repository.GetByIdAsync(id, cancellationToken);
        return typePartenaire == null ? null : _mapper.Map<TypePartenaireDto>(typePartenaire);
    }

    public async Task<TypePartenaireDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var typePartenaire = await _repository.GetByCodeAsync(code, cancellationToken);
        return typePartenaire == null ? null : _mapper.Map<TypePartenaireDto>(typePartenaire);
    }

    public async Task<IEnumerable<TypePartenaireDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var typesPartenaires = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TypePartenaireDto>>(typesPartenaires);
    }

    public async Task<IEnumerable<TypePartenaireDto>> GetActifsAsync(CancellationToken cancellationToken = default)
    {
        var typesPartenaires = await _repository.GetActifsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TypePartenaireDto>>(typesPartenaires);
    }

    public async Task<TypePartenaireDto> CreerAsync(
        CreerTypePartenaireDto dto,
        string? utilisateur = null,
        CancellationToken cancellationToken = default)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _repository.ExistsByCodeAsync(code, cancellationToken))
        {
            throw new InvalidOperationException($"Un type de partenaire avec le code '{code}' existe déjà.");
        }

        var entity = new TypePartenaire
        {
            Id = Guid.NewGuid(),
            Code = code,
            Nom = dto.Nom.Trim(),
            Description = dto.Description,
            Actif = dto.Actif,
            CreeLe = DateTime.UtcNow,
            CreePar = utilisateur ?? "SIMULATOR"
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TypePartenaireDto>(entity);
    }
}
