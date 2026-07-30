namespace COService.Application.DTOs;

/// <summary>
/// DTO représentant un exportateur (lecture live depuis Organisation via Gateway)
/// </summary>
public class ExportateurDto
{
    public Guid Id { get; set; }
    public string CodeExportateur { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? RaisonSociale { get; set; }
    public string? NIU { get; set; }
    public string? RCCM { get; set; }
    public string? CodeActivite { get; set; }
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public bool Actif { get; set; }
    public string? PartenaireNom { get; set; }
    public string? DepartementNom { get; set; }
}
