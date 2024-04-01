using System.ComponentModel.DataAnnotations;

namespace Tags.Options;

public class ReloadTagServiceOptions
{
    public const string Name = "ReloadTagServiceOptions";

    [Required]
    [Range(1, int.MaxValue)]
    public int RequestedNumOnPages { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int RequestedPageSize { get; set; }

    [Required]
    public string RequestedSortType { get; set; } = null!;

    public bool RequestedDescendingOrder { get; set; }

    [Required]
    [Range(0.01, 1.0)]
    public double QuotaRemainingWarningThreshold { get; set; }

    public bool ThrowOnBackOff { get; set; }
}
