using COService.Application.Repositories;
using COService.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace COService.Application.Services;

/// <summary>
/// Service pour la génération des numéros de certificats, abonnements, etc.
/// Le partenaire (chambre de commerce) est identifié par son NIU (Enrôlement), plus de table locale Partenaire.
/// </summary>
public class NumeroGenerationService : INumeroGenerationService
{
    private readonly ICertificatOrigineRepository _certificatRepository;
    private readonly ILogger<NumeroGenerationService> _logger;

    public NumeroGenerationService(
        ICertificatOrigineRepository certificatRepository,
        ILogger<NumeroGenerationService> logger)
    {
        _certificatRepository = certificatRepository;
        _logger = logger;
    }

    public async Task<string> GenererNumeroCertificatAsync(
        string partenaireNIU,
        Guid certificatId,
        string? nomPartenaire = null,
        CancellationToken cancellationToken = default)
    {
        var codeDepartement = await GetCodeDepartementPartenaireInternalAsync(partenaireNIU, nomPartenaire, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Impossible de déterminer le code département pour le partenaire {partenaireNIU}. Sélectionnez une chambre de commerce.");

        // 2. Formater la date actuelle
        var dateFormatee = FormaterDatePourNumero(DateTime.UtcNow);

        // 3. Récupérer le dernier numéro séquentiel pour cette date et ce partenaire
        var dernierNumero = await GetDernierNumeroSequencielAsync(partenaireNIU, DateTime.UtcNow.Date, cancellationToken);

        // 4. Incrémenter
        var nouveauNumero = dernierNumero + 1;

        // 5. Construire le numéro : CO{Numéro}{Date}{CodeDépartement}
        var numeroCertificat = $"CO{nouveauNumero:D6}{dateFormatee}{codeDepartement}";

        _logger.LogInformation(
            "Numéro de certificat généré : {Numero} pour le partenaire {PartenaireNIU}",
            numeroCertificat, partenaireNIU);

        return numeroCertificat;
    }

    public Task<string> GenererNumeroAbonnementAsync(CancellationToken cancellationToken = default)
    {
        var maintenant = DateTime.UtcNow;
        var numero = $"{maintenant:yyyyMMddHHmmss}{GetLettreAleatoire()}";
        
        _logger.LogInformation("Numéro d'abonnement généré : {Numero}", numero);
        return Task.FromResult(numero);
    }

    public async Task<string> GenererNumeroFactureAsync(string partenaireNIU, CancellationToken cancellationToken = default)
    {
        var maintenant = DateTime.UtcNow;
        var codePartenaire = await GetCodeDepartementPartenaireAsync(partenaireNIU, cancellationToken) ?? "XXX";
        var numero = $"FACT{maintenant:yyyyMMdd}{codePartenaire}{maintenant:HHmmss}";
        
        _logger.LogInformation("Numéro de facture généré : {Numero} pour le partenaire {PartenaireNIU}", numero, partenaireNIU);
        return numero;
    }

    public Task<string?> GetCodeDepartementPartenaireAsync(string partenaireNIU, CancellationToken cancellationToken = default)
    {
        var code = ChambresCommerce.ResolveCodeDepartement(partenaireNIU);
        if (code != null)
        {
            return Task.FromResult<string?>(code);
        }

        _logger.LogWarning("Aucun code département connu pour le partenaire {PartenaireNIU}", partenaireNIU);
        return Task.FromResult<string?>(null);
    }

    public Task<string?> GetCodeDepartementPartenaireAsync(
        string partenaireNIU,
        string? nomPartenaire,
        CancellationToken cancellationToken = default)
    {
        var code = ChambresCommerce.ResolveCodeDepartement(partenaireNIU, nomPartenaire);
        if (code != null)
        {
            return Task.FromResult<string?>(code);
        }

        _logger.LogWarning(
            "Aucun code département connu pour le partenaire {PartenaireNIU} ({Nom})",
            partenaireNIU, nomPartenaire);
        return Task.FromResult<string?>(null);
    }

    private Task<string?> GetCodeDepartementPartenaireInternalAsync(
        string partenaireNIU,
        string? nomPartenaire,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(nomPartenaire))
        {
            return GetCodeDepartementPartenaireAsync(partenaireNIU, nomPartenaire, cancellationToken);
        }

        return GetCodeDepartementPartenaireAsync(partenaireNIU, cancellationToken);
    }

    public async Task<int> GetDernierNumeroSequencielAsync(string partenaireNIU, DateTime date, CancellationToken cancellationToken = default)
    {
        // Récupérer tous les certificats du partenaire
        var certificats = await _certificatRepository.GetAllAsync(cancellationToken);
        
        var certificatsPartenaire = certificats
            .Where(c => c.PartenaireNIU == partenaireNIU && c.CertificateNo.StartsWith("CO"))
            .ToList();

        var dateFormatee = FormaterDatePourNumero(date);
        var codeDepartement = await GetCodeDepartementPartenaireAsync(partenaireNIU, cancellationToken);

        if (string.IsNullOrEmpty(codeDepartement))
        {
            return 0;
        }

        // Filtrer les certificats qui correspondent à la date et au code département
        var certificatsDate = certificatsPartenaire
            .Where(c => c.CertificateNo.EndsWith($"{dateFormatee}{codeDepartement}"))
            .ToList();

        if (!certificatsDate.Any())
        {
            return 0;
        }

        // Extraire les numéros séquentiels et trouver le maximum
        var numeros = certificatsDate
            .Select(c => ExtraireNumeroSequenciel(c.CertificateNo))
            .Where(n => n > 0)
            .ToList();

        return numeros.Any() ? numeros.Max() : 0;
    }

    public int ExtraireNumeroSequenciel(string numeroCertificat)
    {
        // Format attendu : CO{Numéro}{Date}{CodeDépartement}
        // Exemple : CO100000241031224PNR
        // Le numéro séquentiel est entre "CO" et la date (6 chiffres)

        if (string.IsNullOrWhiteSpace(numeroCertificat) || !numeroCertificat.StartsWith("CO"))
        {
            return 0;
        }

        try
        {
            // Enlever "CO" au début
            var sansPrefixe = numeroCertificat.Substring(2);
            
            // Le numéro séquentiel fait 6 chiffres (format :D6)
            if (sansPrefixe.Length < 6)
            {
                return 0;
            }

            var numeroStr = sansPrefixe.Substring(0, 6);
            if (int.TryParse(numeroStr, out var numero))
            {
                return numero;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de l'extraction du numéro séquentiel de {Numero}", numeroCertificat);
        }

        return 0;
    }

    public string FormaterDatePourNumero(DateTime date)
    {
        // Format : ddmmyy
        // Exemple : 24/10/2024 → 241024
        return date.ToString("ddMMyy");
    }

    private char GetLettreAleatoire()
    {
        var random = new Random();
        return (char)('A' + random.Next(0, 26));
    }
}
