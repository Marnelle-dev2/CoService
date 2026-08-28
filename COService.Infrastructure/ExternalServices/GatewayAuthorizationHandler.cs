using System.Net.Http.Headers;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Injecte le Bearer gateway à jour sur chaque requête sortante.
/// </summary>
public sealed class GatewayAuthorizationHandler : DelegatingHandler
{
    private readonly IGatewayTokenProvider _tokenProvider;

    public GatewayAuthorizationHandler(IGatewayTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetBearerTokenAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
