using COService.Application.DTOs;
using COService.Infrastructure.ExternalServices;
using Refit;

namespace COService.API.Endpoints;

/// <summary>
/// Exportateurs — lecture live Gateway → /organisation/Organisations/type/EXPORTATEUR
/// </summary>
public static class ExportateurEndpoints
{
    public static void MapExportateurEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exportateurs")
            .WithTags("Exportateurs");

        group.MapGet("/", async (
            IEnrolementServiceClient organisationClient,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var orgs = await organisationClient.GetOrganisationsByTypeAsync("EXPORTATEUR", cancellationToken);
                var exportateurs = orgs.Select(OrganisationRemoteMapper.ToExportateur).ToList();
                return Results.Ok(exportateurs);
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
        .WithName("GetAllExportateurs")
        .WithSummary("Exportateurs depuis Organisation (type EXPORTATEUR) via Gateway")
        .Produces<IEnumerable<ExportateurDto>>(StatusCodes.Status200OK);

        group.MapGet("/code/{code}", async (
            string code,
            IEnrolementServiceClient organisationClient,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var org = await organisationClient.GetOrganisationByCodeAsync(code, cancellationToken);
                if (!string.Equals(org.Type, "EXPORTATEUR", StringComparison.OrdinalIgnoreCase))
                {
                    return Results.NotFound(new { message = $"Aucune organisation EXPORTATEUR avec le code {code}." });
                }
                return Results.Ok(OrganisationRemoteMapper.ToExportateur(org));
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.NotFound(new { message = $"Exportateur {code} introuvable." });
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
        .WithName("GetExportateurByCode")
        .WithSummary("Exportateur par code depuis Organisation")
        .Produces<ExportateurDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
