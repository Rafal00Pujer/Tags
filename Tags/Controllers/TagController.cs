using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tags.Models;
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICollection<TagModel>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [SwaggerOperation(
        Summary = "Returns tags",
        Description = "Returns sorted tags from specified page with count equal or less than page size")]
    public async Task<IActionResult> GetTagsAsync(
        [FromQuery]
        [SwaggerParameter("Page must be greater than 0", Required = false)]
        int? page,

        [FromQuery]
        [SwaggerParameter("Page size must be greater than 0", Required = false)]
        int? pageSize,

        [FromQuery]
        [SwaggerParameter("Sort type valid range: id, name, percent", Required = false)]
        string? sortType,

        [FromQuery]
        [SwaggerParameter("Descending order valid range: true, false", Required = false)]
        bool? descendingOrder)
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
