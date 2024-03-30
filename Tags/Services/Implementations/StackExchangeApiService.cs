using Tags.Models;
using Tags.Models.StackEcchange;
using Tags.Services.Interfaces;

namespace Tags.Services.Implementations;

public class StackExchangeApiService : IStackExchangeApiService
{
    private const string StackOverflowSite = "&site=stackoverflow";

    private readonly HttpClient _httpClient;

    public StackExchangeApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri("https://api.stackexchange.com/2.3/");
    }

    public async Task<StackExchangeResponse<StackExchangeTag>> GetStackOverflowTags(int page = 1, int pageSize = 100, string? sortType = "popular", bool descendingOrder = true)
    {
        var requestUri = BuildGetTagsUri(page, pageSize, sortType, descendingOrder);

        var response = await _httpClient
            .GetFromJsonAsync<StackExchangeResponse<StackExchangeTag>>(requestUri);

        return response;
    }

    private static string BuildGetTagsUri(int page, int pageSize, string? sortType, bool descendingOrder)
    {
        const string filter = "&filter=!*MO(WDa3IXAe5mql";

        var sortString = "";

        if (sortType is not null)
        {
            sortString = "&sort=" + sortType;
        }

        var orderString = "&order=";

        if (descendingOrder)
        {
            orderString += "desc";
        }
        else
        {
            orderString += "asc";
        }

        return $"tags?page={page}&pagesize={pageSize}{orderString}{sortString}{StackOverflowSite}{filter}";
    }
}
