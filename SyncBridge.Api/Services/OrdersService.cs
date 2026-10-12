
using Microsoft.EntityFrameworkCore;
using SyncBridge.Api.Data;
using SyncBridge.Api.Models;

namespace SyncBridge.Api.Services;

public interface IOrdersService
{
	Task<Order> CreateAsync(string externalId, string customerName, decimal totalAmount, CancellationToken cancellationToken);

	Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken);

	Task<List<Order>> GetAllAsync(int pageSize = 10, int pageNumber = 1, CancellationToken cancellationToken = default);
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
			throw new DuplicateOrderException(externalId);
		}

		var order = new Order
		{
			ExternalId = externalId,
			CustomerName = customerName,
			TotalAmount = totalAmount,
			CreatedAtUtc = DateTime.UtcNow
		};

		_dbContext.Orders.Add(order);

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
}
public sealed class DuplicateOrderException(string externalId)
	: Exception($"An order with ExternalId '{externalId}' already exists.");
