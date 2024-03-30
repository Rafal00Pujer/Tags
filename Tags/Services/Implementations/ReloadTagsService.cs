using Microsoft.EntityFrameworkCore;
using Tags.Context;
using Tags.Models;
using Tags.Services.Interfaces;

namespace Tags.Services.Implementations;

public class ReloadTagsService(
    TagsContext _context,
    IStackExchangeApiService _stackExchangeApi)
    : IReloadTagsService
{
    private readonly TagsContext _context = _context;
    private readonly IStackExchangeApiService _stackExchangeApi = _stackExchangeApi;

    public async Task ReloadAsync()
    {
        const int numOfPages = 10;
        const int pageSize = 100;

        await using var transaction = _context.Database.BeginTransaction();

        await _context.Tags.ExecuteDeleteAsync();
        await _context.SaveChangesAsync();

        for (var i = 1; i <= numOfPages; i++)
        {
            var response = await _stackExchangeApi.GetStackOverflowTags(i, pageSize);

            if (response.error_id.HasValue)
            {
                transaction.Rollback();
                throw new Exception();
            }

            if (response.backoff.HasValue)
            {
                transaction.Rollback();
                throw new Exception();
            }

            var tagsModels = response
                .items
                .Select(x =>
                new TagModel
                {
                    Count = x.count,
                    Name = x.name,
                });

            await _context.Tags.AddRangeAsync(tagsModels);
            await _context.SaveChangesAsync();

            if (!response.has_more)
            {
                transaction.Rollback();
                throw new Exception();
            }
        }

        await transaction.CommitAsync();
    }
}
