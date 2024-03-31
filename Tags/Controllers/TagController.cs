using Microsoft.AspNetCore.Mvc;
using Tags.Models;
using Tags.Services.Interfaces;

namespace Tags.Controllers;

[ApiController]
[Route("[controller]")]
public class TagController(
    ITagService _tagService,
    IReloadTagsService _reloadTagsService)
    : ControllerBase
{
    private readonly ITagService _tagService = _tagService;
    private readonly IReloadTagsService _reloadTagsService = _reloadTagsService;

    [HttpGet("Get")]
    public async Task<IActionResult> GetTagsAsync(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sortType,
        [FromQuery] bool? descendingOrder)
    {
        var result = await _tagService
            .GetTagsAsync(page, pageSize, sortType, descendingOrder);

        return Ok(result);
    }

    [HttpGet("Reload")]
    public async Task<IActionResult> ReloadAsync()
    {
        await _reloadTagsService.ReloadAsync();
        return NoContent();
    }
}
