using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Shared.Constants;

namespace COService.Application.Services;

/// <summary>
/// Service pour la gestion des certificats d'origine
/// </summary>
public class CertificatOrigineService : ICertificatOrigineService
{
    private readonly ICertificatOrigineRepository _repository;
    private readonly ICertificatLigneRepository _ligneRepository;
    private readonly IEtatRepository _etatRepository;
    private readonly INumeroGenerationService _numeroGenerationService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CertificatOrigineService(
        ICertificatOrigineRepository repository,
        ICertificatLigneRepository ligneRepository,
        IEtatRepository etatRepository,
        INumeroGenerationService numeroGenerationService,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _ligneRepository = ligneRepository;
        _etatRepository = etatRepository;
        _numeroGenerationService = numeroGenerationService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<CertificatOrigineDto> CreerCertificatAsync(CreerCertificatOrigineDto dto, string? utilisateur = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.PartenaireNIU))
        {
            throw new InvalidOperationException("Le partenaire (chambre de commerce) est obligatoire pour créer un certificat.");
        }

        var certificat = _mapper.Map<CertificatOrigine>(dto);
        certificat.CreePar = utilisateur;
        certificat.CreeLe = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(dto.CertificateNo))
        {
            certificat.CertificateNo = await _numeroGenerationService.GenererNumeroCertificatAsync(
                dto.PartenaireNIU.Trim(),
                certificat.Id,
                dto.PartenaireNom,
                cancellationToken);
        }
        else if (await _repository.ExistsAsync(dto.CertificateNo, cancellationToken))
        {
            throw new InvalidOperationException($"Un certificat avec le numéro {dto.CertificateNo} existe déjà.");
        }

        // Assigner l'état "Élaboré" par défaut lors de la création
        var etatElabore = await _etatRepository.GetByCodeAsync(StatutsCertificats.Elabore, cancellationToken);
        if (etatElabore == null)
        {
            throw new InvalidOperationException($"L'état '{StatutsCertificats.Elabore}' est introuvable. Appelez POST /api/etats/seed-workflow ou POST /api/etats/sync-referentiel.");
        }
        certificat.EtatCode = etatElabore.Code;

        // Créer les lignes si fournies
        if (dto.CertificatLignes.Any())
        {
            foreach (var ligneDto in dto.CertificatLignes)
            {
                var ligne = _mapper.Map<CertificatLigne>(ligneDto);
                ligne.CertificatId = certificat.Id;
                ligne.CreePar = utilisateur;
                ligne.CreeLe = DateTime.UtcNow;
                certificat.CertificatLignes.Add(ligne);
            }
        }

        await _repository.AddAsync(certificat, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<IEnumerable<CertificatOrigineDto>> GetAllCertificatsAsync(CancellationToken cancellationToken = default)
    {
        var certificats = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CertificatOrigineDto>>(certificats);
    }

    public async Task<CertificatOrigineDto?> GetCertificatByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var certificat = await _repository.GetByIdAsync(id, cancellationToken);
        return certificat == null ? null : _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto?> GetCertificatByNoAsync(string certificateNo, CancellationToken cancellationToken = default)
    {
        var certificat = await _repository.GetByCertificateNoAsync(certificateNo, cancellationToken);
        return certificat == null ? null : _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task<CertificatOrigineDto> ModifierCertificatAsync(Guid id, ModifierCertificatOrigineDto dto, string? utilisateur = null, CancellationToken cancellationToken = default)
    {
        var certificat = await _repository.GetByIdAsync(id, cancellationToken);
        if (certificat == null)
        {
            throw new KeyNotFoundException($"Certificat avec l'ID {id} introuvable.");
        }

        _mapper.Map(dto, certificat);
        certificat.ModifiePar = utilisateur;
        certificat.ModifierLe = DateTime.UtcNow;

        _repository.Update(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CertificatOrigineDto>(certificat);
    }

    public async Task SupprimerCertificatAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var certificat = await _repository.GetByIdAsync(id, cancellationToken);
        if (certificat == null)
        {
            throw new KeyNotFoundException($"Certificat avec l'ID {id} introuvable.");
        }

        _repository.Remove(certificat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<CertificatOrigineDto>> GetCertificatsByExportateurAsync(string exportateur, CancellationToken cancellationToken = default)
    {
        var certificats = await _repository.GetByExportateurAsync(exportateur, cancellationToken);
        return _mapper.Map<IEnumerable<CertificatOrigineDto>>(certificats);
    }

    public async Task<IEnumerable<CertificatOrigineDto>> GetCertificatsByStatutAsync(string statut, CancellationToken cancellationToken = default)
    {
        var certificats = await _repository.GetByEtatCodeAsync(statut, cancellationToken);
        return _mapper.Map<IEnumerable<CertificatOrigineDto>>(certificats);
    }

    public async Task<IEnumerable<CertificatOrigineDto>> GetCertificatsByPaysDestinationAsync(string paysDestination, CancellationToken cancellationToken = default)
    {
        var certificats = await _repository.GetByPaysDestinationAsync(paysDestination, cancellationToken);
        return _mapper.Map<IEnumerable<CertificatOrigineDto>>(certificats);
    }
}
