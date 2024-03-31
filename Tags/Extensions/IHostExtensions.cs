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

        var context = services.GetRequiredService<TagsContext>();
        context.Database.Migrate();
    }

    public static async Task ReloadTagsAsync(this IHost host, bool force = false)
    {
        var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var services = scope.ServiceProvider;

        var tagService = services.GetRequiredService<ITagService>();
        var reloadTagsService = services.GetRequiredService<IReloadTagsService>();

        var tagInDb = await tagService.GetTagsAsync(1, 1, null, null);

        if (tagInDb.Count == 1 && !force)
        {
            return;
        }

        await reloadTagsService.ReloadAsync();
    }
}
