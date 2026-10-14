using SyncBridge.Api.Models;
using SyncBridge.Api.Services;

namespace SyncBridge.Api.Tests;

public class ThrowingOrdersService : IOrdersService
{
    public Task<Order> CreateAsync(
        string externalId,
        string customerName,
        decimal totalAmount,
        CancellationToken cancellationToken)
        => throw new InvalidOperationException("Test exception");

    public Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
        => throw new InvalidOperationException("Test exception");

    public Task<List<Order>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
        => throw new InvalidOperationException("Test exception");

    public Task<List<OrderSyncAttempt>> GetSyncAttemptsAsync(
        int orderId,
        CancellationToken cancellationToken)
        => throw new InvalidOperationException("Test exception");
}