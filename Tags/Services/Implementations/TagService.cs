using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using Tags.Context;
using Tags.Models;
using Tags.Options;
using Tags.Services.Interfaces;

namespace Tags.Services.Implementations;

public class TagService(
    ITagSortFactory _sortFactory,
    TagsContext _context,
    IOptions<TagServiceOptions> _options,
    ILogger<TagService> _logger)
    : ITagService
{
    private readonly ITagSortFactory _sortFactory = _sortFactory;
    private readonly TagsContext _context = _context;
    private readonly TagServiceOptions _options = _options.Value;
    private readonly ILogger<TagService> _logger = _logger;

    public async Task<ICollection<TagModel>> GetTagsAsync(int? page, int? pageSize, string? sortType, bool? descendingOrder)
    {
        _logger
            .LogInformation(
            "Processing get tags request with arguments: " +
            "{page}, {pageSize}, {sortType}, {descendingOrder}",
            page, pageSize, sortType, descendingOrder);

        ValidateAndApplyDefaultArguments(ref page, ref pageSize, ref sortType, ref descendingOrder);

        var sortService = _sortFactory.GetTagSortService(sortType);

        var query = sortService
             .Sort(_context.Tags, descendingOrder.Value)
             .Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value);

        var tags = await query.ToListAsync();

        _logger.LogTrace("Request processed with result: {tags}", tags);

        return tags;
    }

    private void ValidateAndApplyDefaultArguments(
        [NotNull] ref int? page,
        [NotNull] ref int? pageSize,
        [NotNull] ref string? sortType,
        [NotNull] ref bool? descendingOrder)
    {
        if (page.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page.Value, nameof(page));
        }
        else
        {
            const int deafultPage = 1;

            _logger.LogDebug("Setting default page: {defaultPage}", deafultPage);

            page = deafultPage;
        }

        if (pageSize.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize.Value, nameof(pageSize));
            ArgumentOutOfRangeException
                .ThrowIfGreaterThan(
                pageSize.Value,
                _options.MaxGetPageSize,
                nameof(pageSize));
        }
        else
        {
            _logger.LogDebug("Setting default page size: {deafultPageSize}", _options.DefaultGetPageSize);

            pageSize = _options.DefaultGetPageSize;
        }

        if (sortType is not null)
        {
            if (!_sortFactory.SortNames.Contains(sortType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sortType),
                    sortType,
                    "Unsupported sort type.");
            }
        }
        else
        {
            _logger.LogDebug("Setting default sort type: {deafultSortType}", _sortFactory.DefaultSortName);

            sortType = _sortFactory.DefaultSortName;
        }

        if (!descendingOrder.HasValue)
        {
            descendingOrder = _options.DefaultDescendingOrderValue;
        }

        _logger
            .LogTrace(
            "Arguments after validation and setting defaults: " +
            "{page}, {pageSize}, {sortType}, {descendingOrder}",
            page, pageSize, sortType, descendingOrder);
    }
}
