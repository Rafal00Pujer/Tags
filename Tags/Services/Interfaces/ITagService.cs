using Tags.Models;

namespace Tags.Services.Interfaces;

public interface ITagService
{
    public Task<ICollection<TagModel>> GetTagsAsync(int? page, int? pageSize, string? sortType, bool? descendingOrder);
}
