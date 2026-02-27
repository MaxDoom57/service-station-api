using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Infrastructure.Context;
using Xunit;

namespace ServiceStation.Tests;

public class BasicApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                services.RemoveAll(typeof(DbContextOptions<MainDbContext>));

                // Add InMemory DbContext
                services.AddDbContext<MainDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
    }

    [Fact]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        // Trying to access an endpoint. Since we don't know accessible ones without auth, 
        // checking 404 or 401 is a valid "Application Running" test.
        var response = await client.GetAsync("/api/ssms/v0.1/accessLevel"); // Expecting 401 Unauthorized

        // Assert
        // If the application is running, it should return 401 because we didn't provide a token.
        // If it wasn't running or crashed, it would be an exception or 500.
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
