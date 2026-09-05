using COService.Domain.Entities;
using COService.Infrastructure.Data;
using COService.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Services;

/// <summary>
/// Pull HTTP du MS Référentiel → upsert tables locales COServiceDb.
/// </summary>
public class ReferentielSyncService : IReferentielSyncService
{
    private const string SyncUser = "REFERENTIEL";
    private readonly IReferentielServiceClient _client;
    private readonly COServiceDbContext _db;
    private readonly ILogger<ReferentielSyncService> _logger;

    public ReferentielSyncService(
        IReferentielServiceClient client,
        COServiceDbContext db,
        ILogger<ReferentielSyncService> logger)
    {
        _client = client;
        _db = db;
        _logger = logger;
    }

    public async Task<bool> IsLocalEmptyAsync(CancellationToken cancellationToken = default) =>
        !await _db.Pays.AsNoTracking().AnyAsync(cancellationToken);

    public async Task<ReferentielSyncResult> SyncAllAsync(
        bool includePositions = false,
        CancellationToken cancellationToken = default)
    {
        var result = new ReferentielSyncResult();
        var now = DateTime.UtcNow;

        result.Pays = await SafeSyncAsync("pays", result, () => SyncPaysAsync(now, cancellationToken));
        result.Ports = await SafeSyncAsync("ports", result, () => SyncPortsAsync(now, cancellationToken));
        result.Aeroports = await SafeSyncAsync("aeroports", result, () => SyncAeroportsAsync(now, cancellationToken));
        result.Devises = await SafeSyncAsync("devises", result, () => SyncDevisesAsync(now, cancellationToken));
        result.ModesTransport = await SafeSyncAsync("modes-transport", result, () => SyncModulesAsync(now, cancellationToken));
        result.Incoterms = await SafeSyncAsync("incoterms", result, () => SyncIncotermsAsync(now, cancellationToken));
        result.Departements = await SafeSyncAsync("departements", result, () => SyncDepartementsAsync(now, cancellationToken));
        result.Bureaux = await SafeSyncAsync("bureaux", result, () => SyncBureauxAsync(now, cancellationToken));
        result.Unites = await SafeSyncAsync("unites", result, () => SyncUnitesAsync(now, cancellationToken));
        result.Corridors = await SafeSyncAsync("corridors", result, () => SyncCorridorsAsync(now, cancellationToken));

        if (includePositions)
        {
            result.Positions = await SafeSyncAsync(
                "positions",
                result,
                () => SyncPositionsAsync(now, cancellationToken));
        }

        _logger.LogInformation(
            "Sync référentiel CO terminé: pays={Pays}, ports={Ports}, aeroports={Aeroports}, devises={Devises}, modes={Modes}, incoterms={Incoterms}, depts={Departements}, bureaux={Bureaux}, unites={Unites}, corridors={Corridors}, positions={Positions}, errors={ErrorCount}",
            result.Pays, result.Ports, result.Aeroports, result.Devises, result.ModesTransport,
            result.Incoterms, result.Departements, result.Bureaux, result.Unites, result.Corridors,
            result.Positions, result.Errors.Count);

        return result;
    }

    private async Task<int> SafeSyncAsync(
        string label,
        ReferentielSyncResult result,
        Func<Task<int>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sync référentiel CO — échec famille {Label}", label);
            result.Errors.Add($"{label}: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> SyncPaysAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetPaysAsync(ct);
        var rows = await _db.Pays.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var nom = ResolveNom(item.Nom, code);
            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new Pays
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Nom = nom,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.Pays.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Nom = nom;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncPortsAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetPortsAsync(ct);
        var knownPays = await _db.Pays.AsNoTracking().Select(p => p.Id).ToListAsync(ct);
        var known = knownPays.ToHashSet();
        var rows = await _db.Ports.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var nom = ResolveNom(item.Nom, code);
            Guid? paysId = item.PaysId.HasValue && known.Contains(item.PaysId.Value)
                ? item.PaysId
                : null;

            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new Port
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Nom = nom,
                    PaysId = paysId,
                    Type = item.Type,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.Ports.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Nom = nom;
                existing.PaysId = paysId ?? existing.PaysId;
                existing.Type = item.Type ?? existing.Type;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncAeroportsAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetAeroportsAsync(ct);
        var known = (await _db.Pays.AsNoTracking().Select(p => p.Id).ToListAsync(ct)).ToHashSet();
        var rows = await _db.Aeroports.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var nom = ResolveNom(item.Nom, code);
            Guid? paysId = item.PaysId.HasValue && known.Contains(item.PaysId.Value)
                ? item.PaysId
                : null;

            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new Aeroport
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Nom = nom,
                    PaysId = paysId,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.Aeroports.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Nom = nom;
                existing.PaysId = paysId ?? existing.PaysId;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncDevisesAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetDevisesAsync(ct);
        var rows = await _db.Devises.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var nom = ResolveNom(item.Nom, code);
            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new Devise
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Nom = nom,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.Devises.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Nom = nom;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncModulesAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetModeDeTransportsAsync(ct);
        var rows = await _db.Modules.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var nom = ResolveNom(item.Nom, code);
            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new Module
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Nom = nom,
                    Description = nom,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.Modules.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Nom = nom;
                existing.Description = nom;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncIncotermsAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetIncotermsAsync(ct);
        var rows = await _db.Incoterms.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var desc = string.IsNullOrWhiteSpace(item.Description) ? code : item.Description.Trim();
            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new Incoterm
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Description = desc,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.Incoterms.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Description = desc;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncDepartementsAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetDepartementsAsync(ct);
        var rows = await _db.Departements.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var nom = ResolveNom(item.Nom, code);
            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new Departement
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Nom = nom,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.Departements.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Nom = nom;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncBureauxAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetBureauxDouanesAsync(ct);
        var rows = await _db.BureauxDedouanements.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var desc = ResolveNom(item.Nom, code);
            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new BureauDedouanement
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Description = desc,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.BureauxDedouanements.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Description = desc;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncUnitesAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetUniteStatistiquesAsync(ct);
        var rows = await _db.UniteStatistiques.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var nom = ResolveNom(item.Nom, code);
            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new UniteStatistique
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Nom = nom,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.UniteStatistiques.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Nom = nom;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncCorridorsAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetCorridorsAsync(ct);
        var rows = await _db.Corridors.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = Norm(item.Code);
            if (string.IsNullOrEmpty(code))
                continue;

            var nom = ResolveNom(item.Nom, code);
            if (!TryGet(byId, byCode, item.Id, code, out var existing) || existing is null)
            {
                var entity = new Corridor
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = code,
                    Nom = nom,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.Corridors.Add(entity);
                byId[entity.Id] = entity;
                byCode[code] = entity;
            }
            else
            {
                existing.Code = code;
                existing.Nom = nom;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> SyncPositionsAsync(DateTime now, CancellationToken ct)
    {
        var remote = await _client.GetPositionTarifairesAsync(ct);
        var rows = await _db.PositionsTariffaires.ToListAsync(ct);
        var byId = rows.ToDictionary(x => x.Id);
        var byCode = IndexByCode(rows, x => x.Code);
        var count = 0;

        foreach (var item in remote.Where(x => x.Actif))
        {
            var code = item.Code?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(code))
                continue;

            var codeKey = Norm(code);
            var desc = string.IsNullOrWhiteSpace(item.Description) ? code : item.Description.Trim();
            if (desc.Length > 1000)
                desc = desc[..1000];
            var storedCode = code.Length > 50 ? code[..50] : code;

            if (!TryGet(byId, byCode, item.Id, codeKey, out var existing) || existing is null)
            {
                var entity = new PositionTarifaire
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    Code = storedCode,
                    Description = desc,
                    Actif = true,
                    CreeLe = now,
                    CreePar = SyncUser
                };
                _db.PositionsTariffaires.Add(entity);
                byId[entity.Id] = entity;
                byCode[codeKey] = entity;
            }
            else
            {
                existing.Code = storedCode;
                existing.Description = desc;
                existing.Actif = true;
                existing.ModifierLe = now;
                existing.ModifiePar = SyncUser;
            }

            count++;
        }

        if (count > 0)
            await _db.SaveChangesAsync(ct);
        return count;
    }

    private static Dictionary<string, T> IndexByCode<T>(IEnumerable<T> rows, Func<T, string> codeSelector)
    {
        var dict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            var code = Norm(codeSelector(row));
            if (!string.IsNullOrEmpty(code) && !dict.ContainsKey(code))
                dict[code] = row;
        }

        return dict;
    }

    private static bool TryGet<T>(
        Dictionary<Guid, T> byId,
        Dictionary<string, T> byCode,
        Guid id,
        string code,
        out T? entity)
    {
        if (id != Guid.Empty && byId.TryGetValue(id, out entity!))
            return true;
        if (!string.IsNullOrEmpty(code) && byCode.TryGetValue(code, out entity!))
            return true;
        entity = default;
        return false;
    }

    private static string Norm(string? code) =>
        string.IsNullOrWhiteSpace(code) ? string.Empty : code.Trim().ToUpperInvariant();

    private static string ResolveNom(string? nom, string code) =>
        string.IsNullOrWhiteSpace(nom) ? code : nom.Trim();
}
