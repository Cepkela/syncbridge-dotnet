
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using SyncBridge.Api.Data;
using SyncBridge.Api.Models;

namespace SyncBridge.Api.Tests;

public class OrdersApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrdersApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ReturnsCreated()
    {
        var request = new
        {
            externalId = "TEST-1001",
            customerName = "Test Customer",
            totalAmount = 149.99m
        };

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created, got {(int)response.StatusCode} " +
            $"{response.StatusCode}. Response: {responseBody}");

        var order = await response.Content
            .ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);
        Assert.True(order.Id > 0);
        Assert.Equal("TEST-1001", order.ExternalId);
    }


    [Fact]
    public async Task CreateOrder_WithEmptyExternalId_ReturnsBadRequest()
    {
        var request = new
        {
            externalId = "",
            customerName = "Test Customer",
            totalAmount = 100m
        };

        var response = await _client.PostAsJsonAsync(
            "/api/orders", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        Assert.NotNull(body);
        Assert.Equal(400, body.Status);
        Assert.Equal("Bad request", body.Title);
        Assert.Equal("ExternalId is required.", body.Detail);
    }

    [Fact]
    public async Task CreateOrder_WithDuplicateExternalId_ReturnsConflict()
    {
        var request = new
        {
            externalId = $"DUP-{Guid.NewGuid()}",
            customerName = "Test Customer",
            totalAmount = 100m
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/orders", request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/orders", request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);

        var body = await secondResponse.Content
            .ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        Assert.NotNull(body);
        Assert.Equal(409, body.Status);
        Assert.Equal("Duplicate order", body.Title);
        Assert.Equal(
            $"An order with ExternalId '{request.externalId}' already exists.",
            body.Detail);
    }


    [Fact]
    public async Task GetOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
        "/api/orders/999999/sync-attempts");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        Assert.NotNull(body);
        Assert.Equal(404, body.Status);
        Assert.Equal("Order not found", body.Title);
        Assert.Equal(
            "Order with ID 999999 was not found.",
            body.Detail);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task CreateOrder_WhenAmountIsInvalid_ReturnsBadRequest(
        decimal totalAmount)
    {
        var request = new
        {
            externalId = $"EXT-{Guid.NewGuid()}",
            customerName = "Test Customer",
            totalAmount
        };

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(
            System.Net.HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task GetOrders_ReturnsOkWithOrderList()
    {
        var createRequest = new
        {
            externalId = $"EXT-{Guid.NewGuid()}",
            customerName = "Test Customer",
            totalAmount = 125.50m
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            createRequest);

        createResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/orders");

        Assert.Equal(
            System.Net.HttpStatusCode.OK,
            response.StatusCode);

        var orders = await response.Content
            .ReadFromJsonAsync<List<Models.Order>>();

        Assert.NotNull(orders);
        Assert.Contains(
            orders,
            order => order.ExternalId == createRequest.externalId);
    }


    [Fact]
    public async Task CreateDuplicateOrder_RecordsDuplicateAttempt()
    {
        var externalId = $"EXT-{Guid.NewGuid()}";

        var request = new
        {
            externalId,
            customerName = "Test Customer",
            totalAmount = 125.50m
        };

        // Create the first order.
        var firstResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(
            System.Net.HttpStatusCode.Created,
            firstResponse.StatusCode);

        // Try to create the same order again.
        var duplicateResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(
            System.Net.HttpStatusCode.Conflict,
            duplicateResponse.StatusCode);


        // Verify that the duplicate attempt was recorded.
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var attempts = await dbContext.OrderSyncAttempts
            .AsNoTracking()
            .Where(attempt => attempt.ExternalId == externalId)
            .ToListAsync();

        Assert.Contains(
            attempts,
            attempt => attempt.Status == "Succeeded");

        Assert.Contains(
            attempts,
            attempt => attempt.Status == "Duplicate");

    }

    [Fact]
    public async Task GetSyncAttempts_WhenOrderExists_ReturnsOk()
    {
        var request = new
        {
            externalId = $"SYNC-{Guid.NewGuid()}",
            customerName = "Test Customer",
            totalAmount = 125.50m
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/orders", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var order = await createResponse.Content
            .ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(order);

        var response = await _client.GetAsync(
            $"/api/orders/{order.Id}/sync-attempts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var attempts = await response.Content
            .ReadFromJsonAsync<List<SyncAttemptResponse>>();

        Assert.NotNull(attempts);
        Assert.Contains(attempts, attempt => attempt.Status == "Succeeded");
    }

    [Fact]
    public async Task GetSyncAttempts_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            "/api/orders/999999/sync-attempts");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateOrder_WithEmptyCustomerName_ReturnsBadRequest(
    string customerName)
    {
        var request = new
        {
            externalId = $"CUSTOMER-{Guid.NewGuid()}",
            customerName,
            totalAmount = 100m
        };

        var response = await _client.PostAsJsonAsync(
            "/api/orders", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetOrders_WhenPageSizeIsOutOfRange_ReturnsBadRequest(
    int pageSize)
    {
        var response = await _client.GetAsync(
            $"/api/orders?pageNumber=1&pageSize={pageSize}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetOrders_WhenPageNumberIsInvalid_ReturnsBadRequest(
    int pageNumber)
    {
        var response = await _client.GetAsync(
            $"/api/orders?pageNumber={pageNumber}&pageSize=10");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithMalformedJson_ReturnsBadRequest()
    {
        using var content = new StringContent(
            """{"externalId": "TEST-1", "totalAmount": """,
            System.Text.Encoding.UTF8,
            "application/json");
           
        var response = await _client.PostAsync(
            "/api/orders", content);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
    "application/problem+json",
    response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content
            .ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        Assert.NotNull(body);
        Assert.Equal(400, body.Status);
    }

    private record SyncAttemptResponse(
        int Id,
        string ExternalId,
        string Status,
        DateTime AttemptedAtUtc,
        string? Message);

    private record OrderResponse(
        int Id,
        string ExternalId,
        string CustomerName,
        decimal TotalAmount,
        DateTime CreatedAtUtc);
}
