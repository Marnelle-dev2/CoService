namespace COService.Infrastructure.Services;

/// <summary>
/// Synchronise le MS Référentiel vers les tables locales CO (résilience hors-ligne).
/// </summary>
public interface IReferentielSyncService
{
    /// <summary>Indique si la copie locale pays est vide (cold start).</summary>
    Task<bool> IsLocalEmptyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tire pays, ports, aéroports, devises, modes, incoterms, départements,
    /// bureaux, unités (et positions si demandé) depuis le MS Référentiel.
    /// Chaque famille est isolée : un échec n'annule pas les autres.
    /// </summary>
    Task<ReferentielSyncResult> SyncAllAsync(
        bool includePositions = false,
        CancellationToken cancellationToken = default);
}

public class ReferentielSyncResult
{
    public int Pays { get; set; }
    public int Ports { get; set; }
    public int Aeroports { get; set; }
    public int Devises { get; set; }
    public int ModesTransport { get; set; }
    public int Incoterms { get; set; }
    public int Departements { get; set; }
    public int Bureaux { get; set; }
    public int Unites { get; set; }
    public int Corridors { get; set; }
    public int Positions { get; set; }
    public List<string> Errors { get; set; } = [];
    public bool HasAnySuccess =>
        Pays + Ports + Aeroports + Devises + ModesTransport + Incoterms
        + Departements + Bureaux + Unites + Corridors + Positions > 0;
}
