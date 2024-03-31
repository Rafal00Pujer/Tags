namespace Tags.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TagSortServiceTypeAttribute : Attribute
{
    public string Name { get; set; }

    public bool IsDefault { get; set; }

    public TagSortServiceTypeAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Name = name;
    }
}
