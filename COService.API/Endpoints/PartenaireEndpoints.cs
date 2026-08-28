using COService.API.Auth;
using COService.Application.DTOs;
using COService.Infrastructure.ExternalServices;
using Refit;

namespace COService.API.Endpoints;

/// <summary>
/// Partenaires (chambres) — lecture live Gateway → /organisation/Organisations/type/PARTENAIRE
/// </summary>
public static class PartenaireEndpoints
{
    public static void MapPartenaireEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/partenaires")
            .WithTags("Partenaires");

        group.MapGet("/", async (
            IEnrolementServiceClient organisationClient,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var orgs = await organisationClient.GetOrganisationsByTypeAsync("PARTENAIRE", cancellationToken);
                var partenaires = orgs
                    .Select(OrganisationRemoteMapper.ToPartenaire)
                    .Where(PartenaireFilters.IsChambreCommerce)
                    .ToList();
                return Results.Ok(partenaires);
            }
            catch (ApiException ex)
            {
                return Results.Problem(
                    detail: $"Gateway/Organisation: {ex.StatusCode} — {ex.Content}",
                    statusCode: (int)ex.StatusCode);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Impossible de joindre le MS Organisation via le gateway: {ex.Message}",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("GetAllPartenaires")
        .WithSummary("Chambres de commerce (CCI) depuis Organisation via Gateway")
        .Produces<IEnumerable<PartenaireDto>>(StatusCodes.Status200OK);

        group.MapGet("/code/{code}", async (
            string code,
            IEnrolementServiceClient organisationClient,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var org = await organisationClient.GetOrganisationByCodeAsync(code, cancellationToken);
                if (!string.Equals(org.Type, "PARTENAIRE", StringComparison.OrdinalIgnoreCase))
                {
                    return Results.NotFound(new { message = $"Aucune organisation PARTENAIRE avec le code {code}." });
                }
                var partenaire = OrganisationRemoteMapper.ToPartenaire(org);
                if (!PartenaireFilters.IsChambreCommerce(partenaire))
                {
                    return Results.NotFound(new { message = $"Aucune chambre de commerce avec le code {code}." });
                }
                return Results.Ok(partenaire);
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.NotFound(new { message = $"Partenaire {code} introuvable." });
            }
            catch (ApiException ex)
            {
                return Results.Problem(
                    detail: $"Gateway/Organisation: {ex.StatusCode} — {ex.Content}",
                    statusCode: (int)ex.StatusCode);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Impossible de joindre le MS Organisation via le gateway: {ex.Message}",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("GetPartenaireByCode")
        .WithSummary("Partenaire par code depuis Organisation")
        .Produces<PartenaireDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
