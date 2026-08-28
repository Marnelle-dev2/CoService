using COService.Application.Auth;
using COService.Infrastructure.Auth;

namespace COService.API.Auth;

/// <summary>
/// Lit les en-têtes POC envoyés par le client (profil gateway simulé) en attendant Auth MS.
/// </summary>
public class PocAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _enabled;

    public PocAuthorizationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _enabled = configuration.GetValue<bool>("PocAuth:Enabled", true);
    }

    public async Task InvokeAsync(HttpContext context, IPocUserContext pocUser)
    {
        if (!_enabled)
        {
            context.Items[nameof(IPocUserContext)] = new PocUserContext { IsEnabled = false };
            await _next(context);
            return;
        }

        var profile = NormalizeProfile(context.Request.Headers["X-Poc-Profile"].FirstOrDefault());
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault()
            ?? context.Request.Headers["X-User-Name"].FirstOrDefault();
        var organisationCode = context.Request.Headers["X-Organisation-Code"].FirstOrDefault();

        var userContext = new PocUserContext
        {
            IsEnabled = true,
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim(),
            OrganisationCode = string.IsNullOrWhiteSpace(organisationCode) ? null : organisationCode.Trim(),
            Profile = profile
        };

        context.Items[nameof(IPocUserContext)] = userContext;
        await _next(context);
    }

    private static string NormalizeProfile(string? raw)
    {
        var value = (raw ?? "lecteur").Trim().ToLowerInvariant();
        return value switch
        {
            "exportateur" => "exportateur",
            "chambre" or "cciam" => "chambre",
            "transitaire" => "transitaire",
            "admin" => "admin",
            _ => "lecteur"
        };
    }
}

public static class PocAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UsePocAuthorization(this IApplicationBuilder app)
        => app.UseMiddleware<PocAuthorizationMiddleware>();
}
