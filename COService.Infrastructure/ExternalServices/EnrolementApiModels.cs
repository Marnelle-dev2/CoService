using System.Text.Json.Serialization;

namespace COService.Infrastructure.ExternalServices;

internal sealed class EnrolementPagedResponse<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = [];

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

internal sealed class EnrolementActeurDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("entiteLabel")]
    public string? EntiteLabel { get; set; }

    [JsonPropertyName("niu")]
    public string? Niu { get; set; }

    [JsonPropertyName("raisonSociale")]
    public string? RaisonSociale { get; set; }

    [JsonPropertyName("adresse")]
    public string? Adresse { get; set; }

    [JsonPropertyName("departement")]
    public string? Departement { get; set; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("actif")]
    public bool? Actif { get; set; }
}

internal sealed class EnrolementPrestataireDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("raisonSociale")]
    public string? RaisonSociale { get; set; }

    [JsonPropertyName("niu")]
    public string? Niu { get; set; }

    [JsonPropertyName("adresse")]
    public string? Adresse { get; set; }

    [JsonPropertyName("departement")]
    public string? Departement { get; set; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("actif")]
    public bool? Actif { get; set; }
}
