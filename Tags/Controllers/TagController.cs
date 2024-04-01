using Microsoft.AspNetCore.Mvc;
using Tags.Services.Interfaces;

namespace Tags.Controllers;

[ApiController]
[Route("[controller]")]
public class TagController(
    ITagService _tagService,
    IReloadTagsService _reloadTagsService,
    ILogger<TagController> _logger)
    : ControllerBase
{
    private readonly ITagService _tagService = _tagService;
    private readonly IReloadTagsService _reloadTagsService = _reloadTagsService;
    private readonly ILogger<TagController> _logger = _logger;

    [HttpGet("Get")]
    public async Task<IActionResult> GetTagsAsync(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sortType,
        [FromQuery] bool? descendingOrder)
    {
        try
        {
            var result = await _tagService
                .GetTagsAsync(page, pageSize, sortType, descendingOrder);

            return Ok(result);
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Invalid request arguments.");

            return BadRequest(e.Message);
        }
    }

    [HttpGet("Reload")]
    public async Task<IActionResult> ReloadAsync()
    {
        await _reloadTagsService.ReloadAsync();
        return NoContent();
    }
}
