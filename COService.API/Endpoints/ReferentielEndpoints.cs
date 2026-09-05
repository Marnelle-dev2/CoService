using COService.Application.DTOs;
using COService.Application.Repositories;
using COService.Domain.Entities;
using COService.Infrastructure.Data;
using COService.Infrastructure.ExternalServices;
using COService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Refit;

namespace COService.API.Endpoints;

/// <summary>
/// Référentiels : live MS Référentiel si dispo, sinon copie locale CO (tables sync).
/// </summary>
public static class ReferentielEndpoints
{
    public static void MapReferentielEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/referentiel")
            .WithTags("Référentiels (local + MS)");

        group.MapPost("/sync", async (
            bool? includePositions,
            IReferentielSyncService sync,
            CancellationToken ct) =>
        {
            try
            {
                var result = await sync.SyncAllAsync(includePositions ?? true, ct);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Sync référentiel impossible: {ex.Message}",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("SyncReferentielLocal")
        .WithSummary("Tire le MS Référentiel vers la copie locale CO");

        group.MapGet("/pays", async (
            IReferentielServiceClient client,
            IPaysRepository paysRepo,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
            await ServeListAsync(
                () => client.GetPaysAsync(ct),
                MapItem,
                async () => (await paysRepo.GetActifsAsync(ct))
                    .Select(p => MapLocalItem(p.Id, p.Code, p.Nom, p.Actif, p.CreePar, p.ModifiePar, p.CreeLe, p.ModifierLe)),
                scopes,
                ct))
            .WithName("GetReferentielPays")
            .WithSummary("Pays — MS Référentiel ou copie locale CO");

        group.MapGet("/ports", async (
            Guid? paysId,
            string? codePays,
            IReferentielServiceClient client,
            IPortRepository portRepo,
            IPaysRepository paysRepo,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
        {
            try
            {
                var ports = await client.GetPortsAsync(ct);
                ports = await FilterPortsRemoteAsync(ports, paysId, codePays, client, ct);
                KickSoftSync(scopes);
                return Results.Ok(ports.Select(MapPort));
            }
            catch (Exception ex) when (IsRemoteFailure(ex))
            {
                var local = (await portRepo.GetActifsAsync(ct)).AsEnumerable();
                if (paysId.HasValue)
                    local = local.Where(p => p.PaysId == paysId.Value);
                if (!string.IsNullOrWhiteSpace(codePays))
                {
                    var pays = await paysRepo.GetByCodeAsync(codePays.Trim(), ct);
                    if (pays != null)
                        local = local.Where(p => p.PaysId == pays.Id);
                    else
                        local = Enumerable.Empty<Port>();
                }

                var list = local.Select(MapLocalPort).ToList();
                if (list.Count == 0)
                    return RemoteDownProblem(ex);
                return Results.Ok(list);
            }
        })
        .WithName("GetReferentielPorts")
        .WithSummary("Ports — MS ou copie locale");

        group.MapGet("/aeroports", async (
            Guid? paysId,
            string? codePays,
            IReferentielServiceClient client,
            IAeroportRepository aeroportRepo,
            IPaysRepository paysRepo,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
        {
            try
            {
                var items = await client.GetAeroportsAsync(ct);
                if (paysId.HasValue)
                    items = items.Where(p => p.PaysId == paysId.Value).ToList();
                else if (!string.IsNullOrWhiteSpace(codePays))
                {
                    var pays = await client.GetPaysAsync(ct);
                    var ids = pays
                        .Where(p => string.Equals(p.Code, codePays.Trim(), StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Id)
                        .ToHashSet();
                    items = items.Where(p => p.PaysId.HasValue && ids.Contains(p.PaysId.Value)).ToList();
                }

                KickSoftSync(scopes);
                return Results.Ok(items.Select(MapPort));
            }
            catch (Exception ex) when (IsRemoteFailure(ex))
            {
                var local = (await aeroportRepo.GetActifsAsync(ct)).AsEnumerable();
                if (paysId.HasValue)
                    local = local.Where(p => p.PaysId == paysId.Value);
                else if (!string.IsNullOrWhiteSpace(codePays))
                {
                    var pays = await paysRepo.GetByCodeAsync(codePays.Trim(), ct);
                    if (pays != null)
                        local = local.Where(p => p.PaysId == pays.Id);
                    else
                        local = Enumerable.Empty<Aeroport>();
                }

                var list = local.Select(a => MapLocalPortLike(a.Id, a.Code, a.Nom, a.PaysId, null, a.Actif, a.CreePar, a.ModifiePar, a.CreeLe, a.ModifierLe)).ToList();
                if (list.Count == 0)
                    return RemoteDownProblem(ex);
                return Results.Ok(list);
            }
        })
        .WithName("GetReferentielAeroports");

        group.MapGet("/devises", async (
            IReferentielServiceClient client,
            IDeviseRepository repo,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
            await ServeListAsync(
                () => client.GetDevisesAsync(ct),
                MapItem,
                async () => (await repo.GetActifsAsync(ct))
                    .Select(d => MapLocalItem(d.Id, d.Code, d.Nom, d.Actif, d.CreePar, d.ModifiePar, d.CreeLe, d.ModifierLe)),
                scopes,
                ct))
            .WithName("GetReferentielDevises");

        group.MapGet("/incoterms", async (
            IReferentielServiceClient client,
            IIncotermRepository repo,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
        {
            try
            {
                var items = await client.GetIncotermsAsync(ct);
                KickSoftSync(scopes);
                return Results.Ok(items.Select(i => new
                {
                    i.Id,
                    i.Code,
                    Nom = i.Description,
                    i.Description,
                    i.Actif
                }));
            }
            catch (Exception ex) when (IsRemoteFailure(ex))
            {
                var local = (await repo.GetActifsAsync(ct))
                    .Select(i => new
                    {
                        i.Id,
                        i.Code,
                        Nom = i.Description,
                        i.Description,
                        i.Actif
                    })
                    .ToList();
                if (local.Count == 0)
                    return RemoteDownProblem(ex);
                return Results.Ok(local);
            }
        })
        .WithName("GetReferentielIncoterms");

        group.MapGet("/departements", async (
            IReferentielServiceClient client,
            IDepartementRepository repo,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
            await ServeListAsync(
                () => client.GetDepartementsAsync(ct),
                MapItem,
                async () => (await repo.GetActifsAsync(ct))
                    .Select(d => MapLocalItem(d.Id, d.Code, d.Nom, d.Actif, d.CreePar, d.ModifiePar, d.CreeLe, d.ModifierLe)),
                scopes,
                ct))
            .WithName("GetReferentielDepartements");

        group.MapGet("/modes-transport", async (
            IReferentielServiceClient client,
            IModuleRepository repo,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
            await ServeListAsync(
                () => client.GetModeDeTransportsAsync(ct),
                MapItem,
                async () => (await repo.GetActifsAsync(ct))
                    .Select(m => MapLocalItem(m.Id, m.Code, m.Nom, m.Actif, m.CreePar, m.ModifiePar, m.CreeLe, m.ModifierLe)),
                scopes,
                ct))
            .WithName("GetReferentielModesTransport");

        group.MapGet("/corridors", async (
            IReferentielServiceClient client,
            COServiceDbContext db,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
        {
            try
            {
                var items = await client.GetCorridorsAsync(ct);
                KickSoftSync(scopes);
                return Results.Ok(items.Select(MapItem));
            }
            catch (Exception ex) when (IsRemoteFailure(ex))
            {
                var local = await db.Corridors.AsNoTracking()
                    .Where(c => c.Actif)
                    .OrderBy(c => c.Nom)
                    .ToListAsync(ct);
                if (local.Count == 0)
                    return RemoteDownProblem(ex);
                return Results.Ok(local.Select(c =>
                    MapLocalItem(c.Id, c.Code, c.Nom, c.Actif, c.CreePar, c.ModifiePar, c.CreeLe, c.ModifierLe)));
            }
        })
        .WithName("GetReferentielCorridors");

        group.MapGet("/unites-statistiques", async (
            IReferentielServiceClient client,
            IUniteStatistiqueRepository repo,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
            await ServeListAsync(
                () => client.GetUniteStatistiquesAsync(ct),
                MapItem,
                async () => (await repo.GetActifsAsync(ct))
                    .Select(u => MapLocalItem(u.Id, u.Code, u.Nom, u.Actif, u.CreePar, u.ModifiePar, u.CreeLe, u.ModifierLe)),
                scopes,
                ct))
            .WithName("GetReferentielUnitesStatistiques");

        group.MapGet("/etats", async (IReferentielServiceClient client, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await client.GetEtatsAsync(ct));
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
        .WithSummary("États live depuis MS Référentiel (/api/etats) — workflow CO utilise /api/etats local");

        group.MapGet("/bureaux-douanes", async (
            IReferentielServiceClient client,
            IBureauDedouanementRepository repo,
            IServiceScopeFactory scopes,
            CancellationToken ct) =>
            await ServeListAsync(
                () => client.GetBureauxDouanesAsync(ct),
                MapItem,
                async () => (await repo.GetActifsAsync(ct))
                    .Select(b => MapLocalItem(b.Id, b.Code, b.Description, b.Actif, b.CreePar, b.ModifiePar, b.CreeLe, b.ModifierLe)),
                scopes,
                ct))
            .WithName("GetReferentielBureauxDouanes");

        group.MapGet("/positions-tarifaires", async (
            string? search,
            string? regime,
            int? take,
            IMemoryCache cache,
            IReferentielServiceClient client,
            IPositionTarifaireRepository positionsRepo,
            IServiceScopeFactory scopes,
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

                var items = FilterPositions(all, search, regime, take);
                KickSoftSync(scopes, includePositions: true);
                return Results.Ok(items);
            }
            catch (Exception ex) when (IsRemoteFailure(ex))
            {
                var local = (await positionsRepo.GetActifsAsync(ct)).ToList();
                if (local.Count == 0)
                    return RemoteDownProblem(ex);

                var asDto = local.Select(p => new ReferentielPositionTarifaireDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Description = p.Description,
                    Actif = p.Actif
                }).ToList();

                return Results.Ok(FilterPositions(asDto, search, regime, take));
            }
        })
        .WithName("GetReferentielPositionsTarifaires")
        .WithSummary("Positions tarifaires — MS (cache) ou copie locale");

        // Carnet d'adresses : scoped organisation — pas de réplique locale métier
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
        .WithName("GetReferentielCarnetAdresses");

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

    private static async Task<IResult> ServeListAsync<TRemote>(
        Func<Task<List<TRemote>>> remoteLoader,
        Func<TRemote, object> remoteMapper,
        Func<Task<IEnumerable<object>>> localLoader,
        IServiceScopeFactory scopes,
        CancellationToken ct)
    {
        try
        {
            var items = await remoteLoader();
            KickSoftSync(scopes);
            return Results.Ok(items.Select(remoteMapper));
        }
        catch (Exception ex) when (IsRemoteFailure(ex))
        {
            var local = (await localLoader()).ToList();
            if (local.Count == 0)
                return RemoteDownProblem(ex);
            return Results.Ok(local);
        }
    }

    private static async Task<List<ReferentielPortDto>> FilterPortsRemoteAsync(
        List<ReferentielPortDto> ports,
        Guid? paysId,
        string? codePays,
        IReferentielServiceClient client,
        CancellationToken ct)
    {
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

        return ports;
    }

    private static IEnumerable<object> FilterPositions(
        List<ReferentielPositionTarifaireDto> all,
        string? search,
        string? regime,
        int? take)
    {
        IEnumerable<ReferentielPositionTarifaireDto> query = all.Where(p => p.Actif);

        if (!string.IsNullOrWhiteSpace(regime))
        {
            var regimeFilter = regime.Trim().ToUpperInvariant();
            var withRegime = query.Where(p =>
                !string.IsNullOrWhiteSpace(p.Regime)
                && string.Equals(p.Regime, regimeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            if (withRegime.Count > 0)
                query = withRegime;
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

        var limit = Math.Clamp(take ?? (string.IsNullOrWhiteSpace(search) ? 2000 : 50), 1, 5000);
        return query
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
            });
    }

    private static void KickSoftSync(IServiceScopeFactory scopes, bool includePositions = false)
    {
        // Un seul refresh soft toutes les 15 min pour ne pas saturer le MS Référentiel.
        var gate = SoftSyncGate.Default;
        if (!gate.TryEnter())
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var sync = scope.ServiceProvider.GetRequiredService<IReferentielSyncService>();
                await sync.SyncAllAsync(includePositions, CancellationToken.None);
            }
            catch
            {
                // Soft refresh best-effort.
            }
        });
    }

    private sealed class SoftSyncGate
    {
        public static SoftSyncGate Default { get; } = new();
        private long _lastTicks;
        private readonly object _lock = new();

        public bool TryEnter()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow.Ticks;
                if (_lastTicks != 0 && now - _lastTicks < TimeSpan.FromMinutes(15).Ticks)
                    return false;
                _lastTicks = now;
                return true;
            }
        }
    }

    private static bool IsRemoteFailure(Exception ex) =>
        ex is ApiException
        or HttpRequestException
        or TaskCanceledException
        or TimeoutException
        || (ex.InnerException is HttpRequestException or TaskCanceledException or TimeoutException);

    private static IResult RemoteDownProblem(Exception ex) =>
        Results.Problem(
            detail: "MS Référentiel indisponible et copie locale CO vide. Réessayez POST /api/referentiel/sync quand le référentiel est rétabli. " +
                    ex.Message,
            statusCode: StatusCodes.Status502BadGateway);

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

    private static object MapLocalItem(
        Guid id,
        string code,
        string? nom,
        bool actif,
        string? creePar,
        string? modifiePar,
        DateTime? creeLe,
        DateTime? modifierLe) => new PaysDto
    {
        Id = id,
        Code = code,
        Nom = nom ?? code,
        Actif = actif,
        CreePar = creePar,
        ModifiePar = modifiePar,
        CreeLe = creeLe,
        ModifierLe = modifierLe
    };

    private static object MapLocalPort(Port p) =>
        MapLocalPortLike(p.Id, p.Code, p.Nom, p.PaysId, p.Type, p.Actif, p.CreePar, p.ModifiePar, p.CreeLe, p.ModifierLe);

    private static object MapLocalPortLike(
        Guid id,
        string code,
        string? nom,
        Guid? paysId,
        string? type,
        bool actif,
        string? creePar,
        string? modifiePar,
        DateTime? creeLe,
        DateTime? modifierLe) => new PortDto
    {
        Id = id,
        Code = code,
        Nom = nom ?? code,
        PaysId = paysId,
        Type = type,
        Actif = actif,
        CreePar = creePar,
        ModifiePar = modifiePar,
        CreeLe = creeLe,
        ModifierLe = modifierLe
    };
}
