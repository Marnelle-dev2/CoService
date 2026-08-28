using COService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace COService.API.Endpoints;

/// <summary>
/// Zones de production — données internes au MS CO (table ZonesProductions).
/// </summary>
public static class ZoneProductionEndpoints
{
    public static void MapZoneProductionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/zones-production")
            .WithTags("Zones de production");

        group.MapGet("/", async (
            string? partenaireNIU,
            IZoneProductionService service,
            CancellationToken cancellationToken) =>
        {
            var zones = string.IsNullOrWhiteSpace(partenaireNIU)
                ? await service.GetAllAsync(cancellationToken)
                : await service.GetByPartenaireNIUAsync(partenaireNIU.Trim(), cancellationToken);

            return Results.Ok(zones);
        })
        .WithName("GetAllZonesProduction")
        .WithSummary("Liste les zones de production (filtre optionnel partenaireNIU)");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IZoneProductionService service,
            CancellationToken cancellationToken) =>
        {
            var zone = await service.GetByIdAsync(id, cancellationToken);
            return zone == null
                ? Results.NotFound(new { message = $"Zone de production {id} introuvable." })
                : Results.Ok(zone);
        })
        .WithName("GetZoneProductionById");

        group.MapGet("/code/{code}", async (
            string code,
            IZoneProductionService service,
            CancellationToken cancellationToken) =>
        {
            var zone = await service.GetByCodeAsync(code, cancellationToken);
            return zone == null
                ? Results.NotFound(new { message = $"Zone de production {code} introuvable." })
                : Results.Ok(zone);
        })
        .WithName("GetZoneProductionByCode");
    }
}
