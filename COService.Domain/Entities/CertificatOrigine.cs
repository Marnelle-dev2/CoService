using COService.Domain.Enums;

namespace COService.Domain.Entities;

/// <summary>
/// Entité représentant un certificat d'origine
/// </summary>
public class CertificatOrigine
{
    /// <summary>
    /// Identifiant unique du certificat
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Numéro du certificat (unique)
    /// </summary>
    public string CertificateNo { get; set; } = string.Empty;

    /// <summary>
    /// Référence à l'exportateur
    /// </summary>
    public Guid? ExportateurId { get; set; }

    /// <summary>
    /// Exportateur
    /// </summary>
    public Exportateur? Exportateur { get; set; }

    /// <summary>
    /// Référence au partenaire/CCI
    /// </summary>
    public Guid? PartenaireId { get; set; }

    /// <summary>
    /// Partenaire/CCI
    /// </summary>
    public Partenaire? Partenaire { get; set; }

    /// <summary>
    /// Référence au pays de destination
    /// </summary>
    public Guid? PaysDestinationId { get; set; }

    /// <summary>
    /// Pays de destination
    /// </summary>
    public Pays? PaysDestination { get; set; }

    /// <summary>
    /// Référence au port de sortie
    /// </summary>
    public Guid? PortSortieId { get; set; }

    /// <summary>
    /// Port de sortie
    /// </summary>
    public Port? PortSortie { get; set; }

    /// <summary>
    /// Référence au port côté Congo
    /// </summary>
    public Guid? PortCongoId { get; set; }

    /// <summary>
    /// Port côté Congo
    /// </summary>
    public Port? PortCongo { get; set; }

    /// <summary>
    /// Référence au type de certificat
    /// </summary>
    public Guid? TypeId { get; set; }

    /// <summary>
    /// Type de certificat (EUR.1, Formule A, etc.)
    /// </summary>
    public CertificateType? Type { get; set; }

    /// <summary>
    /// Formule du certificat
    /// </summary>
    public string? Formule { get; set; }

    /// <summary>
    /// Mandataire (optionnel)
    /// </summary>
    public string? Mandataire { get; set; }

    /// <summary>
    /// Référence au statut du certificat
    /// </summary>
    public Guid? StatutCertificatId { get; set; }

    /// <summary>
    /// Statut du certificat
    /// </summary>
    public StatutCertificat? StatutCertificat { get; set; }

    /// <summary>
    /// Observation interne
    /// </summary>
    public string? Observation { get; set; }

    /// <summary>
    /// Référence au destinataire (carnet d'adresses)
    /// </summary>
    public Guid? CarnetAdresseId { get; set; }

    /// <summary>
    /// Destinataire (carnet d'adresses)
    /// </summary>
    public CarnetAdresse? CarnetAdresse { get; set; }

    /// <summary>
    /// Nom du navire
    /// </summary>
    public string? Navire { get; set; }

    /// <summary>
    /// Référence aux documents attachés (microservice Documents)
    /// </summary>
    public Guid? DocumentsId { get; set; }

    /// <summary>
    /// URL de la facture stockée dans MinIO
    /// </summary>
    public string? FactureUrl { get; set; }

    /// <summary>
    /// URLs des pièces justificatives stockées dans MinIO (format JSON)
    /// </summary>
    public string? PiecesJustificativesUrls { get; set; }

    /// <summary>
    /// URL du certificat généré stocké dans MinIO
    /// </summary>
    public string? CertificatGenereUrl { get; set; }

    // Champs d'audit
    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreeLe { get; set; }

    /// <summary>
    /// Utilisateur créateur
    /// </summary>
    public string? CreePar { get; set; }

    /// <summary>
    /// Date de dernière modification
    /// </summary>
    public DateTime? ModifierLe { get; set; }

    /// <summary>
    /// Utilisateur ayant modifié
    /// </summary>
    public string? ModifiePar { get; set; }

    // Navigation properties
    /// <summary>
    /// Lignes de produits du certificat
    /// </summary>
    public ICollection<CertificateLine> CertificateLines { get; set; } = new List<CertificateLine>();

    /// <summary>
    /// Validations du certificat
    /// </summary>
    public ICollection<CertificateValidation> CertificateValidations { get; set; } = new List<CertificateValidation>();

    /// <summary>
    /// Commentaires sur le certificat
    /// </summary>
    public ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    /// <summary>
    /// Référence à l'abonnement (optionnel, un certificat peut ne pas avoir d'abonnement)
    /// </summary>
    public Guid? AbonnementId { get; set; }

    /// <summary>
    /// Abonnement auquel ce certificat est rattaché
    /// </summary>
    public Abonnement? Abonnement { get; set; }

    /// <summary>
    /// Référence à la zone de production
    /// </summary>
    public Guid? ZoneProductionId { get; set; }

    /// <summary>
    /// Zone de production
    /// </summary>
    public ZoneProduction? ZoneProduction { get; set; }

    /// <summary>
    /// Référence au bureau de douane
    /// </summary>
    public Guid? BureauDedouanementId { get; set; }

    /// <summary>
    /// Bureau de douane
    /// </summary>
    public BureauDedouanement? BureauDedouanement { get; set; }

    /// <summary>
    /// Référence au module de transport
    /// </summary>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Module de transport
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Référence à la devise
    /// </summary>
    public Guid? DeviseId { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public Devise? Devise { get; set; }
}

