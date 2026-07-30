using COService.Application.DTOs;
using COService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace COService.API.Endpoints;

/// <summary>
/// Endpoints pour la gestion des états (statuts) de certificats
/// </summary>
public static class EtatEndpoints
{
    public static void MapEtatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/etats")
            .WithTags("États");

        // GET /api/etats - Liste tous les états
        group.MapGet("/", async (
            IEtatService service,
            CancellationToken cancellationToken) =>
        {
            var etats = await service.GetAllEtatsAsync(cancellationToken);
            return Results.Ok(etats);
        })
        .WithName("GetAllEtats")
        .WithSummary("Récupère tous les états de certificats")
        .Produces<IEnumerable<EtatDto>>(StatusCodes.Status200OK);

        // GET /api/etats/{id} - Récupère un état par ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            IEtatService service,
            CancellationToken cancellationToken) =>
        {
            var etat = await service.GetEtatByIdAsync(id, cancellationToken);
            return etat == null
                ? Results.NotFound(new { message = $"État avec l'ID {id} introuvable." })
                : Results.Ok(etat);
        })
        .WithName("GetEtatById")
        .WithSummary("Récupère un état par son identifiant")
        .Produces<EtatDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/etats/code/{code} - Récupère un état par code
        group.MapGet("/code/{code}", async (
            string code,
            IEtatService service,
            CancellationToken cancellationToken) =>
        {
            var etat = await service.GetEtatByCodeAsync(code, cancellationToken);
            return etat == null
                ? Results.NotFound(new { message = $"État avec le code {code} introuvable." })
                : Results.Ok(etat);
        })
        .WithName("GetEtatByCode")
        .WithSummary("Récupère un état par son code")
        .Produces<EtatDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            [FromBody] CreerEtatDto dto,
            IEtatService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var etat = await service.CreerEtatAsync(dto, utilisateur, cancellationToken);
                return Results.Created($"/api/etats/{etat.Id}", etat);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("CreerEtat")
        .WithSummary("Crée un état de certificat")
        .Produces<EtatDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/seed-workflow", async (
            IEtatService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            var etats = await service.SeedEtatsWorkflowAsync(utilisateur, cancellationToken);
            return Results.Ok(etats);
        })
        .WithName("SeedEtatsWorkflow")
        .WithSummary("Insère les états workflow manquants (ELABORE, SOUMIS, …)")
        .Produces<IEnumerable<EtatDto>>(StatusCodes.Status200OK);
    }
}
