using COService.Application.Interfaces;
using COService.Infrastructure.Configuration;
using Minio;
using Minio.Exceptions;
using Minio.DataModel.Args;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace COService.Infrastructure.Services;

/// <summary>
/// Service MinIO pour la gestion des fichiers
/// </summary>
public class MinIOService : IMinIOService
{
    private readonly IMinioClient? _minioClient;
    private readonly string _bucketName;
    private readonly string _servicePrefix;
    private readonly ILogger<MinIOService> _logger;

    public MinIOService(IOptions<MinIOOptions> options, ILogger<MinIOService> logger)
    {
        var config = options.Value;
        
        // Logs de debug
        logger.LogInformation("Configuration MinIO reçue:");
        logger.LogInformation($"  Endpoint: {config.Endpoint}");
        logger.LogInformation($"  AccessKey: {config.AccessKey}");
        logger.LogInformation($"  SecretKey: {(string.IsNullOrEmpty(config.SecretKey) ? "NULL" : "***")}");
        logger.LogInformation($"  BucketName: {config.BucketName}");
        logger.LogInformation($"  ServicePrefix: {config.ServicePrefix}");
        logger.LogInformation($"  UseSSL: {config.UseSSL}");

        _bucketName = config.BucketName;
        _servicePrefix = config.ServicePrefix;
        _logger = logger;

        // Vérifier si la configuration est valide
        if (string.IsNullOrEmpty(config.Endpoint) || string.IsNullOrEmpty(config.AccessKey) || string.IsNullOrEmpty(config.SecretKey))
        {
            _logger.LogWarning("Configuration MinIO incomplète. Le service sera désactivé.");
            _minioClient = null!;
            return;
        }

        try
        {
            _logger.LogInformation("Tentative de connexion MinIO à {Endpoint}...", config.Endpoint);
            
            _minioClient = new MinioClient()
                .WithEndpoint(config.Endpoint)
                .WithCredentials(config.AccessKey, config.SecretKey)
                .WithSSL(config.UseSSL)
                .Build();

            _logger.LogInformation("Service MinIO initialisé avec succès !");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation du client MinIO");
            _logger.LogError($"Détails: Endpoint={config.Endpoint}, UseSSL={config.UseSSL}");
            _minioClient = null!;
        }
    }

    /// <summary>
    /// S'assure que le bucket existe
    /// </summary>
    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
            var bucketExists = await _minioClient!.BucketExistsAsync(bucketExistsArgs);
            
            if (!bucketExists)
            {
                _logger.LogInformation("Création du bucket {BucketName}", _bucketName);
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
                _logger.LogInformation("Bucket {BucketName} créé avec succès", _bucketName);
            }
            else
            {
                _logger.LogInformation("Bucket {BucketName} existe déjà", _bucketName);
            }
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du bucket {BucketName}", _bucketName);
            throw;
        }
    }

    public async Task<string> UploadFileAsync(string objectName, Stream fileStream, string contentType)
    {
        if (_minioClient == null)
        {
            throw new InvalidOperationException("Le service MinIO n'est pas configuré correctement.");
        }

        try
        {
            // Créer le bucket s'il n'existe pas (lazy loading)
            await EnsureBucketExistsAsync();
            
            // Ajouter le préfixe du service
            var fullObjectName = $"{_servicePrefix}/{objectName}";
            
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fullObjectName)
                .WithStreamData(fileStream)
                .WithContentType(contentType)
                .WithObjectSize(fileStream.Length);

            await _minioClient.PutObjectAsync(putObjectArgs);
            
            var fileUrl = $"http://{_minioClient.Config.Endpoint}/{_bucketName}/{fullObjectName}";
            _logger.LogInformation("Fichier {ObjectName} uploadé avec succès", fullObjectName);
            
            return fileUrl;
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Erreur lors de l'upload du fichier {ObjectName}", objectName);
            throw new Exception($"Erreur MinIO: {ex.Message}");
        }
    }

    public async Task<Stream> DownloadFileAsync(string objectName)
    {
        try
        {
            // Ajouter le préfixe du service
            var fullObjectName = $"{_servicePrefix}/{objectName}";
            
            var memoryStream = new MemoryStream();
            
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fullObjectName)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                });

            await _minioClient.GetObjectAsync(getObjectArgs);
            
            _logger.LogInformation("Fichier {ObjectName} téléchargé avec succès", fullObjectName);
            return memoryStream;
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Erreur lors du téléchargement du fichier {ObjectName}", objectName);
            throw new Exception($"Erreur MinIO: {ex.Message}");
        }
    }

    public async Task<bool> DeleteFileAsync(string objectName)
    {
        try
        {
            // Ajouter le préfixe du service
            var fullObjectName = $"{_servicePrefix}/{objectName}";
            
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fullObjectName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs);
            
            _logger.LogInformation("Fichier {ObjectName} supprimé avec succès", fullObjectName);
            return true;
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du fichier {ObjectName}", objectName);
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, int expiryHours = 24)
    {
        try
        {
            // Ajouter le préfixe du service
            var fullObjectName = $"{_servicePrefix}/{objectName}";
            
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fullObjectName)
                .WithExpiry((int)TimeSpan.FromHours(expiryHours).TotalSeconds);

            var url = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
            
            _logger.LogInformation("URL pré-signée générée pour {ObjectName}", fullObjectName);
            return url;
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération de l'URL pré-signée pour {ObjectName}", objectName);
            throw new Exception($"Erreur MinIO: {ex.Message}");
        }
    }

    public async Task<bool> FileExistsAsync(string objectName)
    {
        try
        {
            // Ajouter le préfixe du service
            var fullObjectName = $"{_servicePrefix}/{objectName}";
            
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fullObjectName);

            await _minioClient.StatObjectAsync(statObjectArgs);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification du fichier {ObjectName}", objectName);
            return false;
        }
    }
}
