using System.Reflection;
using Tags.Attributes;
using Tags.Services.Interfaces;

namespace Tags.Services.Implementations;

public class TagSortFactory(
    IServiceProvider _services,
    IReadOnlyCollection<string> sortNames,
    string defaultSortName)
    : ITagSortFactory
{
    private readonly IServiceProvider _services = _services;

    public string DefaultSortName { get; private set; } = defaultSortName;

    public IReadOnlyCollection<string> SortNames { get; private set; } = sortNames;

    public ITagSortService GetTagSortService(string sortType) => _services.GetRequiredKeyedService<ITagSortService>(sortType);
}
