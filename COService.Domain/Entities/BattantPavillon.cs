namespace COService.Domain.Entities;

/// <summary>
/// Pavillon du navire (table locale CO).
/// </summary>
public class BattantPavillon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public bool Actif { get; set; } = true;

    public DateTime? CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }

    public ICollection<CertificatOrigine> Certificats { get; set; } = new List<CertificatOrigine>();
}
