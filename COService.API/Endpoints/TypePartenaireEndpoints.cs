using COService.Application.DTOs;
using COService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace COService.API.Endpoints;

/// <summary>
/// Endpoints pour la gestion des types de partenaires
/// </summary>
public static class TypePartenaireEndpoints
{
    public static void MapTypePartenaireEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/types-partenaires")
            .WithTags("Types de Partenaires");

        // GET /api/types-partenaires - Liste tous les types de partenaires
        group.MapGet("/", async (
            ITypePartenaireService service,
            CancellationToken cancellationToken) =>
        {
            var types = await service.GetAllAsync(cancellationToken);
            return Results.Ok(types);
        })
        .WithName("GetAllTypesPartenaires")
        .WithSummary("Récupère tous les types de partenaires")
        .Produces<IEnumerable<TypePartenaireDto>>(StatusCodes.Status200OK);

        // GET /api/types-partenaires/{id} - Récupère un type par ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            ITypePartenaireService service,
            CancellationToken cancellationToken) =>
        {
            var type = await service.GetByIdAsync(id, cancellationToken);
            return type == null
                ? Results.NotFound(new { message = $"Type de partenaire avec l'ID {id} introuvable." })
                : Results.Ok(type);
        })
        .WithName("GetTypePartenaireById")
        .WithSummary("Récupère un type de partenaire par son identifiant")
        .Produces<TypePartenaireDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/types-partenaires/code/{code} - Récupère un type par code
        group.MapGet("/code/{code}", async (
            string code,
            ITypePartenaireService service,
            CancellationToken cancellationToken) =>
        {
            var type = await service.GetByCodeAsync(code, cancellationToken);
            return type == null
                ? Results.NotFound(new { message = $"Type de partenaire avec le code {code} introuvable." })
                : Results.Ok(type);
        })
        .WithName("GetTypePartenaireByCode")
        .WithSummary("Récupère un type de partenaire par son code")
        .Produces<TypePartenaireDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/types-partenaires/actifs - Liste les types actifs
        group.MapGet("/actifs", async (
            ITypePartenaireService service,
            CancellationToken cancellationToken) =>
        {
            var types = await service.GetActifsAsync(cancellationToken);
            return Results.Ok(types);
        })
        .WithName("GetTypesPartenairesActifs")
        .WithSummary("Récupère tous les types de partenaires actifs")
        .Produces<IEnumerable<TypePartenaireDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (
            [FromBody] CreerTypePartenaireDto dto,
            ITypePartenaireService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var type = await service.CreerAsync(dto, utilisateur, cancellationToken);
                return Results.Created($"/api/types-partenaires/{type.Id}", type);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("CreerTypePartenaire")
        .WithSummary("Crée un type de partenaire")
        .Produces<TypePartenaireDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
