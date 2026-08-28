using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;

namespace COService.Application.Services;

public class ZoneProductionService : IZoneProductionService
{
    private readonly IZoneProductionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ZoneProductionService(
        IZoneProductionRepository repository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<ZoneProductionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var zone = await _repository.GetByIdAsync(id, cancellationToken);
        return zone == null ? null : _mapper.Map<ZoneProductionDto>(zone);
    }

    public async Task<ZoneProductionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var zone = await _repository.GetByCodeAsync(code, cancellationToken);
        return zone == null ? null : _mapper.Map<ZoneProductionDto>(zone);
    }

    public async Task<IEnumerable<ZoneProductionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var zones = await _repository.GetAllAsync(cancellationToken);
        return zones.Select(z => _mapper.Map<ZoneProductionDto>(z));
    }

    public async Task<IEnumerable<ZoneProductionDto>> GetByPartenaireNIUAsync(
        string partenaireNIU,
        CancellationToken cancellationToken = default)
    {
        var zones = await _repository.GetByPartenaireNIUAsync(partenaireNIU, cancellationToken);
        return zones.Select(z => _mapper.Map<ZoneProductionDto>(z));
    }

    public async Task<ZoneProductionDto> CreerZoneProductionAsync(
        CreerZoneProductionDto dto,
        string? utilisateur = null,
        CancellationToken cancellationToken = default)
    {
        var zone = _mapper.Map<ZoneProduction>(dto);
        zone.Id = Guid.NewGuid();
        zone.CreeLe = DateTime.UtcNow;
        zone.CreePar = utilisateur;

        await _repository.AddAsync(zone, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ZoneProductionDto>(zone);
    }

    public async Task<ZoneProductionDto> ModifierZoneProductionAsync(
        Guid id,
        ModifierZoneProductionDto dto,
        string? utilisateur = null,
        CancellationToken cancellationToken = default)
    {
        var zone = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Zone de production {id} introuvable.");

        if (!string.IsNullOrWhiteSpace(dto.Nom))
            zone.Nom = dto.Nom;
        if (dto.Description != null)
            zone.Description = dto.Description;
        if (dto.PartenaireNIU != null)
            zone.PartenaireNIU = dto.PartenaireNIU;

        zone.ModifierLe = DateTime.UtcNow;
        zone.ModifiePar = utilisateur;

        _repository.Update(zone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ZoneProductionDto>(zone);
    }

    public async Task SupprimerZoneProductionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var zone = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Zone de production {id} introuvable.");

        _repository.Remove(zone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
