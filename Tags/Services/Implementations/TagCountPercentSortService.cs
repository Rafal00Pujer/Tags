using Tags.Attributes;
using Tags.Models;
using Tags.Services.Interfaces;

namespace Tags.Services.Implementations;

[TagSortServiceType("percent")]
public class TagCountPercentSortService : ITagSortService
{
    public IQueryable<TagModel> Sort(IQueryable<TagModel> query, bool descendingOrder)
    {
        if (descendingOrder)
        {
            return query.OrderByDescending(x => x.CountPercent);
        }

        return query.OrderBy(x => x.CountPercent);
    }
}
