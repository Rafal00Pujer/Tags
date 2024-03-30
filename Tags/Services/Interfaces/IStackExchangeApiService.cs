using Tags.Models;
using Tags.Models.StackEcchange;

namespace Tags.Services.Interfaces;

public interface IStackExchangeApiService
{
    public Task<StackExchangeResponse<StackExchangeTag>> GetStackOverflowTags(int page = 1, int pageSize = 100, string? sortType = "popular", bool descendingOrder = true);
}
