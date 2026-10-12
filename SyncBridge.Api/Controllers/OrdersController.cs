
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncBridge.Api.Data;
using SyncBridge.Api.Models;

namespace SyncBridge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public OrdersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
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

        var duplicateExists = await _dbContext.Orders
            .AnyAsync(
                order => order.ExternalId == externalId,
                cancellationToken);

        if (duplicateExists)
        {
            return Conflict(
                $"An order with ExternalId '{externalId}' already exists.");
        }

        var order = new Order
        {
            ExternalId = externalId,
            CustomerName = customerName,
            TotalAmount = request.TotalAmount,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            order);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                order => order.Id == id,
                cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}

public record CreateOrderRequest(
    string? ExternalId,
    string? CustomerName,
    decimal TotalAmount);
