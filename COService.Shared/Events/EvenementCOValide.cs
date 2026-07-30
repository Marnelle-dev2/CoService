namespace COService.Shared.Events;

/// <summary>
/// Événement publié lorsqu'un certificat d'origine (CO) est validé définitivement.
/// Destiné à informer les autres microservices.
/// </summary>
public class EvenementCOValide
{
    public Guid IdentifiantCO { get; set; }
    public string NumeroCO { get; set; } = string.Empty;
    public string? NIUExportateur { get; set; }
    public string? NIUPartenaire { get; set; }
    public DateTime DateValidationUtc { get; set; } = DateTime.UtcNow;
}

