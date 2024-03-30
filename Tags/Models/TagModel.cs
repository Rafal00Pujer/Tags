using System.ComponentModel.DataAnnotations;

namespace Tags.Models;

public class TagModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Count { get; set; }

    public float CountPercent { get; set; }
}
