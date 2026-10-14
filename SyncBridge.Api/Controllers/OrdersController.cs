
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SyncBridge.Api.Models;
using SyncBridge.Api.Services;

namespace SyncBridge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
	private readonly IOrdersService _ordersService;

	public OrdersController(IOrdersService ordersService)
	{
		_ordersService = ordersService;
	}

	[HttpPost]
	public async Task<ActionResult<Order>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.ExternalId))
		{
			return BadRequest("ExternalId is required.");
		}

		if (string.IsNullOrWhiteSpace(request.CustomerName))
		{
			return BadRequest("CustomerName is required.");
		}

		if (request.TotalAmount <= 0)
		{
			return BadRequest("TotalAmount must be greater than zero.");
		}

		var externalId = request.ExternalId.Trim();
		var customerName = request.CustomerName.Trim();

		try
		{
			var order = await _ordersService.CreateAsync(
				externalId,
				customerName,
				request.TotalAmount,
				cancellationToken);

			return CreatedAtAction(
				nameof(GetById),
				new { id = order.Id },
				order);
		}
		catch (DbUpdateException ex)
			when (ex.GetBaseException() is SqlException sqlException
				  && (sqlException.Number == 2601
					  || sqlException.Number == 2627))
		{
			return Conflict(
				$"An order with ExternalId '{externalId}' already exists.");
		}
		catch (DuplicateOrderException)
		{
			return Conflict(
				$"An order with ExternalId '{externalId}' already exists.");
		}
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<Order>> GetById(int id, CancellationToken cancellationToken)
	{
		var order = await _ordersService.GetByIdAsync(
			id,
			cancellationToken);

		if (order is null)
		{
			return NotFound();
		}

		return Ok(order);
	}



    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAllOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            return BadRequest("pageNumber must be greater than zero.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest("pageSize must be between 1 and 100.");
        }

        var orders = await _ordersService.GetAllAsync(
            pageNumber,
            pageSize,
            cancellationToken);

        return Ok(orders);
    }

	[HttpGet("{id:int}/sync-attempts")]
	public async Task<ActionResult<List<OrderSyncAttempt>>> GetSyncAttempts(int id, CancellationToken cancellationToken)
	{
        var order = await _ordersService.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        var attempts = await _ordersService.GetSyncAttemptsAsync(id, cancellationToken);

		return Ok(attempts);
	}
}

public record CreateOrderRequest(
	string? ExternalId,
	string? CustomerName,
	decimal TotalAmount);
