using Microsoft.AspNetCore.Http;

namespace COService.Application.DTOs;

/// <summary>
/// DTO pour la création d'un certificat avec documents
/// </summary>
public class CreateCertificatWithDocumentsDto
{
    /// <summary>
    /// Numéro du certificat
    /// </summary>
    public string CertificateNo { get; set; } = string.Empty;

    /// <summary>
    /// NIU de l'exportateur
    /// </summary>
    public string? ExportateurNIU { get; set; }

    /// <summary>
    /// NIU du partenaire
    /// </summary>
    public string? PartenaireNIU { get; set; }

    /// <summary>
    /// Code du pays de destination
    /// </summary>
    public string? PaysDestinationCode { get; set; }

    /// <summary>
    /// Fichier de la facture (obligatoire)
    /// </summary>
    public IFormFile FactureFile { get; set; } = null!;

    /// <summary>
    /// Pièces justificatives (optionnelles)
    /// </summary>
    public List<IFormFile>? PiecesJustificatives { get; set; }

    /// <summary>
    /// Autres propriétés du certificat...
    /// </summary>
    public string? Observation { get; set; }
    public string? Navire { get; set; }
    // ... ajouter autres champs nécessaires
}
