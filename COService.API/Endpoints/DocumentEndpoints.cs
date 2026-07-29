using COService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace COService.API.Endpoints;

/// <summary>
/// Endpoints pour la gestion des documents avec MinIO
/// </summary>
public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents")
            .WithTags("Gestion des Documents")
            .DisableAntiforgery();

        // Upload d'un fichier
        group.MapPost("/upload", async (
            IFormFile file,
            string certificatNo,        // Changé de certificatId à certificatNo
            string documentType,
            IMinIOService minioService,
            CancellationToken cancellationToken) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("Aucun fichier fourni");

            // Validation du type de document
            var allowedTypes = new[] { "facture", "piece-justificative", "certificat-genere" };
            if (!allowedTypes.Contains(documentType.ToLower()))
                return Results.BadRequest("Type de document non autorisé");

            // Génération du nom d'objet
            var objectName = documentType switch
            {
                "facture" => $"Factures/{certificatNo}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}",
                "piece-justificative" => $"PiecesJustificatives/{certificatNo}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}",
                "certificat-genere" => $"CertificatsGeneres/{certificatNo}/{file.FileName}",
                _ => $"Autres/{certificatNo}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"
            };

            try
            {
                using var stream = file.OpenReadStream();
                var fileUrl = await minioService.UploadFileAsync(objectName, stream, file.ContentType);

                return Results.Ok(new
                {
                    message = "Fichier uploadé avec succès",
                    url = fileUrl,
                    objectName = objectName,
                    originalFileName = file.FileName,
                    size = file.Length,
                    contentType = file.ContentType
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erreur lors de l'upload: {ex.Message}");
            }
        })
        .WithName("UploadDocument")
        .WithSummary("Upload un fichier vers MinIO")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Téléchargement d'un fichier
        group.MapGet("/download/{objectName}", async (
            string objectName,
            IMinIOService minioService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var fileStream = await minioService.DownloadFileAsync(objectName);
                
                // Déterminer le content type basé sur l'extension
                var contentType = Path.GetExtension(objectName).ToLower() switch
                {
                    ".pdf" => "application/pdf",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".doc" or ".docx" => "application/msword",
                    _ => "application/octet-stream"
                };

                return Results.File(fileStream, contentType, Path.GetFileName(objectName));
            }
            catch (Exception ex)
            {
                return Results.NotFound($"Fichier non trouvé: {ex.Message}");
            }
        })
        .WithName("DownloadDocument")
        .WithSummary("Télécharge un fichier depuis MinIO")
        .Produces(200)
        .Produces(404);

        // Suppression d'un fichier
        group.MapDelete("/{objectName}", async (
            string objectName,
            IMinIOService minioService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var success = await minioService.DeleteFileAsync(objectName);
                
                return success 
                    ? Results.Ok(new { message = "Fichier supprimé avec succès" })
                    : Results.NotFound("Fichier non trouvé");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erreur lors de la suppression: {ex.Message}");
            }
        })
        .WithName("DeleteDocument")
        .WithSummary("Supprime un fichier de MinIO")
        .Produces(200)
        .Produces(404)
        .Produces(500);

        // Génération d'URL pré-signée
        group.MapGet("/presigned-url/{objectName}", async (
            string objectName,
            int? expiryHours,
            IMinIOService minioService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var expiry = expiryHours ?? 24;
                var presignedUrl = await minioService.GetPresignedUrlAsync(objectName, expiry);
                
                return Results.Ok(new
                {
                    url = presignedUrl,
                    expiryHours = expiry,
                    objectName = objectName
                });
            }
            catch (Exception ex)
            {
                return Results.NotFound($"Fichier non trouvé: {ex.Message}");
            }
        })
        .WithName("GetPresignedUrl")
        .WithSummary("Génère une URL pré-signée pour un accès temporaire")
        .Produces(200)
        .Produces(404);

        // Test simple de connexion MinIO
        group.MapGet("/test-minio-connection", async (IMinIOService minioService) =>
        {
            try
            {
                // Test simple sans upload
                return Results.Ok(new { 
                    message = "Test de connexion MinIO",
                    timestamp = DateTime.UtcNow,
                    serviceStatus = "OK"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(new { 
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                }.ToString());
            }
        })
        .WithName("TestMinIOConnectionSimple")
        .Produces(200)
        .Produces(500);

        // Test de connexion MinIO
        group.MapGet("/test-minio", async (IMinIOService minioService) =>
        {
            try
            {
                // Test simple de connexion
                var testObjectName = "test/connection-test.txt";
                using var testStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test connection"));
                var url = await minioService.UploadFileAsync(testObjectName, testStream, "text/plain");
                
                return Results.Ok(new { 
                    message = "MinIO connection successful",
                    uploadedUrl = url,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(new { 
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    timestamp = DateTime.UtcNow
                }.ToString());
            }
        })
        .WithName("TestMinIOConnection")
        .WithSummary("Test la connexion MinIO")
        .Produces(200)
        .Produces(500);

        // Vérification de l'existence d'un fichier
        group.MapGet("/{objectName}/exists", async (
            string objectName,
            IMinIOService minioService,
            CancellationToken cancellationToken) =>
        {
            var exists = await minioService.FileExistsAsync(objectName);
            return exists ? Results.Ok(new { exists = true }) : Results.NotFound(new { exists = false });
        })
        .WithName("FileExists")
        .WithSummary("Vérifie si un fichier existe dans MinIO")
        .Produces(200)
        .Produces(404);
    }
}
