namespace COService.Application.DTOs;

/// <summary>
/// DTO pour un certificat d'origine
/// </summary>
public class CertificatOrigineDto
{
    public Guid Id { get; set; }
    public string CertificateNo { get; set; } = string.Empty;

    // Enrôlement (pas de table locale)
    public string? ExportateurNIU { get; set; }
    public string? ExportateurNom { get; set; }
    public string? PartenaireNIU { get; set; }
    public string? PartenaireNom { get; set; }
    public string? MandataireNIU { get; set; }
    public string? MandataireNom { get; set; }

    // Référentiel (copie locale, jointure par Code)
    public string? PaysDestinationCode { get; set; }
    public string? PaysDestinationNom { get; set; }
    public string? PortSortieCode { get; set; }
    public string? PortSortieNom { get; set; }
    public string? PortCongoCode { get; set; }
    public string? PortCongoNom { get; set; }
    public string? AeroportCode { get; set; }
    public string? RouteCode { get; set; }
    public string? CarnetAdresseCode { get; set; }
    public string? ModuleCode { get; set; }
    public string? DeviseCode { get; set; }
    public string? BureauDedouanementCode { get; set; }

    // État
    public string? EtatCode { get; set; }
    public string? EtatLibelle { get; set; }

    // Interne CO
    public Guid? TypeId { get; set; }
    public string? Type { get; set; }

    public string? ZoneProductionCode { get; set; }
    public string? BattantPavillonCode { get; set; }

    // Paiement
    public string? ModePaiementCode { get; set; }
    public string? ModePaiement { get; set; }

    // MinIO
    public string? CodeDocument { get; set; }
    public string? FactureUrl { get; set; }
    public string? PiecesJustificativesUrls { get; set; }
    public string? CertificatGenereUrl { get; set; }

    public string? Formule { get; set; }
    public string? Observation { get; set; }
    public string? Navire { get; set; }

    public Guid? AbonnementId { get; set; }
    public AbonnementDto? Abonnement { get; set; }

    public DateTime CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }

    public List<CertificatLigneDto> CertificatLignes { get; set; } = new();
    public List<CertificateValidationDto> CertificateValidations { get; set; } = new();
    public List<CommentaireDto> Commentaires { get; set; } = new();
}

/// <summary>
/// DTO pour créer un certificat d'origine
/// </summary>
public class CreerCertificatOrigineDto
{
    public string CertificateNo { get; set; } = string.Empty;

    public string? ExportateurNIU { get; set; }
    public string? ExportateurNom { get; set; }
    public string? PartenaireNIU { get; set; }
    public string? PartenaireNom { get; set; }
    public string? MandataireNIU { get; set; }
    public string? MandataireNom { get; set; }

    public string? PaysDestinationCode { get; set; }
    public string? PortSortieCode { get; set; }
    public string? PortCongoCode { get; set; }
    public string? AeroportCode { get; set; }
    public string? RouteCode { get; set; }
    public string? CarnetAdresseCode { get; set; }
    public string? ModuleCode { get; set; }
    public string? DeviseCode { get; set; }
    public string? BureauDedouanementCode { get; set; }

    public Guid? TypeId { get; set; }

    public string? ZoneProductionCode { get; set; }
    public string? BattantPavillonCode { get; set; }

    public string? ModePaiementCode { get; set; }
    public string? ModePaiement { get; set; }

    public string? CodeDocument { get; set; }
    public string? FactureUrl { get; set; }
    public string? PiecesJustificativesUrls { get; set; }
    public string? CertificatGenereUrl { get; set; }

    public string? Formule { get; set; }
    public string? Observation { get; set; }
    public string? Navire { get; set; }

    public Guid? AbonnementId { get; set; }

    public List<CreerCertificatLigneDto> CertificatLignes { get; set; } = new();
}

/// <summary>
/// DTO pour modifier un certificat d'origine
/// </summary>
public class ModifierCertificatOrigineDto
{
    public string? ExportateurNIU { get; set; }
    public string? ExportateurNom { get; set; }
    public string? PartenaireNIU { get; set; }
    public string? PartenaireNom { get; set; }
    public string? MandataireNIU { get; set; }
    public string? MandataireNom { get; set; }

    public string? PaysDestinationCode { get; set; }
    public string? PortSortieCode { get; set; }
    public string? PortCongoCode { get; set; }
    public string? AeroportCode { get; set; }
    public string? RouteCode { get; set; }
    public string? CarnetAdresseCode { get; set; }
    public string? ModuleCode { get; set; }
    public string? DeviseCode { get; set; }
    public string? BureauDedouanementCode { get; set; }

    public Guid? TypeId { get; set; }

    public string? ZoneProductionCode { get; set; }
    public string? BattantPavillonCode { get; set; }

    public string? ModePaiementCode { get; set; }
    public string? ModePaiement { get; set; }

    public string? CodeDocument { get; set; }
    public string? FactureUrl { get; set; }
    public string? PiecesJustificativesUrls { get; set; }
    public string? CertificatGenereUrl { get; set; }

    public string? Formule { get; set; }
    public string? Observation { get; set; }
    public string? Navire { get; set; }

    public Guid? AbonnementId { get; set; }
}
