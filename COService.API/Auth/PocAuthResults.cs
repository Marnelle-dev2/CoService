using COService.Application.Auth;

namespace COService.API.Auth;

public static class PocAuthResults
{
    public static IPocUserContext GetUser(HttpContext context)
    {
        if (context.Items.TryGetValue(nameof(IPocUserContext), out var value) && value is IPocUserContext user)
        {
            return user;
        }

        return new Infrastructure.Auth.PocUserContext { IsEnabled = false };
    }

    public static IResult Forbidden(string message)
        => Results.Json(new { message }, statusCode: StatusCodes.Status403Forbidden);
}
