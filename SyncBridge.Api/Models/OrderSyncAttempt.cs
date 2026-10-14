
namespace SyncBridge.Api.Models;

public class OrderSyncAttempt
{
    public int Id { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime AttemptedAtUtc { get; set; } = DateTime.UtcNow;

    public string? Message { get; set; }

    public int? OrderId { get; set; }

    public Order? Order { get; set; }
}
