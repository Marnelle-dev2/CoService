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

        // GET /api/certificats/{certificatId}/lignes - Liste toutes les lignes d'un certificat
        group.MapGet("/", async (
            Guid certificatId,
            ICertificatLigneService service,
            CancellationToken cancellationToken) =>
        {
            var lignes = await service.GetLignesByCertificatIdAsync(certificatId, cancellationToken);
            return Results.Ok(lignes);
        })
        .WithName("GetLignesByCertificatId")
        .WithSummary("Récupère toutes les lignes d'un certificat")
        .Produces<IEnumerable<CertificatLigneDto>>(StatusCodes.Status200OK);

        // GET /api/certificats/{certificatId}/lignes/{id} - Récupère une ligne par ID
        group.MapGet("/{id:guid}", async (
            Guid certificatId,
            Guid id,
            ICertificatLigneService service,
            CancellationToken cancellationToken) =>
        {
            var ligne = await service.GetLigneByIdAsync(id, cancellationToken);
            return ligne == null
                ? Results.NotFound(new { message = $"Ligne avec l'ID {id} introuvable." })
                : Results.Ok(ligne);
        })
        .WithName("GetLigneById")
        .WithSummary("Récupère une ligne par son identifiant")
        .Produces<CertificatLigneDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/certificats/{certificatId}/lignes - Crée une nouvelle ligne
        group.MapPost("/", async (
            Guid certificatId,
            [FromBody] CreerCertificatLigneDto dto,
            ICertificatLigneService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var ligne = await service.CreerLigneAsync(certificatId, dto, utilisateur, cancellationToken);
                return Results.Created($"/api/certificats/{certificatId}/lignes/{ligne.Id}", ligne);
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
        .Produces(StatusCodes.Status404NotFound);

        // PUT /api/certificats/{certificatId}/lignes/{id} - Modifie une ligne
        group.MapPut("/{id:guid}", async (
            Guid certificatId,
            Guid id,
            [FromBody] ModifierCertificatLigneDto dto,
            ICertificatLigneService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var ligne = await service.ModifierLigneAsync(id, dto, utilisateur, cancellationToken);
                return Results.Ok(ligne);
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
        .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/certificats/{certificatId}/lignes/{id} - Supprime une ligne
        group.MapDelete("/{id:guid}", async (
            Guid certificatId,
            Guid id,
            ICertificatLigneService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await service.SupprimerLigneAsync(id, cancellationToken);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("SupprimerLigne")
        .WithSummary("Supprime une ligne de certificat")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
