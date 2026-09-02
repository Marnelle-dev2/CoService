using COService.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Client MS Enrôlement SEG (port 8300).
/// </summary>
public class EnrolementActeursServiceClient : IEnrolementServiceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<EnrolementActeursServiceClient> _logger;

    public EnrolementActeursServiceClient(
        HttpClient http,
        ILogger<EnrolementActeursServiceClient> logger)
    {
        _http = http;
        _logger = logger;
        _logger.LogInformation("Client ActeursService configuré: {BaseAddress}", _http.BaseAddress);
    }

    public Task<List<OrganisationRemoteDto>> GetAllOrganisationsAsync(CancellationToken cancellationToken = default)
        => EnrolementActeursApiClient.GetAllOrganisationsAsync(_http, cancellationToken);

    public Task<List<OrganisationRemoteDto>> GetOrganisationsByTypeAsync(
        string type,
        CancellationToken cancellationToken = default)
        => EnrolementActeursApiClient.GetOrganisationsByTypeAsync(_http, type, cancellationToken);

    public async Task<OrganisationRemoteDto> GetOrganisationByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var organisation = await EnrolementActeursApiClient.GetOrganisationByCodeAsync(
            _http,
            code,
            cancellationToken);

        if (organisation == null)
            throw new HttpRequestException($"Organisation '{code}' introuvable dans l'enrôlement.");

        return organisation;
    }
}
