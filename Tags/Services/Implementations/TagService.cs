using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Tags.Context;
using Tags.Models;
using Tags.Services.Interfaces;

namespace Tags.Services.Implementations;

public class TagService(
    ITagSortFactory _sortFactory,
    TagsContext _context)
    : ITagService
{
    private readonly ITagSortFactory _sortFactory = _sortFactory;
    private readonly TagsContext _context = _context;

    public async Task<ICollection<TagModel>> GetTagsAsync(int? page, int? pageSize, string? sortType, bool? descendingOrder)
    {
        ValidateAndApplyDefaultArguments(ref page, ref pageSize, ref sortType, ref descendingOrder);

        var sortService = _sortFactory.GetTagSortService(sortType);

        var tags = sortService
             .Sort(_context.Tags, descendingOrder.Value)
             .Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value);

        return await tags.ToListAsync();
    }

    private void ValidateAndApplyDefaultArguments(
        [NotNull] ref int? page,
        [NotNull] ref int? pageSize,
        [NotNull] ref string? sortType,
        [NotNull] ref bool? descendingOrder)
    {
        if (page.HasValue)
        {
            // TODO - check range
        }
        else
        {
            page = 1;
        }

        if (pageSize.HasValue)
        {
            // TODO - check range
        }
        else
        {
            pageSize = 10;
        }

        if (sortType is not null)
        {
            // TODO - check range
        }
        else
        {
            sortType = _sortFactory.DefaultSortName;
        }

        if (!descendingOrder.HasValue)
        {
            descendingOrder = true;
        }
    }
}
