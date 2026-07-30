namespace COService.Domain.Entities;

/// <summary>
/// Entité représentant un destinataire (carnet d'adresses)
/// Synchronisée depuis le microservice Référentiel global.
/// </summary>
public class CarnetAdresse
{
    /// <summary>
    /// Identifiant unique du destinataire
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Code unique (clé de sync référentiel)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nom du destinataire
    /// </summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Raison sociale
    /// </summary>
    public string? RaisonSociale { get; set; }

    /// <summary>
    /// Coordonnées (téléphone, email, etc.)
    /// </summary>
    public string? Coordonnees { get; set; }

    /// <summary>
    /// Adresse complète
    /// </summary>
    public string? Adresse { get; set; }

    // Champs d'audit

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime? CreeLe { get; set; }

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

    /// <summary>
    /// Certificats associés à ce destinataire
    /// </summary>
    public ICollection<CertificatOrigine> Certificats { get; set; } = new List<CertificatOrigine>();
}

