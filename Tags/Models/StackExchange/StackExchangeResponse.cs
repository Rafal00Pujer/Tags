namespace Tags.Models.StackExchange;

public class StackExchangeResponse<T>
{
    public int? backoff { get; set; }

    public int? error_id { get; set; }

    public string? error_message { get; set; }

    public string? error_name { get; set; }

    public bool has_more { get; set; }

    public ICollection<T> items { get; set; } = [];

    public int quota_max { get; set; }

    public int quota_remaining { get; set; }
}
