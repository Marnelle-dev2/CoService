using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Shared.Constants;

namespace COService.Application.Services;

/// <summary>
/// Service pour la gestion des lignes de certificat
/// </summary>
public class CertificatLigneService : ICertificatLigneService
{
    private readonly ICertificatLigneRepository _repository;
    private readonly ICertificatOrigineRepository _certificatRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CertificatLigneService(
        ICertificatLigneRepository repository,
        ICertificatOrigineRepository certificatRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _certificatRepository = certificatRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<CertificatLigneDto> CreerLigneAsync(Guid certificatId, CreerCertificatLigneDto dto, string? utilisateur = null, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            throw new KeyNotFoundException($"Certificat avec l'ID {certificatId} introuvable.");
        }

        StatutsCertificats.EnsureEditableParExportateur(certificat.EtatCode, certificat.CertificateNo);

        var ligne = _mapper.Map<CertificatLigne>(dto);
        ligne.CertificatId = certificatId;
        ligne.CreePar = utilisateur;
        ligne.CreeLe = DateTime.UtcNow;

        await _repository.AddAsync(ligne, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CertificatLigneDto>(ligne);
    }

    public async Task<IEnumerable<CertificatLigneDto>> GetLignesByCertificatIdAsync(Guid certificatId, CancellationToken cancellationToken = default)
    {
        var lignes = await _repository.GetByCertificatIdAsync(certificatId, cancellationToken);
        return _mapper.Map<IEnumerable<CertificatLigneDto>>(lignes);
    }

    public async Task<CertificatLigneDto?> GetLigneByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ligne = await _repository.GetByIdAsync(id, cancellationToken);
        return ligne == null ? null : _mapper.Map<CertificatLigneDto>(ligne);
    }

    public async Task<CertificatLigneDto> ModifierLigneAsync(Guid id, ModifierCertificatLigneDto dto, string? utilisateur = null, CancellationToken cancellationToken = default)
    {
        var ligne = await _repository.GetByIdAsync(id, cancellationToken);
        if (ligne == null)
        {
            throw new KeyNotFoundException($"Ligne avec l'ID {id} introuvable.");
        }

        await EnsureCertificatEditableAsync(ligne.CertificatId, cancellationToken);

        _mapper.Map(dto, ligne);
        ligne.ModifiePar = utilisateur;
        ligne.ModifierLe = DateTime.UtcNow;

        _repository.Update(ligne);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CertificatLigneDto>(ligne);
    }

    public async Task SupprimerLigneAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ligne = await _repository.GetByIdAsync(id, cancellationToken);
        if (ligne == null)
        {
            throw new KeyNotFoundException($"Ligne avec l'ID {id} introuvable.");
        }

        await EnsureCertificatEditableAsync(ligne.CertificatId, cancellationToken);

        _repository.Remove(ligne);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCertificatEditableAsync(Guid certificatId, CancellationToken cancellationToken)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            throw new KeyNotFoundException($"Certificat avec l'ID {certificatId} introuvable.");
        }

        StatutsCertificats.EnsureEditableParExportateur(certificat.EtatCode, certificat.CertificateNo);
    }
}
