using Tags.Models;
using Tags.Services.Interfaces;
using Tags.Attributes;

namespace Tags.Services.Implementations;

[TagSortServiceType("id", IsDefault = true)]
public class TagIdSortService : ITagSortService
{
    public IQueryable<TagModel> Sort(IQueryable<TagModel> query, bool descendingOrder)
    {
        if (descendingOrder)
        {
            return query.OrderByDescending(x => x.Id);
        }

        return query.OrderBy(x => x.Id);
    }
}
