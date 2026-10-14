
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
            return BadRequest(BadRequestProblem("ExternalId is required."));
        }

		if (string.IsNullOrWhiteSpace(request.CustomerName))
		{
            return BadRequest(BadRequestProblem("CustomerName is required."));
        }

		if (request.TotalAmount <= 0)
		{
			return BadRequest(BadRequestProblem("TotalAmount must be greater than zero."));
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
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate order",
                Detail = $"An order with ExternalId '{externalId}' already exists."
            });
        }
		catch (DuplicateOrderException)
		{
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate order",
                Detail = $"An order with ExternalId '{externalId}' already exists."
            });
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
            return NotFound(OrderNotFound(id));
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
            return BadRequest(BadRequestProblem("pageNumber must be greater than zero."));
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(BadRequestProblem("pageSize must be between 1 and 100."));
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
            return NotFound(OrderNotFound(id));
        }

        var attempts = await _ordersService.GetSyncAttemptsAsync(id, cancellationToken);

		return Ok(attempts);
	}

    private static ProblemDetails OrderNotFound(int id)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Order not found",
            Detail = $"Order with ID {id} was not found."
        };
    }

	private static ProblemDetails BadRequestProblem(string detail)
	{
		return new ProblemDetails
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Bad request",
			Detail = detail
		};
	}
}

public record CreateOrderRequest(
	string? ExternalId,
	string? CustomerName,
	decimal TotalAmount);
