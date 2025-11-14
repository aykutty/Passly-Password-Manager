using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Passly.DTOs.Request;
using Passly.Services;

namespace Passly.Controllers;

[Authorize]
[ApiController]
[Route("api/vault")]
public class VaultController : ControllerBase
{
    private readonly IVaultService _vaultService;

    public VaultController(IVaultService vaultService)
    {
        _vaultService = vaultService;
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            throw new UnauthorizedAccessException("User ID missing from token.");

        return Guid.Parse(userId);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateVaultItem(
        [FromBody] VaultItemInputDto dto,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var created = await _vaultService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = GetUserId();
        var items = await _vaultService.GetAllByUserAsync(userId, ct);
        return Ok(items);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var item = await _vaultService.GetByIdAsync(id, userId, ct);
        return Ok(item);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] VaultItemInputDto dto,
        CancellationToken ct)
    {
        var userId = GetUserId();
        await _vaultService.UpdateAsync(id,dto,userId);
        return NoContent();
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        await _vaultService.DeleteAsync(id, userId);
        return NoContent();
    }
}