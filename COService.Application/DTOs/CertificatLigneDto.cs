namespace COService.Application.DTOs;

/// <summary>
/// DTO pour une ligne de certificat
/// </summary>
public class CertificatLigneDto
{
    public Guid Id { get; set; }
    public Guid CertificatId { get; set; }
    public string? HSCode { get; set; }
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
    public DateTime? CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }
}

/// <summary>
/// DTO pour créer une ligne de certificat
/// </summary>
public class CreerCertificatLigneDto
{
    public string? HSCode { get; set; }
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
}

/// <summary>
/// DTO pour modifier une ligne de certificat
/// </summary>
public class ModifierCertificatLigneDto
{
    public string? HSCode { get; set; }
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
}
