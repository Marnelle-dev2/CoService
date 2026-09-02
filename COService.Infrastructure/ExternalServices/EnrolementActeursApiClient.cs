using System.Net.Http.Json;
using System.Text.Json;
using COService.Application.DTOs;

namespace COService.Infrastructure.ExternalServices;

internal static class EnrolementActeursApiClient
{
    private const int PageSize = 100;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<List<OrganisationRemoteDto>> GetAllOrganisationsAsync(
        HttpClient http,
        CancellationToken cancellationToken = default)
    {
        var chargeurs = await GetOrganisationsByTypeAsync(http, "EXPORTATEUR", cancellationToken);
        var transitaires = await GetOrganisationsByTypeAsync(http, "TRANSITAIRE", cancellationToken);
        var banques = await GetOrganisationsByTypeAsync(http, "BANQUE", cancellationToken);

        return chargeurs
            .Concat(transitaires)
            .Concat(banques)
            .GroupBy(o => o.Code.Trim().ToUpperInvariant())
            .Select(g => g.First())
            .ToList();
    }

    public static async Task<List<OrganisationRemoteDto>> GetOrganisationsByTypeAsync(
        HttpClient http,
        string type,
        CancellationToken cancellationToken = default)
    {
        var normalized = type.Trim().ToUpperInvariant();
        return normalized switch
        {
            "EXPORTATEUR" or "IMPORTATEUR" or "CHARGEUR" =>
                await FetchActeursSocietesAsync(http, normalized, cancellationToken),
            "TRANSITAIRE" =>
                await FetchPrestatairesAsync(http, "transitaires", "TRANSITAIRE", cancellationToken),
            "BANQUE" =>
                await FetchPartenairesAsync(http, "banques", "BANQUE", cancellationToken),
            _ =>
                await SearchActeursAsync(http, normalized, cancellationToken)
        };
    }

    public static async Task<OrganisationRemoteDto?> GetOrganisationByCodeAsync(
        HttpClient http,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var trimmed = code.Trim();
        var fromSearch = await SearchActeursAsync(http, trimmed, cancellationToken, trimmed);
        var match = fromSearch.FirstOrDefault(o =>
            string.Equals(o.Code, trimmed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(o.Niu, trimmed, StringComparison.OrdinalIgnoreCase));

        if (match != null)
            return match;

        var all = await GetAllOrganisationsAsync(http, cancellationToken);
        return all.FirstOrDefault(o =>
            string.Equals(o.Code, trimmed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(o.Niu, trimmed, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<List<OrganisationRemoteDto>> FetchActeursSocietesAsync(
        HttpClient http,
        string typeLabel,
        CancellationToken cancellationToken)
    {
        var query = "EntiteType=CLIENT&EntiteLabel=CHARGEUR&Statut=SOCIETE&Actif=true";
        var acteurs = await FetchPagedAsync<EnrolementActeurDto>(
            http,
            $"/api/v1/acteurs/search?{query}",
            cancellationToken);

        return acteurs
            .Where(a => !string.IsNullOrWhiteSpace(a.Code))
            .Select(a => MapActeur(a, typeLabel))
            .ToList();
    }

    private static async Task<List<OrganisationRemoteDto>> FetchPrestatairesAsync(
        HttpClient http,
        string segment,
        string typeLabel,
        CancellationToken cancellationToken)
    {
        var items = await FetchPagedAsync<EnrolementPrestataireDto>(
            http,
            $"/api/v1/prestataires/{segment}",
            cancellationToken);

        return items
            .Where(p => !string.IsNullOrWhiteSpace(p.Code))
            .Select(p => MapPrestataire(p, typeLabel))
            .ToList();
    }

    private static async Task<List<OrganisationRemoteDto>> FetchPartenairesAsync(
        HttpClient http,
        string segment,
        string typeLabel,
        CancellationToken cancellationToken)
    {
        var items = await FetchPagedAsync<EnrolementPrestataireDto>(
            http,
            $"/api/v1/partenaires/{segment}",
            cancellationToken);

        return items
            .Where(p => !string.IsNullOrWhiteSpace(p.Code))
            .Select(p => MapPrestataire(p, typeLabel))
            .ToList();
    }

    private static async Task<List<OrganisationRemoteDto>> SearchActeursAsync(
        HttpClient http,
        string query,
        CancellationToken cancellationToken,
        string? code = null)
    {
        var parts = new List<string> { "Actif=true" };
        if (!string.IsNullOrWhiteSpace(code))
            parts.Add($"Code={Uri.EscapeDataString(code)}");
        else if (!string.IsNullOrWhiteSpace(query))
            parts.Add($"Q={Uri.EscapeDataString(query)}");

        var acteurs = await FetchPagedAsync<EnrolementActeurDto>(
            http,
            $"/api/v1/acteurs/search?{string.Join('&', parts)}",
            cancellationToken);

        return acteurs
            .Where(a => !string.IsNullOrWhiteSpace(a.Code))
            .Select(a => MapActeur(a, "EXPORTATEUR"))
            .ToList();
    }

    private static async Task<List<T>> FetchPagedAsync<T>(
        HttpClient http,
        string path,
        CancellationToken cancellationToken)
    {
        var results = new List<T>();
        var separator = path.Contains('?') ? '&' : '?';
        var page = 1;
        var totalPages = 1;

        while (page <= totalPages)
        {
            var url = $"{path}{separator}PageNumber={page}&PageSize={PageSize}";
            var response = await http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"ActeursService {url} → {(int)response.StatusCode}: {errorBody[..Math.Min(errorBody.Length, 300)]}");
            }

            var payload = await response.Content.ReadFromJsonAsync<EnrolementPagedResponse<T>>(
                JsonOptions,
                cancellationToken);
            if (payload?.Items is not { Count: > 0 })
                break;

            results.AddRange(payload.Items);
            totalPages = payload.TotalPages > 0
                ? payload.TotalPages
                : Math.Max((int)Math.Ceiling(payload.TotalCount / (double)PageSize), 1);
            page++;
        }

        return results;
    }

    private static OrganisationRemoteDto MapActeur(EnrolementActeurDto acteur, string type) =>
        new()
        {
            Code = acteur.Code!.Trim(),
            Sigle = acteur.RaisonSociale?.Trim(),
            Name = acteur.RaisonSociale?.Trim(),
            Type = type,
            Niu = acteur.Niu?.Trim(),
            Adresse = acteur.Adresse?.Trim(),
            Telephone = acteur.Telephone?.Trim(),
            Email = acteur.Email?.Trim(),
            Departement = acteur.Departement?.Trim(),
            IsActive = acteur.Actif ?? true
        };

    private static OrganisationRemoteDto MapPrestataire(EnrolementPrestataireDto item, string type) =>
        new()
        {
            Code = item.Code!.Trim(),
            Sigle = item.RaisonSociale?.Trim(),
            Name = item.RaisonSociale?.Trim(),
            Type = type,
            Niu = item.Niu?.Trim(),
            Adresse = item.Adresse?.Trim(),
            Telephone = item.Telephone?.Trim(),
            Email = item.Email?.Trim(),
            Departement = item.Departement?.Trim(),
            IsActive = item.Actif ?? true
        };
}
