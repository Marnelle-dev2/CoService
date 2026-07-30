namespace COService.Domain.Entities;

/// <summary>
/// Certificat d'origine — clés métier = codes / NIU (pas de GUID externes).
/// </summary>
public class CertificatOrigine
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
    public Pays? PaysDestination { get; set; }

    public string? PortSortieCode { get; set; }
    public Port? PortSortie { get; set; }

    public string? PortCongoCode { get; set; }
    public Port? PortCongo { get; set; }

    public string? AeroportCode { get; set; }
    public Aeroport? Aeroport { get; set; }

    public string? RouteCode { get; set; }
    public RouteNationale? Route { get; set; }

    public string? CarnetAdresseCode { get; set; }
    public CarnetAdresse? CarnetAdresse { get; set; }

    public string? ModuleCode { get; set; }
    public Module? Module { get; set; }

    public string? DeviseCode { get; set; }
    public Devise? Devise { get; set; }

    public string? BureauDedouanementCode { get; set; }
    public BureauDedouanement? BureauDedouanement { get; set; }

    // État (copie référentiel)
    public string? EtatCode { get; set; }
    public Etat? Etat { get; set; }

    // Interne CO
    public Guid? TypeId { get; set; }
    public CertificateType? Type { get; set; }

    public string? ZoneProductionCode { get; set; }
    public ZoneProduction? ZoneProduction { get; set; }

    public string? BattantPavillonCode { get; set; }
    public BattantPavillon? BattantPavillon { get; set; }

    // Paiement (pas de table locale)
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
    public Abonnement? Abonnement { get; set; }

    public DateTime CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }

    public ICollection<CertificatLigne> CertificatLignes { get; set; } = new List<CertificatLigne>();
    public ICollection<CertificateValidation> CertificateValidations { get; set; } = new List<CertificateValidation>();
    public ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();
}
