using AutoMapper;
using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Application.Services;
using COService.Application.Messaging;
using COService.Shared.Events;
using COService.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace COService.Application.Services;

/// <summary>
/// Service principal pour gérer les workflows de validation des certificats
/// Délègue aux services spécifiques selon la chambre de commerce
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly ICertificatOrigineRepository _certificatRepository;
    private readonly IEtatRepository _etatRepository;
    private readonly ICommentaireRepository _commentaireRepository;
    private readonly IAuthService _authService;
    private readonly ICertificateEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkflowService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly WorkflowPointeNoireService _pointeNoireService;
    private readonly WorkflowOuessoService _ouessoService;

    public WorkflowService(
        ICertificatOrigineRepository certificatRepository,
        IEtatRepository etatRepository,
        ICommentaireRepository commentaireRepository,
        IAuthService authService,
        ICertificateEventPublisher eventPublisher,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<WorkflowService> logger,
        IServiceProvider serviceProvider)
    {
        _certificatRepository = certificatRepository;
        _etatRepository = etatRepository;
        _commentaireRepository = commentaireRepository;
        _authService = authService;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _serviceProvider = serviceProvider;
        
        // Initialiser les services spécifiques
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        _pointeNoireService = new WorkflowPointeNoireService(
            certificatRepository, etatRepository, commentaireRepository,
            authService, eventPublisher, notificationService, mapper, unitOfWork, logger);
        
        _ouessoService = new WorkflowOuessoService(
            certificatRepository, etatRepository, commentaireRepository,
            authService, eventPublisher, notificationService, mapper, unitOfWork, logger);
    }

    /// <summary>
    /// Détermine le service de workflow à utiliser selon la chambre de commerce
    /// </summary>
    private IWorkflowChambreService GetWorkflowService(CertificatOrigine certificat)
    {
        var codePartenaire = ChambresCommerce.ResolveWorkflowPartenaireCode(
            certificat.PartenaireNIU,
            certificat.PartenaireNom);

        if (ChambresCommerce.EstPointeNoire(codePartenaire))
        {
            return _pointeNoireService;
        }

        if (ChambresCommerce.EstOuesso(codePartenaire))
        {
            return _ouessoService;
        }

        throw new InvalidOperationException(
            $"Chambre de commerce non reconnue pour le partenaire: {certificat.PartenaireNIU}");
    }

    public async Task<CertificatOrigineDto> SoumettreCertificatAsync(Guid certificatId, string userId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var workflowService = GetWorkflowService(certificat);
        return await workflowService.SoumettreCertificatAsync(certificatId, userId, cancellationToken);
    }

    public async Task<CertificatOrigineDto> ControleCertificatAsync(Guid certificatId, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var workflowService = GetWorkflowService(certificat);
        return await workflowService.ControleCertificatAsync(certificatId, userId, password, cancellationToken);
    }

    public async Task<CertificatOrigineDto> ApprouverCertificatAsync(Guid certificatId, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var workflowService = GetWorkflowService(certificat);
        return await workflowService.ApprouverCertificatAsync(certificatId, userId, password, cancellationToken);
    }

    public async Task<CertificatOrigineDto> ValiderCertificatAsync(Guid certificatId, string userId, string password, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var workflowService = GetWorkflowService(certificat);
        return await workflowService.ValiderCertificatAsync(certificatId, userId, password, cancellationToken);
    }

    public async Task<CertificatOrigineDto> RejeterCertificatAsync(Guid certificatId, string userId, string password, string commentaire, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var workflowService = GetWorkflowService(certificat);
        return await workflowService.RejeterCertificatAsync(certificatId, userId, password, commentaire, cancellationToken);
    }

    public async Task<CertificatOrigineDto> DemanderModificationAsync(Guid certificatId, string userId, string commentaire, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var workflowService = GetWorkflowService(certificat);
        return await workflowService.DemanderModificationAsync(certificatId, userId, commentaire, cancellationToken);
    }

    public async Task<bool> EstTransitionValideAsync(Guid certificatId, string codeNouveauStatut, string userId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var workflowService = GetWorkflowService(certificat);
        return await workflowService.EstTransitionValideAsync(certificatId, codeNouveauStatut, userId, cancellationToken);
    }

    public async Task<List<string>> GetTransitionsPossiblesAsync(Guid certificatId, string userId, CancellationToken cancellationToken = default)
    {
        var certificat = await _certificatRepository.GetByIdAsync(certificatId, cancellationToken)
            ?? throw new KeyNotFoundException($"Certificat {certificatId} introuvable");

        var workflowService = GetWorkflowService(certificat);
        return await workflowService.GetTransitionsPossiblesAsync(certificatId, userId, cancellationToken);
    }
}
