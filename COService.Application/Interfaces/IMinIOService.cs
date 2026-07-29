namespace COService.Application.Interfaces;

/// <summary>
/// Interface du service MinIO pour la gestion des fichiers
/// </summary>
public interface IMinIOService
{
    /// <summary>
    /// Uploade un fichier vers MinIO
    /// </summary>
    /// <param name="objectName">Nom de l'objet dans MinIO</param>
    /// <param name="fileStream">Stream du fichier</param>
    /// <param name="contentType">Type de contenu MIME</param>
    /// <returns>URL du fichier uploadé</returns>
    Task<string> UploadFileAsync(string objectName, Stream fileStream, string contentType);

    /// <summary>
    /// Télécharge un fichier depuis MinIO
    /// </summary>
    /// <param name="objectName">Nom de l'objet dans MinIO</param>
    /// <returns>Stream du fichier</returns>
    Task<Stream> DownloadFileAsync(string objectName);

    /// <summary>
    /// Supprime un fichier de MinIO
    /// </summary>
    /// <param name="objectName">Nom de l'objet dans MinIO</param>
    /// <returns>True si supprimé, False sinon</returns>
    Task<bool> DeleteFileAsync(string objectName);

    /// <summary>
    /// Génère une URL pré-signée pour un accès temporaire
    /// </summary>
    /// <param name="objectName">Nom de l'objet dans MinIO</param>
    /// <param name="expiryHours">Durée de validité en heures</param>
    /// <returns>URL pré-signée</returns>
    Task<string> GetPresignedUrlAsync(string objectName, int expiryHours = 24);

    /// <summary>
    /// Vérifie si un fichier existe dans MinIO
    /// </summary>
    /// <param name="objectName">Nom de l'objet dans MinIO</param>
    /// <returns>True si existe, False sinon</returns>
    Task<bool> FileExistsAsync(string objectName);
}
