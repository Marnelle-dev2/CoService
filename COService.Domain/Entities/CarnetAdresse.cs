namespace COService.Domain.Entities;

/// <summary>
/// Ancienne copie locale du carnet (dépréciée).
/// Source de vérité = MS Référentiel /api/carnetadresses (scoped organisation).
/// Conservée pour compatibilité schéma / données historiques.
/// </summary>
public class CarnetAdresse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? RaisonSociale { get; set; }
    public string? Coordonnees { get; set; }
    public string? Adresse { get; set; }

    public DateTime? CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }
}

