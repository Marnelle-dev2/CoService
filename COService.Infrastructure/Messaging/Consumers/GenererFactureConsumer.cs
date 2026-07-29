using COService.Shared.Contracts.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Stub local Facturation (remplacé plus tard par le vrai MS Facturation).
/// </summary>
public class GenererFactureConsumer : IConsumer<GenererFactureCommand>
{
    private readonly ILogger<GenererFactureConsumer> _logger;

    public GenererFactureConsumer(ILogger<GenererFactureConsumer> logger) => _logger = logger;

    public async Task Consume(ConsumeContext<GenererFactureCommand> context)
    {
        var msg = context.Message;
        var numero = $"FAC-{DateTime.UtcNow:yyyyMMdd}-{msg.CertificateNo}";

        _logger.LogInformation(
            "[Stub Facturation] Facture {NumeroFacture} pour CO {CertificateNo}",
            numero, msg.CertificateNo);

        await context.RespondAsync(new FactureGenereeResponse
        {
            CertificatId = msg.CertificatId,
            NumeroFacture = numero
        });
    }
}
