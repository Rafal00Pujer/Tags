using Microsoft.AspNetCore.Mvc;
using Tags.Services.Interfaces;

namespace Tags.Controllers;

[ApiController]
[Route("[controller]")]
public class TagController(
    IReloadTagsService _reloadTagsService)
    : ControllerBase
{
    private readonly IReloadTagsService _reloadTagsService = _reloadTagsService;

    [HttpGet("Reload")]
    public async Task<IActionResult> Reload()
    {
        await _reloadTagsService.ReloadAsync();

        return NoContent();
    }
}
