namespace COService.Application.DTOs;

/// <summary>
/// DTO pour un statut de certificat
/// </summary>
public class StatutCertificatDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public DateTime? CreeLe { get; set; }
    public string? CreePar { get; set; }
    public DateTime? ModifierLe { get; set; }
    public string? ModifiePar { get; set; }
}

public class CreerStatutCertificatDto
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
}
