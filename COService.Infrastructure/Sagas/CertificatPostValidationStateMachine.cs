using COService.Shared.Contracts.Sagas;
using COService.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Sagas;

/// <summary>
/// Saga post-validation : Facturation (RR) → PDF (RR) → Notification (Send) → Échange (Publish).
/// Déclenchée par <see cref="CertificatValideEvent"/>.
/// </summary>
public class CertificatPostValidationStateMachine : MassTransitStateMachine<CertificatPostValidationState>
{
    public CertificatPostValidationStateMachine(ILogger<CertificatPostValidationStateMachine> logger)
    {
        InstanceState(x => x.CurrentState);

        Event(() => CertificatValide, x =>
        {
            x.CorrelateById(m => m.Message.CertificatId);
            x.SelectId(m => m.Message.CertificatId);
        });

        Request(() => Facturation, r =>
        {
            r.ServiceAddress = new Uri("queue:generer-facture");
            r.Timeout = TimeSpan.FromSeconds(60);
        });

        Request(() => Pdf, r =>
        {
            r.ServiceAddress = new Uri("queue:generer-pdf");
            r.Timeout = TimeSpan.FromSeconds(120);
        });

        Initially(
            When(CertificatValide)
                .Then(ctx =>
                {
                    ctx.Saga.CertificateNo = ctx.Message.CertificateNo;
                    ctx.Saga.ExportateurNIU = ctx.Message.ExportateurNIU;
                    ctx.Saga.PartenaireNIU = ctx.Message.PartenaireNIU;
                    ctx.Saga.CreatedAt = DateTime.UtcNow;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    logger.LogInformation(
                        "Saga démarrée pour CO {CertificateNo} ({CertificatId})",
                        ctx.Saga.CertificateNo, ctx.Saga.CorrelationId);
                })
                .Request(Facturation, ctx => new GenererFactureCommand
                {
                    CertificatId = ctx.Saga.CorrelationId,
                    CertificateNo = ctx.Saga.CertificateNo,
                    ExportateurNIU = ctx.Saga.ExportateurNIU,
                    PartenaireNIU = ctx.Saga.PartenaireNIU
                })
                .TransitionTo(FacturationEnCours)
        );

        During(FacturationEnCours,
            When(Facturation.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.NumeroFacture = ctx.Message.NumeroFacture;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    logger.LogInformation(
                        "Facture {NumeroFacture} OK pour CO {CertificateNo}",
                        ctx.Saga.NumeroFacture, ctx.Saga.CertificateNo);
                })
                .Request(Pdf, ctx => new GenererPdfCommand
                {
                    CertificatId = ctx.Saga.CorrelationId,
                    CertificateNo = ctx.Saga.CertificateNo,
                    NumeroFacture = ctx.Saga.NumeroFacture
                })
                .TransitionTo(PdfEnCours),

            When(Facturation.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.LastError = "Facturation faulted";
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    logger.LogError("Facturation en erreur pour CO {CertificateNo}", ctx.Saga.CertificateNo);
                })
                .TransitionTo(Echec)
                .Finalize(),

            When(Facturation.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.LastError = "Facturation timeout";
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    logger.LogError("Timeout facturation pour CO {CertificateNo}", ctx.Saga.CertificateNo);
                })
                .TransitionTo(Echec)
                .Finalize()
        );

        During(PdfEnCours,
            When(Pdf.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.PdfUrl = ctx.Message.PdfUrl;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    logger.LogInformation(
                        "PDF OK pour CO {CertificateNo} → {PdfUrl}",
                        ctx.Saga.CertificateNo, ctx.Saga.PdfUrl);
                })
                .Publish(ctx => new EnvoyerNotificationCommand
                {
                    CertificatId = ctx.Saga.CorrelationId,
                    CertificateNo = ctx.Saga.CertificateNo,
                    NumeroFacture = ctx.Saga.NumeroFacture,
                    PdfUrl = ctx.Saga.PdfUrl
                })
                .Publish(ctx => new CertificatPretPourEchangeEvent
                {
                    CertificatId = ctx.Saga.CorrelationId,
                    CertificateNo = ctx.Saga.CertificateNo,
                    ExportateurNIU = ctx.Saga.ExportateurNIU,
                    PartenaireNIU = ctx.Saga.PartenaireNIU,
                    NumeroFacture = ctx.Saga.NumeroFacture,
                    PdfUrl = ctx.Saga.PdfUrl
                })
                .Then(ctx =>
                {
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    logger.LogInformation(
                        "Saga finalisée pour CO {CertificateNo} (notif + échange publiés)",
                        ctx.Saga.CertificateNo);
                })
                .TransitionTo(Finalisee)
                .Finalize(),

            When(Pdf.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.LastError = "PDF faulted";
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    logger.LogError("PDF en erreur pour CO {CertificateNo}", ctx.Saga.CertificateNo);
                })
                .TransitionTo(Echec)
                .Finalize(),

            When(Pdf.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.LastError = "PDF timeout";
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    logger.LogError("Timeout PDF pour CO {CertificateNo}", ctx.Saga.CertificateNo);
                })
                .TransitionTo(Echec)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }

    public State FacturationEnCours { get; private set; } = null!;
    public State PdfEnCours { get; private set; } = null!;
    public State Finalisee { get; private set; } = null!;
    public State Echec { get; private set; } = null!;

    public Event<CertificatValideEvent> CertificatValide { get; private set; } = null!;

    public Request<CertificatPostValidationState, GenererFactureCommand, FactureGenereeResponse> Facturation { get; private set; } = null!;
    public Request<CertificatPostValidationState, GenererPdfCommand, PdfGenereResponse> Pdf { get; private set; } = null!;
}
