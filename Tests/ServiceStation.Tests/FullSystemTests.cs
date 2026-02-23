using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
using Xunit.Abstractions; 
using System.Collections.Generic;
using Application.Interfaces;

namespace ServiceStation.Tests;

public class FullSystemTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public FullSystemTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
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
                // Replace MainDbContext with In-Memory
                services.RemoveAll(typeof(DbContextOptions<MainDbContext>));
                services.AddDbContext<MainDbContext>(options =>
                {
                    options.UseInMemoryDatabase("FullSystemTestDb");
                });

                // Replace IDynamicDbContextFactory with Test Version
                services.RemoveAll(typeof(IDynamicDbContextFactory));
                services.AddSingleton<IDynamicDbContextFactory, TestDynamicDbContextFactory>();
            });
        });
    }

    [Fact]
    public async Task RunAllGetEndpoints_AndReportEmptyData()
    {
        // 1. Setup Client and DB
        var client = _factory.CreateClient();
        var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MainDbContext>();

        // 2. Seed User
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

        // 3. Login
        var loginRequest = new LoginRequestDto
        {
            UserId = "serviceAdmin",
            Password = "11111",
            CompanyKey = 2,
            ProjectKey = 282
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await client.PostAsync("/api/ssms/v0.1/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(loginBody);
        var token = doc.RootElement.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 4. Test Endpoints
        var endpoints = new[]
        {
            "/api/ssms/v0.1/accessLevel",
            "/api/ssms/v0.1/items",
            "/api/ssms/v0.1/vehicletype",
            "/api/ssms/v0.1/CustomerAccount",
            "/api/ssms/v0.1/PaymentTerm",
            "/api/ssms/v0.1/SalesAccount",
            "/api/ssms/v0.1/InvoiceDetails",
            "/api/ssms/v0.1/Customer",
            "/api/ssms/v0.1/bay",
            "/api/ssms/v0.1/baycontrol/available", 
            "/api/ssms/v0.1/reservation",
            "/api/ssms/v0.1/serviceorder",
        };

        foreach (var url in endpoints)
        {
            try 
            {
                var response = await client.GetAsync(url);
                _output.WriteLine($"Testing {url}...");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    bool isEmpty = string.IsNullOrWhiteSpace(content) || content.Trim() == "[]" || content.Trim() == "null";
                    
                    _output.WriteLine($"[SUCCESS] {url}: Status {response.StatusCode}");
                    _output.WriteLine($"[CONTENT] {url}: IsEmpty={isEmpty}");
                    if (isEmpty) {
                         _output.WriteLine($"[EMPTY_DATA_FOUND] {url}");
                    }
                }
                else
                {
                    _output.WriteLine($"[FAILED] {url}: Status {response.StatusCode}");
                    var error = await response.Content.ReadAsStringAsync();
                    _output.WriteLine($"[ERROR_DETAILS] {error}");
                }
            }
            catch(Exception ex)
            {
                _output.WriteLine($"[EXCEPTION] {url}: {ex.Message}");
            }
            _output.WriteLine("--------------------------------------------------");
        }
    }
}
