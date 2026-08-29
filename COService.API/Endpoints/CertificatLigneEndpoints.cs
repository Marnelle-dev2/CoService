using COService.API.Auth;
using COService.Application.DTOs;
using COService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace COService.API.Endpoints;

/// <summary>
/// Endpoints pour la gestion des lignes de certificat
/// </summary>
public static class CertificatLigneEndpoints
{
    public static void MapCertificatLigneEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/certificats/{certificatId:guid}/lignes")
            .WithTags("Lignes de certificat");

        group.MapGet("/", async (
            HttpContext httpContext,
            Guid certificatId,
            ICertificatOrigineService certificatService,
            ICertificatLigneService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureCertificatAccessAsync(httpContext, certificatId, certificatService, cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            var lignes = await service.GetLignesByCertificatIdAsync(certificatId, cancellationToken);
            return Results.Ok(lignes);
        })
        .WithName("GetLignesByCertificatId")
        .WithSummary("Récupère toutes les lignes d'un certificat")
        .Produces<IEnumerable<CertificatLigneDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (
            HttpContext httpContext,
            Guid certificatId,
            Guid id,
            ICertificatOrigineService certificatService,
            ICertificatLigneService service,
            CancellationToken cancellationToken) =>
        {
            var access = await EnsureCertificatAccessAsync(httpContext, certificatId, certificatService, cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            var ligne = await service.GetLigneByIdAsync(id, cancellationToken);
            return ligne == null
                ? Results.NotFound(new { message = $"Ligne avec l'ID {id} introuvable." })
                : Results.Ok(ligne);
        })
        .WithName("GetLigneById")
        .WithSummary("Récupère une ligne par son identifiant")
        .Produces<CertificatLigneDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            HttpContext httpContext,
            Guid certificatId,
            [FromBody] CreerCertificatLigneDto dto,
            ICertificatOrigineService certificatService,
            ICertificatLigneService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            var user = PocAuthResults.GetUser(httpContext);
            if (user.IsEnabled && !user.CanModifyCertificat)
            {
                return PocAuthResults.Forbidden("Ajout de ligne réservé à l'exportateur.");
            }

            var access = await EnsureCertificatAccessAsync(httpContext, certificatId, certificatService, cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            utilisateur ??= user.UserId;

            try
            {
                var ligne = await service.CreerLigneAsync(certificatId, dto, utilisateur, cancellationToken);
                return Results.Created($"/api/certificats/{certificatId}/lignes/{ligne.Id}", ligne);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("CreerLigne")
        .WithSummary("Crée une nouvelle ligne de certificat")
        .Accepts<CreerCertificatLigneDto>("application/json")
        .Produces<CertificatLigneDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", async (
            HttpContext httpContext,
            Guid certificatId,
            Guid id,
            [FromBody] ModifierCertificatLigneDto dto,
            ICertificatOrigineService certificatService,
            ICertificatLigneService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            var user = PocAuthResults.GetUser(httpContext);
            if (user.IsEnabled && !user.CanModifyCertificat)
            {
                return PocAuthResults.Forbidden("Modification de ligne réservée à l'exportateur.");
            }

            var access = await EnsureCertificatAccessAsync(httpContext, certificatId, certificatService, cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            utilisateur ??= user.UserId;

            try
            {
                var ligne = await service.ModifierLigneAsync(id, dto, utilisateur, cancellationToken);
                return Results.Ok(ligne);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("ModifierLigne")
        .WithSummary("Modifie une ligne de certificat")
        .Accepts<ModifierCertificatLigneDto>("application/json")
        .Produces<CertificatLigneDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", async (
            HttpContext httpContext,
            Guid certificatId,
            Guid id,
            ICertificatOrigineService certificatService,
            ICertificatLigneService service,
            CancellationToken cancellationToken) =>
        {
            var user = PocAuthResults.GetUser(httpContext);
            if (user.IsEnabled && !user.CanModifyCertificat)
            {
                return PocAuthResults.Forbidden("Suppression de ligne réservée à l'exportateur.");
            }

            var access = await EnsureCertificatAccessAsync(httpContext, certificatId, certificatService, cancellationToken);
            if (access.Error != null)
            {
                return access.Error;
            }

            try
            {
                await service.SupprimerLigneAsync(id, cancellationToken);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("SupprimerLigne")
        .WithSummary("Supprime une ligne de certificat")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<(CertificatOrigineDto? Certificat, IResult? Error)> EnsureCertificatAccessAsync(
        HttpContext httpContext,
        Guid certificatId,
        ICertificatOrigineService certificatService,
        CancellationToken cancellationToken)
    {
        var user = PocAuthResults.GetUser(httpContext);
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
}
