using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using Tags.Context;
using Tags.Models;
using Tags.Options;
using Tags.Services.Interfaces;

namespace Tags.Services.Implementations;

public class ReloadTagsService(
    TagsContext _context,
    IStackExchangeApiService _stackExchangeApi,
    IOptions<ReloadTagServiceOptions> _options,
    ILogger<ReloadTagsService> _logger)
    : IReloadTagsService
{
    private readonly TagsContext _context = _context;
    private readonly IStackExchangeApiService _stackExchangeApi = _stackExchangeApi;
    private readonly ReloadTagServiceOptions _options = _options.Value;
    private readonly ILogger<ReloadTagsService> _logger = _logger;

    public async Task ReloadAsync()
    {
        await using var transaction = _context.Database.BeginTransaction();

        await _context.Tags.ExecuteDeleteAsync();
        await _context.SaveChangesAsync();

        for (var i = 1; i <= _options.RequestedNumOnPages; i++)
        {
            var response =
                await _stackExchangeApi
                .GetStackOverflowTags(
                    i,
                    _options.RequestedPageSize,
                    _options.RequestedSortType,
                    _options.RequestedDescendingOrder);

            if (response.error_id.HasValue)
            {
                transaction.Rollback();

                throw new WebException("Stack Exchange Api request returned error." +
                    $" Name: {response.error_name}," +
                    $" Message: {response.error_message}");
            }

            if (response.backoff.HasValue)
            {
                if (_options.ThrowOnBackOff)
                {
                    transaction.Rollback();

                    throw new WebException($"Stack Exchange Api requested that we back off: {response.backoff.Value}");
                }

                _logger.LogWarning(
                    "Stack Exchange Api requested that we back off: {time}",
                    response.backoff.Value);

                await Task.Delay(TimeSpan.FromSeconds(response.backoff.Value));
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

            if ((double)response.quota_remaining / response.quota_max <= _options.QuotaRemainingWarningThreshold)
            {
                _logger.LogWarning(
                    "We are approaching quota limit. remaining: {remaining}, max: {max}",
                    response.quota_remaining,
                    response.quota_max);
            }

            if (!response.has_more)
            {
                _logger.LogWarning("We requested more tags than Stack Exchange Api can provide.");

                continue;
            }
        }

        await transaction.CommitAsync();
    }
}
