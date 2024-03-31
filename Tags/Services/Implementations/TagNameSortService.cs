using Tags.Attributes;
using Tags.Models;
using Tags.Services.Interfaces;

namespace Tags.Services.Implementations;

[TagSortServiceType("name")]
public class TagNameSortService : ITagSortService
{
    public IQueryable<TagModel> Sort(IQueryable<TagModel> query, bool descendingOrder)
    {
        if (descendingOrder)
        {
            return query.OrderByDescending(x => x.Name);
        }

        return query.OrderBy(x => x.Name);
    }
}
