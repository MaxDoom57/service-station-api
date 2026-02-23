using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Infrastructure.Context;
using Shared.Utilities;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ServiceStation.Tests;

public class LoginIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LoginIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var settings = new Dictionary<string, string>
                {
                    {"Jwt:Key", "ThisIsTheKeyInInterwaveSolutions"},
                    {"Jwt:Issuer", "yourapp"},
                    {"Jwt:Audience", "yourapp"}
                };
                config.AddInMemoryCollection(settings!);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<MainDbContext>));
                services.AddDbContext<MainDbContext>(options =>
                {
                    options.UseInMemoryDatabase("LoginTestDb");
                });
            });
        });
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MainDbContext>();

        // Seed User
        var hashedPassword = PasswordHasher.Hash("11111");
        var user = new UsrMas
        {
            UsrKy = 1,
            UsrId = "serviceAdmin",
            Pwd = hashedPassword,
            CKy = 2,
            fGroup = 0,
            fSysAccNm = false,
            SKy = 1
        };

        if (await context.UserMas.FindAsync(1) == null)
        {
            context.UserMas.Add(user);
            await context.SaveChangesAsync();
        }

        var loginRequest = new LoginRequestDto
        {
            UserId = "serviceAdmin",
            Password = "11111",
            CompanyKey = 2,
            ProjectKey = 282
        };

        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/ssms/v0.1/login", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseAndContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", responseAndContent.ToLower());
        
        // Output result for user visibility
        Console.WriteLine(responseAndContent);
    }
}
