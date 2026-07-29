using MassTransit;

namespace COService.Infrastructure.Sagas;

/// <summary>
/// État persisté de la saga post-validation d'un certificat.
/// CorrelationId = CertificatId.
/// </summary>
public class CertificatPostValidationState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public string CertificateNo { get; set; } = string.Empty;
    public Guid? ExportateurId { get; set; }
    public Guid? PartenaireId { get; set; }

    public string? NumeroFacture { get; set; }
    public string? PdfUrl { get; set; }
    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Token MassTransit pour Request Facturation.</summary>
    public Guid? FacturationRequestId { get; set; }

    /// <summary>Token MassTransit pour Request PDF.</summary>
    public Guid? PdfRequestId { get; set; }

    /// <summary>Concurrency optimistic (EF).</summary>
    public int Version { get; set; }
}
