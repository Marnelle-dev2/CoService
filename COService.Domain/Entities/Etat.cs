namespace COService.Domain.Entities;

/// <summary>
/// Copie locale des états métier (source de vérité : ReferentielService).
/// Aligné sur PROPOSITION DES ETATS DES DOSSIERS DU SEG V2.
/// </summary>
public class Etat
{
    public Guid Id { get; set; }

    /// <summary>Code métier numérique stocké sur les dossiers (ex. 42, 79, 50).</summary>
    public string Code { get; set; } = string.Empty;

    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CodeEcran { get; set; }

    /// <summary>COMMUN, CERTIFICAT_ORIGINE, DECLARATION, ASSURANCE, …</summary>
    public string? Domaine { get; set; }

    /// <summary>METIER, TECHNIQUE, FINANCIER, SYSTEME</summary>
    public string? TypeEtat { get; set; }

    public bool Actif { get; set; } = true;

    public DateTime? CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }

    public ICollection<CertificatOrigine> Certificats { get; set; } = new List<CertificatOrigine>();
}
