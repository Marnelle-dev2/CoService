namespace COService.Domain.Entities;

/// <summary>
/// Ligne de produit d'un certificat d'origine.
/// </summary>
public class CertificatLigne
{
    public Guid Id { get; set; }
    public Guid CertificatId { get; set; }

    /// <summary>Code HS (référentiel).</summary>
    public string? HSCode { get; set; }

    /// <summary>Désignation position tarifaire.</summary>
    public string? PositionTarifaire { get; set; }

    public string? NatureProduit { get; set; }
    public string? Quantite { get; set; }

    public string? UniteStatistiqueCode { get; set; }
    public string? UniteStatistique { get; set; }

    public string? PoidsBrut { get; set; }
    public string? PoidsNet { get; set; }
    public string? ValeurFOB { get; set; }
    public string? Volume { get; set; }

    public string? DeviseCode { get; set; }
    public string? Devise { get; set; }

    public string? ProduitCode { get; set; }
    public Produit? Produit { get; set; }

    public DateTime? CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }

    public CertificatOrigine CertificatOrigine { get; set; } = null!;
}
