namespace COService.Shared.Contracts.Sagas;

/// <summary>
/// Commande Request/Response : Saga → Document (PDF + stockage)
/// </summary>
public record GenererPdfCommand
{
    public Guid CertificatId { get; init; }
    public string CertificateNo { get; init; } = string.Empty;
    public string? NumeroFacture { get; init; }
}

public record PdfGenereResponse
{
    public Guid CertificatId { get; init; }
    public string PdfUrl { get; init; } = string.Empty;
}
