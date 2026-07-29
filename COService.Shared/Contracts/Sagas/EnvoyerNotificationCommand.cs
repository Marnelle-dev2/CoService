namespace COService.Shared.Contracts.Sagas;

/// <summary>
/// Commande Send (best effort) : Saga → Notification
/// </summary>
public record EnvoyerNotificationCommand
{
    public Guid CertificatId { get; init; }
    public string CertificateNo { get; init; } = string.Empty;
    public string? NumeroFacture { get; init; }
    public string? PdfUrl { get; init; }
}
