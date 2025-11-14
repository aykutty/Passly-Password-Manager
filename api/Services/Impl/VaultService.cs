using Passly.DTOs.Request;
using Passly.DTOs.Response;
using Passly.Entities;
using Passly.Repositories;

namespace Passly.Services.Impl;

public class VaultService : IVaultService
{
    private readonly IVaultRepository _vaultRepository;

    public VaultService(IVaultRepository vaultRepository)
    {
        _vaultRepository = vaultRepository;
    }
    
    public async Task<VaultItemCreatedDto> CreateAsync(VaultItemInputDto dto, Guid userId)
    {
        var entity = new VaultItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
            Category = dto.Category,
            Url = dto.Url,
            EncryptedPayload = dto.EncryptedPayload,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _vaultRepository.AddAsync(entity);

        return new VaultItemCreatedDto
        {
            Id = entity.Id
        };
    }

    public async Task<VaultItemResponseDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entity = await _vaultRepository.GetByIdAsync(id, userId, ct);

        if (entity is null)
            return null;

        return MapToResponseDto(entity);
    }

    public async Task<List<VaultItemResponseDto>> GetAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await _vaultRepository.GetAllByUserAsync(userId, ct);
        
        return items
            .Select(MapToResponseDto)
            .ToList();
    }

    public async Task<VaultItemResponseDto?> UpdateAsync(Guid id, VaultItemInputDto dto, Guid userId)
    {
        var entity = await _vaultRepository.GetByIdAsync(id, userId);

        if (entity is null)
            return null; 
        
        entity.Name = dto.Name;
        entity.Category = dto.Category;
        entity.Url = dto.Url;
        entity.EncryptedPayload = dto.EncryptedPayload;
        entity.UpdatedAt = DateTime.UtcNow;

        await _vaultRepository.UpdateAsync(entity);

        return MapToResponseDto(entity);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        await _vaultRepository.DeleteAsync(id, userId);
    }
    
    private static VaultItemResponseDto MapToResponseDto(VaultItem entity)
    {
        return new VaultItemResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Category = entity.Category,
            Url = entity.Url,
            EncryptedPayload = entity.EncryptedPayload,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}