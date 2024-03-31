namespace Tags.Services.Interfaces;

public interface ITagSortFactory
{
    public string DefaultSortName { get; }

    public IReadOnlyCollection<string> SortNames { get; }

    public ITagSortService GetTagSortService(string sortType);
}
