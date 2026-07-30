namespace COService.Domain.Entities;

/// <summary>
/// Entité représentant un département
/// Synchronisée depuis le microservice Référentiel Global
/// </summary>
public class Departement
{
    /// <summary>
    /// Identifiant unique du département
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Code unique du département (ex: "PNR", "OUE")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nom du département
    /// </summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Statut d'activation
    /// </summary>
    public bool Actif { get; set; } = true;

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
}
