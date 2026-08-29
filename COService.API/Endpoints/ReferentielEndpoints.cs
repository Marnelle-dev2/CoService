using COService.Application.DTOs;
using COService.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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

        group.MapGet("/aeroports", async (Guid? paysId, string? codePays, IReferentielServiceClient client, CancellationToken ct) =>
        {
            try
            {
                var items = await client.GetAeroportsAsync(ct);
                if (paysId.HasValue)
                {
                    items = items.Where(p => p.PaysId == paysId.Value).ToList();
                }
                else if (!string.IsNullOrWhiteSpace(codePays))
                {
                    var pays = await client.GetPaysAsync(ct);
                    var ids = pays
                        .Where(p => string.Equals(p.Code, codePays.Trim(), StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Id)
                        .ToHashSet();
                    items = items.Where(p => p.PaysId.HasValue && ids.Contains(p.PaysId.Value)).ToList();
                }
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

        group.MapGet("/etats", async (IReferentielServiceClient client, CancellationToken ct) =>
        {
            try
            {
                var items = await client.GetEtatsAsync(ct);
                return Results.Ok(items);
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
        .WithName("GetReferentielEtats")
        .WithSummary("États live depuis MS Référentiel (/api/etats)");

        group.MapGet("/bureaux-douanes", async (IReferentielServiceClient client, CancellationToken ct) =>
            await ProxyAsync(() => client.GetBureauxDouanesAsync(ct), MapItem))
            .WithName("GetReferentielBureauxDouanes")
            .WithSummary("Bureaux de douane depuis Référentiel (/api/bureauxdouanes)");

        group.MapGet("/positions-tarifaires", async (
            string? search,
            string? regime,
            int? take,
            IMemoryCache cache,
            IReferentielServiceClient client,
            CancellationToken ct) =>
        {
            try
            {
                const string cacheKey = "referentiel:positiontarifaires:all";
                if (!cache.TryGetValue(cacheKey, out List<ReferentielPositionTarifaireDto>? all) || all == null)
                {
                    all = await client.GetPositionTarifairesAsync(ct);
                    cache.Set(cacheKey, all, TimeSpan.FromMinutes(30));
                }

                IEnumerable<ReferentielPositionTarifaireDto> query = all.Where(p => p.Actif);

                if (!string.IsNullOrWhiteSpace(regime))
                {
                    var regimeFilter = regime.Trim().ToUpperInvariant();
                    // Le référentiel expose des régimes douaniers (ASI, etc.) — pas « CO ».
                    // On n'applique le filtre que si des positions portent explicitement ce régime.
                    var withRegime = query.Where(p =>
                        !string.IsNullOrWhiteSpace(p.Regime)
                        && string.Equals(p.Regime, regimeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (withRegime.Count > 0)
                    {
                        query = withRegime;
                    }
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim();
                    var termUpper = term.ToUpperInvariant();
                    var termDigits = new string(term.Where(char.IsDigit).ToArray());
                    query = query.Where(p =>
                        p.Code.Contains(termUpper, StringComparison.OrdinalIgnoreCase)
                        || p.Code.StartsWith(termUpper, StringComparison.OrdinalIgnoreCase)
                        || (!string.IsNullOrEmpty(termDigits)
                            && p.Code.Replace(".", "").StartsWith(termDigits, StringComparison.OrdinalIgnoreCase))
                        || (p.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                // Sans search : liste pour dropdown (plafond élevé — cache mémoire côté MS).
                var limit = Math.Clamp(take ?? (string.IsNullOrWhiteSpace(search) ? 2000 : 50), 1, 5000);
                var items = query
                    .OrderBy(p => p.Code)
                    .Take(limit)
                    .Select(p => new
                    {
                        p.Id,
                        p.Code,
                        Nom = p.Description ?? p.Code,
                        Description = p.Description,
                        p.Regime,
                        p.UniteStatistiqueId,
                        p.Actif
                    })
                    .ToList();

                return Results.Ok(items);
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
        .WithName("GetReferentielPositionsTarifaires")
        .WithSummary("Positions tarifaires depuis MS Référentiel (liste dropdown ou recherche + cache)");

        // Carnet d'adresses : propre à une organisation (filtre serveur + client)
        group.MapGet("/carnet-adresses", async (
            string? organisation,
            [FromHeader(Name = "X-Organisation-Id")] string? organisationHeader,
            IReferentielServiceClient client,
            CancellationToken ct) =>
        {
            try
            {
                var org = !string.IsNullOrWhiteSpace(organisation)
                    ? organisation.Trim()
                    : organisationHeader?.Trim();

                if (string.IsNullOrWhiteSpace(org))
                {
                    return Results.BadRequest(new
                    {
                        message = "Paramètre 'organisation' (ou header X-Organisation-Id) obligatoire — carnet scoped par organisation."
                    });
                }

                var items = await client.GetCarnetAdressesAsync(ct);
                var filtered = items
                    .Where(c => string.Equals(c.Organisation, org, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                return Results.Ok(filtered);
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
        .WithName("GetReferentielCarnetAdresses")
        .WithSummary("Carnet d'adresses Référentiel filtré par organisation");

        group.MapGet("/carnet-adresses/{id:guid}", async (
            Guid id,
            string? organisation,
            [FromHeader(Name = "X-Organisation-Id")] string? organisationHeader,
            IReferentielServiceClient client,
            CancellationToken ct) =>
        {
            try
            {
                var item = await client.GetCarnetAdresseByIdAsync(id, ct);
                var org = !string.IsNullOrWhiteSpace(organisation)
                    ? organisation.Trim()
                    : organisationHeader?.Trim();

                if (!string.IsNullOrWhiteSpace(org) &&
                    !string.Equals(item.Organisation, org, StringComparison.OrdinalIgnoreCase))
                {
                    return Results.NotFound(new { message = "Carnet introuvable pour cette organisation." });
                }

                return Results.Ok(item);
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.NotFound();
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
        .WithName("GetReferentielCarnetAdresseById");
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
