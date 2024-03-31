using System.Reflection;
using Tags.Attributes;
using Tags.Services.Implementations;
using Tags.Services.Interfaces;

namespace Tags.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddTagSortFactory(this IServiceCollection services)
    {
        var defaultSortName = string.Empty;
        var sortNames = new List<string>();

        foreach (var type in typeof(TagSortFactory).Assembly.GetTypes())
        {
            var sortType = type.GetCustomAttribute<TagSortServiceTypeAttribute>();

            if (sortType is null)
            {
                continue;
            }

            if (sortType.IsDefault)
            {
                if (!string.IsNullOrWhiteSpace(defaultSortName))
                {
                    throw new Exception($"You cannot add multiple default sort types. Name of an already added type: {defaultSortName}. Name of the next default type: {sortType.Name}");
                }

                defaultSortName = sortType.Name;
            }

            if (sortNames.Contains(sortType.Name))
            {
                throw new Exception($"A sort type with this name: {sortType.Name} has already been added.");
            }

            sortNames.Add(sortType.Name);

            services.AddKeyedTransient(typeof(ITagSortService), sortType.Name, type);
        }

        if (sortNames.Count == 0)
        {
            throw new Exception("No sort types found.");
        }

        if (string.IsNullOrWhiteSpace(defaultSortName))
        {
            throw new Exception("No default sort type added.");
        }

        services.AddSingleton<ITagSortFactory, TagSortFactory>(sp =>
        {
            return new TagSortFactory(sp, sortNames, defaultSortName);
        });

        return services;
    }
}
