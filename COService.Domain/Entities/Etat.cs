namespace COService.Domain.Entities;

/// <summary>
/// Copie locale de l'état référentiel (tous statuts projet).
/// </summary>
public class Etat
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CodeEcran { get; set; }

    public DateTime? CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }

    public ICollection<CertificatOrigine> Certificats { get; set; } = new List<CertificatOrigine>();
}
