namespace COService.Shared.Contracts.Sagas;

/// <summary>
/// Événement Publish (fan-out) : Saga → CO Exchange (+ autres intéressés)
/// </summary>
public record CertificatPretPourEchangeEvent
{
    public Guid CertificatId { get; init; }
    public string CertificateNo { get; init; } = string.Empty;
    public Guid? ExportateurId { get; init; }
    public Guid? PartenaireId { get; init; }
    public string? NumeroFacture { get; init; }
    public string? PdfUrl { get; init; }
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
}
