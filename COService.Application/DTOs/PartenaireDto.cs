namespace COService.Application.DTOs;

/// <summary>
/// DTO représentant un partenaire (Chambre de Commerce), lecture live depuis Organisation via Gateway
/// </summary>
public class PartenaireDto
{
    public Guid Id { get; set; }
    public string CodePartenaire { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public bool Actif { get; set; }
    public string? DepartementNom { get; set; }
}
