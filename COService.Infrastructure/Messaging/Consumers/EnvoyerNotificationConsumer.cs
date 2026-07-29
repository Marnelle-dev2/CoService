using COService.Shared.Contracts.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Stub local Notification (best effort).
/// </summary>
public class EnvoyerNotificationConsumer : IConsumer<EnvoyerNotificationCommand>
{
    private readonly ILogger<EnvoyerNotificationConsumer> _logger;

    public EnvoyerNotificationConsumer(ILogger<EnvoyerNotificationConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<EnvoyerNotificationCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "[Stub Notification] CO {CertificateNo} validé — facture={NumeroFacture}, pdf={PdfUrl}",
            msg.CertificateNo, msg.NumeroFacture, msg.PdfUrl);
        return Task.CompletedTask;
    }
}
