using COService.Application.DTOs;
using COService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace COService.API.Endpoints;

/// <summary>
/// Endpoints pour la gestion des statuts de certificats
/// </summary>
public static class StatutCertificatEndpoints
{
    public static void MapStatutCertificatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/statuts-certificats")
            .WithTags("Statuts de certificats");

        // GET /api/statuts-certificats - Liste tous les statuts
        group.MapGet("/", async (
            IStatutCertificatService service,
            CancellationToken cancellationToken) =>
        {
            var statuts = await service.GetAllStatutsAsync(cancellationToken);
            return Results.Ok(statuts);
        })
        .WithName("GetAllStatutsCertificats")
        .WithSummary("Récupère tous les statuts de certificats")
        .Produces<IEnumerable<StatutCertificatDto>>(StatusCodes.Status200OK);

        // GET /api/statuts-certificats/{id} - Récupère un statut par ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            IStatutCertificatService service,
            CancellationToken cancellationToken) =>
        {
            var statut = await service.GetStatutByIdAsync(id, cancellationToken);
            return statut == null
                ? Results.NotFound(new { message = $"Statut avec l'ID {id} introuvable." })
                : Results.Ok(statut);
        })
        .WithName("GetStatutCertificatById")
        .WithSummary("Récupère un statut par son identifiant")
        .Produces<StatutCertificatDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/statuts-certificats/code/{code} - Récupère un statut par code
        group.MapGet("/code/{code}", async (
            string code,
            IStatutCertificatService service,
            CancellationToken cancellationToken) =>
        {
            var statut = await service.GetStatutByCodeAsync(code, cancellationToken);
            return statut == null
                ? Results.NotFound(new { message = $"Statut avec le code {code} introuvable." })
                : Results.Ok(statut);
        })
        .WithName("GetStatutCertificatByCode")
        .WithSummary("Récupère un statut par son code")
        .Produces<StatutCertificatDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            [FromBody] CreerStatutCertificatDto dto,
            IStatutCertificatService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var statut = await service.CreerStatutAsync(dto, utilisateur, cancellationToken);
                return Results.Created($"/api/statuts-certificats/{statut.Id}", statut);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("CreerStatutCertificat")
        .WithSummary("Crée un statut de certificat")
        .Produces<StatutCertificatDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/seed-workflow", async (
            IStatutCertificatService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            var statuts = await service.SeedStatutsWorkflowAsync(utilisateur, cancellationToken);
            return Results.Ok(statuts);
        })
        .WithName("SeedStatutsWorkflow")
        .WithSummary("Insère les statuts workflow manquants (ELABORE, SOUMIS, …)")
        .Produces<IEnumerable<StatutCertificatDto>>(StatusCodes.Status200OK);
    }
}
