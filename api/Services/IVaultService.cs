using Passly.DTOs.Request;
using Passly.DTOs.Response;
using Passly.Entities;

namespace Passly.Services;

public interface IVaultService
{
    Task<VaultItemCreatedDto> CreateAsync(VaultItemInputDto dto, Guid userId);
    Task<VaultItemResponseDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<List<VaultItemResponseDto>> GetAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task<VaultItemResponseDto?> UpdateAsync(Guid id, VaultItemInputDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
