using Tags.Models;

namespace Tags.Services.Interfaces;

public interface ITagSortService
{
    public IQueryable<TagModel> Sort(IQueryable<TagModel> query, bool descendingOrder);
}
