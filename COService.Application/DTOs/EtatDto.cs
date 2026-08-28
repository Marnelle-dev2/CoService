namespace COService.Application.DTOs;

/// <summary>
/// DTO pour un état (statut) de certificat — modèle V2.
/// </summary>
public class EtatDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CodeEcran { get; set; }
    public string? Domaine { get; set; }
    public string? TypeEtat { get; set; }
    public bool Actif { get; set; } = true;
    public DateTime? CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }
}

public class CreerEtatDto
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CodeEcran { get; set; }
    public string? Domaine { get; set; }
    public string? TypeEtat { get; set; }
    public bool Actif { get; set; } = true;
}

/// <summary>
/// Snapshot distant MS Référentiel (/api/etats) — Code numérique V2.
/// </summary>
public class ReferentielEtatRemoteDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Libelle { get; set; }
    public string? Description { get; set; }
    public string? CodeEcran { get; set; }
    public string? UsageUI { get; set; }
    public string? Domaine { get; set; }
    public string? TypeEtat { get; set; }
    public bool Actif { get; set; } = true;
    public string? CreerPar { get; set; }
    public string? ModifierPar { get; set; }
    public DateTime? CreerLe { get; set; }
    public DateTime? ModifierLe { get; set; }
}

public class SyncEtatsResultDto
{
    public int Upserted { get; set; }
    public int Skipped { get; set; }
    public int RemoteCount { get; set; }
    public IEnumerable<EtatDto> Etats { get; set; } = Array.Empty<EtatDto>();
}
