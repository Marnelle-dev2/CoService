using COService.API.Auth;
using COService.Application.DTOs;
using COService.Infrastructure.ExternalServices;
using Refit;

namespace COService.API.Endpoints;

/// <summary>
/// Partenaires (chambres) — Acteurs / Organisation, avec fallback CCIAM si liste vide.
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

                if (partenaires.Count == 0)
                {
                    partenaires = ChambresCommerceFallback.List
                        .Select(OrganisationRemoteMapper.ToPartenaire)
                        .Where(PartenaireFilters.IsChambreCommerce)
                        .ToList();
                }

                return Results.Ok(partenaires);
            }
            catch (ApiException ex)
            {
                // Gateway 401/5xx : fallback CCIAM pour ne pas bloquer la création CO
                if ((int)ex.StatusCode is >= 400)
                {
                    var fallback = ChambresCommerceFallback.List
                        .Select(OrganisationRemoteMapper.ToPartenaire)
                        .Where(PartenaireFilters.IsChambreCommerce)
                        .ToList();
                    if (fallback.Count > 0)
                        return Results.Ok(fallback);
                }

                return Results.Problem(
                    detail: $"Gateway/Organisation: {ex.StatusCode} — {ex.Content}",
                    statusCode: (int)ex.StatusCode);
            }
            catch (Exception)
            {
                var fallback = ChambresCommerceFallback.List
                    .Select(OrganisationRemoteMapper.ToPartenaire)
                    .Where(PartenaireFilters.IsChambreCommerce)
                    .ToList();
                if (fallback.Count > 0)
                    return Results.Ok(fallback);

                return Results.Problem(
                    detail: "Impossible de charger les chambres de commerce (Acteurs / Organisation).",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("GetAllPartenaires")
        .WithSummary("Chambres de commerce (CCI) — Acteurs / Organisation / fallback CCIAM")
        .Produces<IEnumerable<PartenaireDto>>(StatusCodes.Status200OK);

        group.MapGet("/code/{code}", async (
            string code,
            IEnrolementServiceClient organisationClient,
            CancellationToken cancellationToken) =>
        {
            try
            {
                OrganisationRemoteDto? org = null;
                try
                {
                    org = await organisationClient.GetOrganisationByCodeAsync(code, cancellationToken);
                }
                catch (HttpRequestException)
                {
                    org = ChambresCommerceFallback.FindByCode(code);
                }

                org ??= ChambresCommerceFallback.FindByCode(code);
                if (org == null || !string.Equals(org.Type, "PARTENAIRE", StringComparison.OrdinalIgnoreCase))
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
                var fallback = ChambresCommerceFallback.FindByCode(code);
                if (fallback != null)
                    return Results.Ok(OrganisationRemoteMapper.ToPartenaire(fallback));

                return Results.NotFound(new { message = $"Partenaire {code} introuvable." });
            }
            catch (ApiException ex)
            {
                var fallback = ChambresCommerceFallback.FindByCode(code);
                if (fallback != null)
                    return Results.Ok(OrganisationRemoteMapper.ToPartenaire(fallback));

                return Results.Problem(
                    detail: $"Gateway/Organisation: {ex.StatusCode} — {ex.Content}",
                    statusCode: (int)ex.StatusCode);
            }
            catch (Exception ex)
            {
                var fallback = ChambresCommerceFallback.FindByCode(code);
                if (fallback != null)
                    return Results.Ok(OrganisationRemoteMapper.ToPartenaire(fallback));

                return Results.Problem(
                    detail: $"Impossible de joindre le MS Organisation: {ex.Message}",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("GetPartenaireByCode")
        .WithSummary("Partenaire / chambre par code (Acteurs / fallback CCIAM)")
        .Produces<PartenaireDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
