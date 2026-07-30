using COService.Application.DTOs;
using COService.Application.Services;
using COService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace COService.API.Endpoints;

/// <summary>
/// Endpoints pour la gestion des certificats d'origine
/// </summary>
public static class CertificatEndpoints
{
    public static void MapCertificatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/certificats")
            .WithTags("Certificats d'origine")
            .DisableAntiforgery();

        // GET /api/certificats - Liste tous les certificats
        group.MapGet("/", async (
            ICertificatOrigineService service,
            CancellationToken cancellationToken) =>
        {
            var certificats = await service.GetAllCertificatsAsync(cancellationToken);
            return Results.Ok(certificats);
        })
        .WithName("GetAllCertificats")
        .WithSummary("Récupère tous les certificats d'origine")
        .Produces<IEnumerable<CertificatOrigineDto>>(StatusCodes.Status200OK);

        // GET /api/certificats/{id} - Récupère un certificat par ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            ICertificatOrigineService service,
            CancellationToken cancellationToken) =>
        {
            var certificat = await service.GetCertificatByIdAsync(id, cancellationToken);
            return certificat == null
                ? Results.NotFound(new { message = $"Certificat avec l'ID {id} introuvable." })
                : Results.Ok(certificat);
        })
        .WithName("GetCertificatById")
        .WithSummary("Récupère un certificat par son identifiant")
        .Produces<CertificatOrigineDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/certificats/numero/{certificateNo} - Récupère un certificat par numéro
        group.MapGet("/numero/{certificateNo}", async (
            string certificateNo,
            ICertificatOrigineService service,
            CancellationToken cancellationToken) =>
        {
            var certificat = await service.GetCertificatByNoAsync(certificateNo, cancellationToken);
            return certificat == null
                ? Results.NotFound(new { message = $"Certificat avec le numéro {certificateNo} introuvable." })
                : Results.Ok(certificat);
        })
        .WithName("GetCertificatByNo")
        .WithSummary("Récupère un certificat par son numéro")
        .Produces<CertificatOrigineDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/certificats/exportateur/{exportateur} - Récupère les certificats par exportateur
        group.MapGet("/exportateur/{exportateur}", async (
            string exportateur,
            ICertificatOrigineService service,
            CancellationToken cancellationToken) =>
        {
            var certificats = await service.GetCertificatsByExportateurAsync(exportateur, cancellationToken);
            return Results.Ok(certificats);
        })
        .WithName("GetCertificatsByExportateur")
        .WithSummary("Récupère les certificats par exportateur")
        .Produces<IEnumerable<CertificatOrigineDto>>(StatusCodes.Status200OK);

        // GET /api/certificats/statut/{statut} - Récupère les certificats par statut
        group.MapGet("/statut/{statut}", async (
            string statut,
            ICertificatOrigineService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var certificats = await service.GetCertificatsByStatutAsync(statut, cancellationToken);
                return Results.Ok(certificats);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("GetCertificatsByStatut")
        .WithSummary("Récupère les certificats par statut")
        .Produces<IEnumerable<CertificatOrigineDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // GET /api/certificats/pays/{paysDestination} - Récupère les certificats par pays de destination
        group.MapGet("/pays/{paysDestination}", async (
            string paysDestination,
            ICertificatOrigineService service,
            CancellationToken cancellationToken) =>
        {
            var certificats = await service.GetCertificatsByPaysDestinationAsync(paysDestination, cancellationToken);
            return Results.Ok(certificats);
        })
        .WithName("GetCertificatsByPaysDestination")
        .WithSummary("Récupère les certificats par pays de destination")
        .Produces<IEnumerable<CertificatOrigineDto>>(StatusCodes.Status200OK);

        // POST /api/certificats - Crée un nouveau certificat
        group.MapPost("/", async (
            [FromBody] CreerCertificatOrigineDto dto,
            ICertificatOrigineService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var certificat = await service.CreerCertificatAsync(dto, utilisateur, cancellationToken);
                return Results.Created($"/api/certificats/{certificat.Id}", certificat);
            }
            catch (InvalidOperationException ex)
            {
                // Si le message contient "existe déjà", c'est un conflit (409)
                // Sinon, c'est une erreur de validation (400)
                if (ex.Message.Contains("existe déjà"))
                {
                    return Results.Conflict(new { message = ex.Message });
                }
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("CreerCertificat")
        .WithSummary("Crée un nouveau certificat d'origine")
        .Accepts<CreerCertificatOrigineDto>("application/json")
        .Produces<CertificatOrigineDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        // PUT /api/certificats/{id} - Modifie un certificat
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] ModifierCertificatOrigineDto dto,
            ICertificatOrigineService service,
            [FromHeader(Name = "X-User-Id")] string? utilisateur,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var certificat = await service.ModifierCertificatAsync(id, dto, utilisateur, cancellationToken);
                return Results.Ok(certificat);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("ModifierCertificat")
        .WithSummary("Modifie un certificat d'origine")
        .Accepts<ModifierCertificatOrigineDto>("application/json")
        .Produces<CertificatOrigineDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/certificats/{id} - Supprime un certificat
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ICertificatOrigineService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await service.SupprimerCertificatAsync(id, cancellationToken);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("SupprimerCertificat")
        .WithSummary("Supprime un certificat d'origine")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/certificats/avec-documents - Crée un certificat avec documents
        group.MapPost("/avec-documents", async (
            CreateCertificatWithDocumentsDto dto,
            ICertificatOrigineService certificatService,
            IMinIOService minioService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // 1. Upload de la facture (obligatoire)
                if (dto.FactureFile == null || dto.FactureFile.Length == 0)
                    return Results.BadRequest("La facture est obligatoire");

                var factureObjectName = $"Factures/{dto.CertificateNo}/{Guid.NewGuid()}{Path.GetExtension(dto.FactureFile.FileName)}";
                var factureUrl = await minioService.UploadFileAsync(factureObjectName, dto.FactureFile.OpenReadStream(), dto.FactureFile.ContentType);

                // 2. Upload des pièces justificatives (optionnelles)
                var piecesUrls = new List<string>();
                if (dto.PiecesJustificatives != null)
                {
                    foreach (var piece in dto.PiecesJustificatives)
                    {
                        var pieceObjectName = $"PiecesJustificatives/{dto.CertificateNo}/{Guid.NewGuid()}{Path.GetExtension(piece.FileName)}";
                        var pieceUrl = await minioService.UploadFileAsync(pieceObjectName, piece.OpenReadStream(), piece.ContentType);
                        piecesUrls.Add(pieceUrl);
                    }
                }

                // 3. Création du certificat avec les URLs
                var certificatDto = new CreerCertificatOrigineDto
                {
                    CertificateNo = dto.CertificateNo,
                    ExportateurNIU = dto.ExportateurNIU,
                    PartenaireNIU = dto.PartenaireNIU,
                    PaysDestinationCode = dto.PaysDestinationCode,
                    Observation = dto.Observation,
                    Navire = dto.Navire
                    // FactureUrl et PiecesJustificativesUrls seront gérés dans le service
                };

                var certificat = await certificatService.CreerCertificatAsync(certificatDto, cancellationToken: cancellationToken);

                return Results.Created($"/api/certificats/{certificat.Id}", certificat);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erreur lors de la création du certificat: {ex.Message}");
            }
        })
        .WithName("CreateCertificatWithDocuments")
        .WithSummary("Crée un certificat d'origine avec les documents associés")
        .Accepts<CreateCertificatWithDocumentsDto>("multipart/form-data")
        .Produces<CertificatOrigineDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}

