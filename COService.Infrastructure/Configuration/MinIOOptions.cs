namespace COService.Infrastructure.Configuration;

/// <summary>
/// Options de configuration pour MinIO
/// </summary>
public class MinIOOptions
{
    public const string SectionName = "MinIO";

    /// <summary>
    /// Endpoint du serveur MinIO (ex: localhost:9000)
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Clé d'accès MinIO
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Clé secrète MinIO
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Nom du bucket principal
    /// </summary>
    public string BucketName { get; set; } = "ms-documents";

    /// <summary>
    /// Préfixe pour ce microservice (sous-dossier)
    /// </summary>
    public string ServicePrefix { get; set; } = "CertificatOrigines";

    /// <summary>
    /// Utiliser SSL ou non
    /// </summary>
    public bool UseSSL { get; set; } = false;
}
