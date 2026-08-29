using COService.Application.Repositories;
using COService.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace COService.Application.Services;

/// <summary>
/// Génération des numéros CO — format GECO : CO{seq:D6}{ddMMyy}{Dept}
/// Exemple : CO000002290826PNR
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

        var dateFormatee = FormaterDatePourNumero(DateTime.UtcNow);
        var suffixe = $"{dateFormatee}{codeDepartement}";

        var dernierNumero = await GetDernierNumeroSequencielPourSuffixeAsync(suffixe, cancellationToken);
        var candidat = dernierNumero + 1;

        // Garantir l'unicité (évite collision si filtre partenaire/NIU diverge)
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var numero = $"CO{candidat:D6}{suffixe}";
            if (!await _certificatRepository.ExistsAsync(numero, cancellationToken))
            {
                _logger.LogInformation(
                    "Numéro de certificat généré : {Numero} pour le partenaire {PartenaireNIU}",
                    numero, partenaireNIU);
                return numero;
            }

            candidat++;
        }

        throw new InvalidOperationException(
            $"Impossible de générer un numéro de certificat unique pour le suffixe {suffixe}.");
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

    /// <summary>
    /// Compatibilité interface — délègue au calcul par suffixe date+département.
    /// </summary>
    public async Task<int> GetDernierNumeroSequencielAsync(string partenaireNIU, DateTime date, CancellationToken cancellationToken = default)
    {
        var codeDepartement = await GetCodeDepartementPartenaireAsync(partenaireNIU, cancellationToken);
        if (string.IsNullOrEmpty(codeDepartement))
        {
            // Sans nom partenaire, le NIU Organisation (SEG…) ne résout pas le département :
            // on scanne quand même les suffixes connus PNR/OUE pour la date.
            var dateFormatee = FormaterDatePourNumero(date);
            var maxPnr = await GetDernierNumeroSequencielPourSuffixeAsync($"{dateFormatee}{ChambresCommerce.PointeNoire.CodeDepartement}", cancellationToken);
            var maxOue = await GetDernierNumeroSequencielPourSuffixeAsync($"{dateFormatee}{ChambresCommerce.Ouesso.CodeDepartement}", cancellationToken);
            return Math.Max(maxPnr, maxOue);
        }

        return await GetDernierNumeroSequencielPourSuffixeAsync(
            $"{FormaterDatePourNumero(date)}{codeDepartement}",
            cancellationToken);
    }

    /// <summary>
    /// Max séquentiel sur tous les CO dont le numéro se termine par {ddMMyy}{Dept} (indépendant du NIU partenaire).
    /// </summary>
    private async Task<int> GetDernierNumeroSequencielPourSuffixeAsync(string suffixe, CancellationToken cancellationToken)
    {
        var certificats = await _certificatRepository.GetAllAsync(cancellationToken);
        var numeros = certificats
            .Where(c => !string.IsNullOrWhiteSpace(c.CertificateNo)
                        && c.CertificateNo.StartsWith("CO", StringComparison.OrdinalIgnoreCase)
                        && c.CertificateNo.EndsWith(suffixe, StringComparison.OrdinalIgnoreCase))
            .Select(c => ExtraireNumeroSequenciel(c.CertificateNo))
            .Where(n => n > 0)
            .ToList();

        return numeros.Count > 0 ? numeros.Max() : 0;
    }

    public int ExtraireNumeroSequenciel(string numeroCertificat)
    {
        if (string.IsNullOrWhiteSpace(numeroCertificat) || !numeroCertificat.StartsWith("CO", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        try
        {
            var sansPrefixe = numeroCertificat.Substring(2);
            if (sansPrefixe.Length < 6)
            {
                return 0;
            }

            var numeroStr = sansPrefixe.Substring(0, 6);
            return int.TryParse(numeroStr, out var numero) ? numero : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de l'extraction du numéro séquentiel de {Numero}", numeroCertificat);
            return 0;
        }
    }

    public string FormaterDatePourNumero(DateTime date) => date.ToString("ddMMyy");

    private static char GetLettreAleatoire()
    {
        var random = Random.Shared;
        return (char)('A' + random.Next(0, 26));
    }
}
