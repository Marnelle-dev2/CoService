namespace COService.Infrastructure.ExternalServices;

public interface IGatewayTokenProvider
{
    Task<string?> GetBearerTokenAsync(CancellationToken cancellationToken = default);
}
