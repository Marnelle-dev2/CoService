using COService.Shared.Contracts.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Stub local Document/PDF (remplacé plus tard par Document MS + MinIO réel).
/// </summary>
public class GenererPdfConsumer : IConsumer<GenererPdfCommand>
{
    private readonly ILogger<GenererPdfConsumer> _logger;

    public GenererPdfConsumer(ILogger<GenererPdfConsumer> logger) => _logger = logger;

    public async Task Consume(ConsumeContext<GenererPdfCommand> context)
    {
        var msg = context.Message;
        var url = $"minio://ms-documents/co/{msg.CertificateNo}.pdf";

        _logger.LogInformation(
            "[Stub Document] PDF {PdfUrl} pour CO {CertificateNo}",
            url, msg.CertificateNo);

        await context.RespondAsync(new PdfGenereResponse
        {
            CertificatId = msg.CertificatId,
            PdfUrl = url
        });
    }
}
