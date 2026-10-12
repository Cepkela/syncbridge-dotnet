
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SyncBridge.Api.Tests;

public class OrdersApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(CustomWebApplicationFactory factory)
    {
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
            totalAmount = 149.99m
        };

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithDuplicateExternalId_ReturnsConflict()
    {
        var request = new
        {
            externalId = "TEST-DUPLICATE-1001",
            customerName = "Test Customer",
            totalAmount = 149.99m
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);
    }


    [Fact]
    public async Task GetOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/9999");

        // Assert
        Assert.Equal(
            System.Net.HttpStatusCode.NotFound,
            response.StatusCode);
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

    private record OrderResponse(
        int Id,
        string ExternalId,
        string CustomerName,
        decimal TotalAmount,
        DateTime CreatedAtUtc);
}
