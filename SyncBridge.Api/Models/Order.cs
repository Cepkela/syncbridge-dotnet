namespace SyncBridge.Api.Models;

public class Order
{
    public int Id { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
