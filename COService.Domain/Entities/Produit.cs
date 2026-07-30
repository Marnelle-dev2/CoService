namespace COService.Domain.Entities;

/// <summary>
/// Produit (catalogue local CO, unités depuis référentiel).
/// </summary>
public class Produit
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Categorie { get; set; }
    public string? UniteStatistiqueCode { get; set; }
    public string? UniteStatistique { get; set; }
    public bool Actif { get; set; } = true;

    public DateTime? CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }
}
