using Microsoft.EntityFrameworkCore;
using Tags.Context;
using Tags.Services.Interfaces;

namespace Tags.Extensions;

public static class IHostExtensions
{
    public static void MigrateDatabase(this IHost host)
    {
        var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(MigrateDatabase));

        logger.LogInformation("Applying migrations...");

        var context = services.GetRequiredService<TagsContext>();
        context.Database.Migrate();

        logger.LogInformation("Migrations applied.");
    }

    public static async Task ReloadTagsAtStartupAsync(this IHost host, bool force = false)
    {
        var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(ReloadTagsAtStartupAsync));

        logger.LogInformation("Checking whether to download tags...");

        var tagService = services.GetRequiredService<ITagService>();
        var reloadTagsService = services.GetRequiredService<IReloadTagsService>();

        var tagInDb = await tagService.GetTagsAsync(1, 1, null, null);

        if (tagInDb.Count == 1 && !force)
        {
            logger.LogInformation("Tag download skipped.");
            logger.LogTrace("Tag download skipped because {Count} == 1 and {force} == false", tagInDb.Count, force);

            return;
        }

        logger.LogInformation("Downloading tags...");
        logger.LogTrace("Downloading tags because {Count} != 1 or {force} == true", tagInDb.Count, force);

        await reloadTagsService.ReloadAsync();

        logger.LogInformation("Tags downloaded.");
    }
}
