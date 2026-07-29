using COService.Application.DTOs;
using COService.Infrastructure.ExternalServices;
using Refit;

namespace COService.API.Endpoints;

/// <summary>
/// Proxy lecture live vers MS Référentiel (accès direct :8290/api/...).
/// </summary>
public static class ReferentielEndpoints
{
    public static void MapReferentielEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/referentiel")
            .WithTags("Référentiels (Gateway)");

        group.MapGet("/pays", async (IReferentielServiceClient client, CancellationToken ct) =>
            await ProxyAsync(() => client.GetPaysAsync(ct), MapItem))
            .WithName("GetReferentielPays")
            .WithSummary("Liste des pays depuis le MS Référentiel");

        group.MapGet("/ports", async (Guid? paysId, string? codePays, IReferentielServiceClient client, CancellationToken ct) =>
        {
            try
            {
                var ports = await client.GetPortsAsync(ct);
                if (paysId.HasValue)
                    ports = ports.Where(p => p.PaysId == paysId.Value).ToList();
                if (!string.IsNullOrWhiteSpace(codePays))
                {
                    var pays = await client.GetPaysAsync(ct);
                    var ids = pays
                        .Where(p => string.Equals(p.Code, codePays, StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Id)
                        .ToHashSet();
                    ports = ports.Where(p => p.PaysId.HasValue && ids.Contains(p.PaysId.Value)).ToList();
                }
                return Results.Ok(ports.Select(MapPort));
            }
            catch (ApiException ex)
            {
                return GatewayProblem(ex);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("GetReferentielPorts")
        .WithSummary("Ports depuis Référentiel (filtre optionnel paysId ou codePays=CG)");

        group.MapGet("/aeroports", async (Guid? paysId, IReferentielServiceClient client, CancellationToken ct) =>
        {
            try
            {
                var items = await client.GetAeroportsAsync(ct);
                if (paysId.HasValue)
                    items = items.Where(p => p.PaysId == paysId.Value).ToList();
                return Results.Ok(items.Select(MapPort));
            }
            catch (ApiException ex)
            {
                return GatewayProblem(ex);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("GetReferentielAeroports");

        group.MapGet("/devises", async (IReferentielServiceClient client, CancellationToken ct) =>
            await ProxyAsync(() => client.GetDevisesAsync(ct), MapItem))
            .WithName("GetReferentielDevises");

        group.MapGet("/incoterms", async (IReferentielServiceClient client, CancellationToken ct) =>
        {
            try
            {
                var items = await client.GetIncotermsAsync(ct);
                return Results.Ok(items.Select(i => new
                {
                    i.Id,
                    i.Code,
                    Nom = i.Description,
                    i.Description,
                    i.Actif
                }));
            }
            catch (ApiException ex)
            {
                return GatewayProblem(ex);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("GetReferentielIncoterms");

        group.MapGet("/departements", async (IReferentielServiceClient client, CancellationToken ct) =>
            await ProxyAsync(() => client.GetDepartementsAsync(ct), MapItem))
            .WithName("GetReferentielDepartements");

        group.MapGet("/modes-transport", async (IReferentielServiceClient client, CancellationToken ct) =>
            await ProxyAsync(() => client.GetModeDeTransportsAsync(ct), MapItem))
            .WithName("GetReferentielModesTransport");

        group.MapGet("/corridors", async (IReferentielServiceClient client, CancellationToken ct) =>
            await ProxyAsync(() => client.GetCorridorsAsync(ct), MapItem))
            .WithName("GetReferentielCorridors");

        group.MapGet("/unites-statistiques", async (IReferentielServiceClient client, CancellationToken ct) =>
            await ProxyAsync(() => client.GetUniteStatistiquesAsync(ct), MapItem))
            .WithName("GetReferentielUnitesStatistiques");
    }

    private static async Task<IResult> ProxyAsync<T>(
        Func<Task<List<T>>> loader,
        Func<T, object> mapper)
    {
        try
        {
            var items = await loader();
            return Results.Ok(items.Select(mapper));
        }
        catch (ApiException ex)
        {
            return GatewayProblem(ex);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static IResult GatewayProblem(ApiException ex) =>
        Results.Problem(
            detail: $"Gateway/Référentiel: {ex.StatusCode} — {ex.Content}",
            statusCode: (int)ex.StatusCode);

    private static object MapItem(ReferentielItemDto i) => new PaysDto
    {
        Id = i.Id,
        Code = i.Code,
        Nom = i.Nom ?? i.Code,
        Actif = i.Actif,
        CreePar = i.CreerPar,
        ModifiePar = i.ModifierPar,
        CreeLe = i.CreerLe,
        ModifierLe = i.ModifierLe
    };

    private static object MapPort(ReferentielPortDto p) => new PortDto
    {
        Id = p.Id,
        Code = p.Code,
        Nom = p.Nom ?? p.Code,
        PaysId = p.PaysId,
        Type = p.Type,
        Actif = p.Actif,
        CreePar = p.CreerPar,
        ModifiePar = p.ModifierPar,
        CreeLe = p.CreerLe,
        ModifierLe = p.ModifierLe
    };
}
