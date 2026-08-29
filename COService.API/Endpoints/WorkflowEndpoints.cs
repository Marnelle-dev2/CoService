using COService.API.Auth;
using COService.Application.DTOs;
using COService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace COService.API.Endpoints;

/// <summary>
/// Endpoints pour la gestion des workflows de validation des certificats
/// </summary>
public static class WorkflowEndpoints
{
    public static void MapWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workflow")
            .WithTags("Workflow de validation");

        group.MapPost("/{id:guid}/soumettre", async (
            HttpContext httpContext,
            Guid id,
            [FromBody] SoumettreCertificatRequest request,
            ICertificatOrigineService certificatService,
            IWorkflowService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureWorkflowAccessAsync(
                httpContext, id, certificatService, requireModify: true, cancellationToken: cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            var userId = ResolveUserId(httpContext, request.UserId);

            try
            {
                var certificat = await service.SoumettreCertificatAsync(id, userId, cancellationToken);
                return Results.Ok(certificat);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("SoumettreCertificat")
        .WithSummary("Soumet un certificat pour validation (Élaboré → Soumis)")
        .Produces<CertificatOrigineDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/controle", async (
            HttpContext httpContext,
            Guid id,
            [FromBody] ControleCertificatRequest request,
            ICertificatOrigineService certificatService,
            IWorkflowService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureWorkflowAccessAsync(
                httpContext, id, certificatService, requireValidate: true, cancellationToken: cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            var userId = ResolveUserId(httpContext, request.UserId);

            try
            {
                var certificat = await service.ControleCertificatAsync(id, userId, request.Password, cancellationToken);
                return Results.Ok(certificat);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("ControleCertificat")
        .WithSummary("Contrôle un certificat (Soumis → Contrôlé)")
        .Produces<CertificatOrigineDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/approuver", async (
            HttpContext httpContext,
            Guid id,
            [FromBody] ApprouverCertificatRequest request,
            ICertificatOrigineService certificatService,
            IWorkflowService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureWorkflowAccessAsync(
                httpContext, id, certificatService, requireValidate: true, cancellationToken: cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            var userId = ResolveUserId(httpContext, request.UserId);

            try
            {
                var certificat = await service.ApprouverCertificatAsync(id, userId, request.Password, cancellationToken);
                return Results.Ok(certificat);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("ApprouverCertificat")
        .WithSummary("Approuve un certificat (Contrôlé → Approuvé)")
        .Produces<CertificatOrigineDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/valider", async (
            HttpContext httpContext,
            Guid id,
            [FromBody] ValiderCertificatRequest request,
            ICertificatOrigineService certificatService,
            IWorkflowService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureWorkflowAccessAsync(
                httpContext, id, certificatService, requireValidate: true, cancellationToken: cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            var userId = ResolveUserId(httpContext, request.UserId);

            try
            {
                var certificat = await service.ValiderCertificatAsync(id, userId, request.Password, cancellationToken);
                return Results.Ok(certificat);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("ValiderCertificat")
        .WithSummary("Valide définitivement un certificat (Approuvé → Validé)")
        .Produces<CertificatOrigineDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/rejeter", async (
            HttpContext httpContext,
            Guid id,
            [FromBody] RejeterCertificatRequest request,
            ICertificatOrigineService certificatService,
            IWorkflowService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureWorkflowAccessAsync(
                httpContext, id, certificatService, requireValidate: true, cancellationToken: cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            var userId = ResolveUserId(httpContext, request.UserId);

            try
            {
                var certificat = await service.RejeterCertificatAsync(id, userId, request.Password, request.Commentaire, cancellationToken);
                return Results.Ok(certificat);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("RejeterCertificat")
        .WithSummary("Rejette un certificat (vers Rejeté)")
        .Produces<CertificatOrigineDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/demander-modification", async (
            HttpContext httpContext,
            Guid id,
            [FromBody] DemanderModificationRequest request,
            ICertificatOrigineService certificatService,
            IWorkflowService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureWorkflowAccessAsync(
                httpContext, id, certificatService, requireValidate: true, cancellationToken: cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            var userId = ResolveUserId(httpContext, request.UserId);

            try
            {
                var certificat = await service.DemanderModificationAsync(id, userId, request.Commentaire, cancellationToken);
                return Results.Ok(certificat);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("DemanderModification")
        .WithSummary("Demande une modification sur un certificat validé (Validé → Modification)")
        .Produces<CertificatOrigineDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}/transitions-possibles", async (
            HttpContext httpContext,
            Guid id,
            [FromQuery] string userId,
            ICertificatOrigineService certificatService,
            IWorkflowService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureWorkflowAccessAsync(
                httpContext, id, certificatService, cancellationToken: cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            try
            {
                var resolvedUserId = ResolveUserId(httpContext, userId);
                var transitions = await service.GetTransitionsPossiblesAsync(id, resolvedUserId, cancellationToken);
                return Results.Ok(new { transitions });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("GetTransitionsPossibles")
        .WithSummary("Récupère les transitions possibles pour un certificat selon l'utilisateur")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/transition-valide", async (
            HttpContext httpContext,
            Guid id,
            [FromQuery] string codeNouveauStatut,
            [FromQuery] string userId,
            ICertificatOrigineService certificatService,
            IWorkflowService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureWorkflowAccessAsync(
                httpContext, id, certificatService, cancellationToken: cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            try
            {
                var resolvedUserId = ResolveUserId(httpContext, userId);
                var estValide = await service.EstTransitionValideAsync(id, codeNouveauStatut, resolvedUserId, cancellationToken);
                return Results.Ok(new { estValide });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("EstTransitionValide")
        .WithSummary("Vérifie si une transition de statut est valide pour un certificat")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }

    private static string ResolveUserId(HttpContext httpContext, string? requestUserId)
    {
        var user = PocAuthResults.GetUser(httpContext);
        if (!string.IsNullOrWhiteSpace(requestUserId))
        {
            return requestUserId.Trim();
        }

        return user.UserId ?? "poc.user";
    }

    private static async Task<(CertificatOrigineDto? Certificat, IResult? Error)> EnsureWorkflowAccessAsync(
        HttpContext httpContext,
        Guid certificatId,
        ICertificatOrigineService certificatService,
        bool requireModify = false,
        bool requireValidate = false,
        CancellationToken cancellationToken = default)
    {
        var user = PocAuthResults.GetUser(httpContext);

        if (requireModify && user.IsEnabled && !user.CanModifyCertificat)
        {
            return (null, PocAuthResults.Forbidden("Action réservée à l'exportateur propriétaire."));
        }

        if (requireValidate && user.IsEnabled && !user.CanValidateCertificat)
        {
            return (null, PocAuthResults.Forbidden("Action de validation réservée à la chambre de commerce."));
        }

        if (!user.CanReadCertificats)
        {
            return (null, PocAuthResults.Forbidden("Accès lecture certificats refusé pour ce profil."));
        }

        var certificat = await certificatService.GetCertificatByIdAsync(certificatId, cancellationToken);
        if (certificat == null)
        {
            return (null, Results.NotFound(new { message = $"Certificat avec l'ID {certificatId} introuvable." }));
        }

        if (user.IsEnabled && !PocCertificatScope.CanAccessCertificat(certificat, user))
        {
            return (null, PocAuthResults.Forbidden("Accès à ce certificat refusé pour votre profil ou organisation."));
        }

        return (certificat, null);
    }

    public record SoumettreCertificatRequest(string UserId);
    public record ControleCertificatRequest(string UserId, string Password);
    public record ApprouverCertificatRequest(string UserId, string Password);
    public record ValiderCertificatRequest(string UserId, string Password);
    public record RejeterCertificatRequest(string UserId, string Password, string Commentaire);
    public record DemanderModificationRequest(string UserId, string Commentaire);
}
