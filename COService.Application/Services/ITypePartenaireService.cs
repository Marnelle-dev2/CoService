using COService.Application.DTOs;

namespace COService.Application.Services;

public interface ITypePartenaireService
{
    Task<TypePartenaireDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TypePartenaireDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<TypePartenaireDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TypePartenaireDto>> GetActifsAsync(CancellationToken cancellationToken = default);
    Task<TypePartenaireDto> CreerAsync(CreerTypePartenaireDto dto, string? utilisateur = null, CancellationToken cancellationToken = default);
}
