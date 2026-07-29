namespace COService.Shared.Contracts.Sagas;

/// <summary>
/// Commande Request/Response : Saga → Facturation
/// </summary>
public record GenererFactureCommand
{
    public Guid CertificatId { get; init; }
    public string CertificateNo { get; init; } = string.Empty;
    public Guid? ExportateurId { get; init; }
    public Guid? PartenaireId { get; init; }
}

public record FactureGenereeResponse
{
    public Guid CertificatId { get; init; }
    public string NumeroFacture { get; init; } = string.Empty;
}
