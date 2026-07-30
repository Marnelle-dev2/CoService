namespace COService.Shared.Events;

/// <summary>
/// Événement publié lorsqu'un certificat est validé
/// Consommé par le microservice Facturation pour générer la facture
/// </summary>
public class CertificatValideEvent
{
    public Guid CertificatId { get; set; }
    public string CertificateNo { get; set; } = string.Empty;
    public string? ExportateurNIU { get; set; }
    public string? PartenaireNIU { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
