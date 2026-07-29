using COService.Shared.Contracts.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Stub local CO Exchange (consomme l'événement Publish).
/// </summary>
public class CertificatPretPourEchangeConsumer : IConsumer<CertificatPretPourEchangeEvent>
{
    private readonly ILogger<CertificatPretPourEchangeConsumer> _logger;

    public CertificatPretPourEchangeConsumer(ILogger<CertificatPretPourEchangeConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<CertificatPretPourEchangeEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "[Stub CO Exchange] CO {CertificateNo} prêt pour échange (facture={NumeroFacture})",
            msg.CertificateNo, msg.NumeroFacture);
        return Task.CompletedTask;
    }
}
