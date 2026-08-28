using COService.Application.DTOs;
using COService.Application.Services;

namespace COService.Infrastructure.ExternalServices;

/// <summary>
/// Adaptateur Application ← Refit Référentiel pour les états.
/// </summary>
public class ReferentielEtatsClient : IReferentielEtatsClient
{
    private readonly IReferentielServiceClient _client;

    public ReferentielEtatsClient(IReferentielServiceClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<ReferentielEtatRemoteDto>> GetEtatsAsync(CancellationToken cancellationToken = default)
    {
        var remote = await _client.GetEtatsAsync(cancellationToken);
        return remote.Select(e => new ReferentielEtatRemoteDto
        {
            Id = e.Id,
            Code = e.Code?.ToString(),
            Libelle = e.Libelle,
            Description = e.Description,
            CodeEcran = e.CodeEcran,
            UsageUI = e.UsageUI,
            Domaine = e.Domaine,
            TypeEtat = e.TypeEtat,
            Actif = e.Actif,
            CreerPar = e.CreerPar,
            ModifierPar = e.ModifierPar,
            CreerLe = e.CreerLe,
            ModifierLe = e.ModifierLe
        }).ToList();
    }
}
