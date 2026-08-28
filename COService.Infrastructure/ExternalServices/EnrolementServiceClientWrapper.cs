using COService.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Client Organisation via API Gateway.
/// Exemple : http://192.168.2.89:5000/organisation/Organisations
/// </summary>
public class EnrolementServiceClientWrapper : IEnrolementServiceClient
{
    private readonly ILogger<EnrolementServiceClientWrapper> _logger;
    private readonly IEnrolementServiceClient _client;

    public EnrolementServiceClientWrapper(
        ILogger<EnrolementServiceClientWrapper> logger,
        IConfiguration configuration,
        IGatewayTokenProvider tokenProvider)
    {
        _logger = logger;

        var serviceConfig = configuration.GetSection("ExternalServices:EnrolementService");
        var useGateway = serviceConfig.GetValue("UseApiGateway", true);
        var path = serviceConfig.GetValue<string>("Path") ?? "/organisation";
        var timeout = serviceConfig.GetValue("Timeout", 30);

        string baseAddress;
        if (useGateway)
        {
            baseAddress = configuration.GetValue<string>("ApiGateway:BaseUrl")
                ?? throw new InvalidOperationException("ApiGateway:BaseUrl non configuré");
            _ = path;
        }
        else
        {
            baseAddress = (serviceConfig.GetValue<string>("BaseUrl") ?? "http://localhost:5000").TrimEnd('/');
        }

        var handler = new GatewayAuthorizationHandler(tokenProvider)
        {
            InnerHandler = new SocketsHttpHandler()
        };

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(timeout)
        };

        _client = RestService.For<IEnrolementServiceClient>(httpClient);

        _logger.LogInformation(
            "Client Organisation configuré ({Mode}): {BaseAddress} — auth gateway auto/renouvelée",
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
