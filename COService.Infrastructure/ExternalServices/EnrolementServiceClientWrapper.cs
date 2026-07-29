using System.Net.Http.Headers;
using COService.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Client Organisation via API Gateway.
/// Exemple : http://srv-guot-cont.gumar.local:5000/organisation/Organisations
/// </summary>
public class EnrolementServiceClientWrapper : IEnrolementServiceClient
{
    private readonly ILogger<EnrolementServiceClientWrapper> _logger;
    private readonly IEnrolementServiceClient _client;

    public EnrolementServiceClientWrapper(
        ILogger<EnrolementServiceClientWrapper> logger,
        IConfiguration configuration)
    {
        _logger = logger;

        var serviceConfig = configuration.GetSection("ExternalServices:EnrolementService");
        var useGateway = serviceConfig.GetValue("UseApiGateway", true);
        var path = serviceConfig.GetValue<string>("Path") ?? "/organisation";
        var timeout = serviceConfig.GetValue("Timeout", 30);
        var bearerToken =
            configuration.GetValue<string>("ApiGateway:BearerToken")
            ?? serviceConfig.GetValue<string>("BearerToken");

        string baseAddress;
        if (useGateway)
        {
            // Base = gateway seul ; le Path /organisation est dans les attributs Refit
            // (un Get("/...") avec BaseAddress contenant déjà un path remplacerait ce path)
            baseAddress = configuration.GetValue<string>("ApiGateway:BaseUrl")
                ?? throw new InvalidOperationException("ApiGateway:BaseUrl non configuré");
            _ = path; // conservé en config pour documentation / futurs usages
        }
        else
        {
            baseAddress = (serviceConfig.GetValue<string>("BaseUrl") ?? "http://localhost:5000").TrimEnd('/');
        }

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(timeout)
        };

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        else
        {
            _logger.LogWarning(
                "EnrolementService:BearerToken vide — le gateway renverra probablement 401 sur /organisation");
        }

        _client = RestService.For<IEnrolementServiceClient>(httpClient);

        _logger.LogInformation(
            "Client Organisation configuré ({Mode}): {BaseAddress}",
            useGateway ? "API Gateway" : "direct",
            baseAddress);
    }

    public Task<List<OrganisationRemoteDto>> GetAllOrganisationsAsync(CancellationToken cancellationToken = default)
        => _client.GetAllOrganisationsAsync(cancellationToken);

    public Task<List<OrganisationRemoteDto>> GetOrganisationsByTypeAsync(string type, CancellationToken cancellationToken = default)
        => _client.GetOrganisationsByTypeAsync(type, cancellationToken);

    public Task<OrganisationRemoteDto> GetOrganisationByCodeAsync(string code, CancellationToken cancellationToken = default)
        => _client.GetOrganisationByCodeAsync(code, cancellationToken);
}
