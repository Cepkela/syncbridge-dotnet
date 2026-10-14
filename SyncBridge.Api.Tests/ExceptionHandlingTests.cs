using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SyncBridge.Api.Services;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SyncBridge.Api.Tests;

public class ExceptionHandlingTests
{
    [Fact]
    public async Task UnexpectedException_ReturnsInternalServerError()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IOrdersService>();
                    services.AddScoped<IOrdersService, ThrowingOrdersService>();
                });
            });

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/orders/1");

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(
            "Test exception",
            body,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

                var problem = await response.Content
                    .ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();

                Assert.NotNull(problem);
                Assert.Equal(500, problem.Status);
    }
}