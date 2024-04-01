using System.ComponentModel.DataAnnotations;

namespace Tags.Options;

public class TagServiceOptions
{
    public const string Name = "TagServiceOptions";

    [Required]
    [Range(1, int.MaxValue)]
    public int DefaultGetPageSize { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int MaxGetPageSize { get; set; }

    public bool DefaultDescendingOrderValue { get; set; }
}
