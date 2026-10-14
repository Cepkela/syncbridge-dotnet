
using Microsoft.EntityFrameworkCore;
using SyncBridge.Api.Data;
using SyncBridge.Api.Models;

namespace SyncBridge.Api.Services;

public interface IOrdersService
{
	Task<Order> CreateAsync(string externalId, string customerName, decimal totalAmount, CancellationToken cancellationToken);

	Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken);

	Task<List<Order>> GetAllAsync(int pageSize = 10, int pageNumber = 1, CancellationToken cancellationToken = default);

    Task<List<OrderSyncAttempt>> GetSyncAttemptsAsync(int orderId, CancellationToken cancellationToken);
}

public class OrdersService : IOrdersService
{
	private readonly AppDbContext _dbContext;

	public OrdersService(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<Order> CreateAsync(string externalId, string customerName, decimal totalAmount, CancellationToken cancellationToken)
	{
		var duplicateExists = await _dbContext.Orders
			.AnyAsync(
				order => order.ExternalId == externalId,
				cancellationToken);

		if (duplicateExists)
		{
            var syncAttemptWithDuplicate = new OrderSyncAttempt
            {
                ExternalId = externalId,
                Status = "Duplicate",
                AttemptedAtUtc = DateTime.UtcNow,
                Message = "An order with this ExternalId already exists."
            };

            _dbContext.OrderSyncAttempts.Add(syncAttemptWithDuplicate);

            await _dbContext.SaveChangesAsync(cancellationToken);

            throw new DuplicateOrderException(externalId);
		}

        var order = new Order
        {
            ExternalId = externalId,
            CustomerName = customerName,
            TotalAmount = totalAmount,
            CreatedAtUtc = DateTime.UtcNow
        };

        var syncAttempt = new OrderSyncAttempt
        {
            ExternalId = externalId,
            Status = "Succeeded",
            AttemptedAtUtc = DateTime.UtcNow,
            Message = "Order created successfully.",
            Order = order
        };

        _dbContext.Orders.Add(order);
        _dbContext.OrderSyncAttempts.Add(syncAttempt);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return order;
    }

	public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken)
	{
		return await _dbContext.Orders
			.AsNoTracking()
			.FirstOrDefaultAsync(
				order => order.Id == id,
				cancellationToken);
	}

	public async Task<List<Order>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
	{
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return await _dbContext.Orders
			.AsNoTracking()
			.OrderBy(order => order.Id)
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);
	}

    public async Task<List<OrderSyncAttempt>> GetSyncAttemptsAsync(int orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.OrderSyncAttempts
            .AsNoTracking()
            .Where(attempt => attempt.OrderId == orderId)
            .OrderByDescending(attempt => attempt.AttemptedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
public sealed class DuplicateOrderException(string externalId)
	: Exception($"An order with ExternalId '{externalId}' already exists.");
