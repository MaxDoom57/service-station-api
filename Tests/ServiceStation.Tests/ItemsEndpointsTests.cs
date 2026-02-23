using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Application.DTOs.Items;
using Application.DTOs.ItemBatch;
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
using Application.Interfaces;

namespace ServiceStation.Tests;

public class ItemsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly JsonSerializerOptions _jsonOptions;

    public ItemsEndpointsTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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
                    options.UseInMemoryDatabase("ItemsTestDb_Main");
                });

                services.RemoveAll(typeof(IDynamicDbContextFactory));
                // Use a separate in-memory DB for Items testing to ensure clean state
                services.AddSingleton<IDynamicDbContextFactory, TestItemsDynamicDbContextFactory>();
            });
        });
    }

    // Separate factory for Items tests
    public class TestItemsDynamicDbContextFactory : IDynamicDbContextFactory
    {
        private readonly DbContextOptions<DynamicDbContext> _options;

        public TestItemsDynamicDbContextFactory()
        {
            _options = new DbContextOptionsBuilder<DynamicDbContext>()
                .UseInMemoryDatabase("ItemsTestDb_Tenant")
                .Options;
        }

        public Task<DynamicDbContext> CreateDbContextAsync()
        {
            return Task.FromResult(new DynamicDbContext(_options));
        }
    }

    [Fact]
    public async Task TestAllItemsEndpoints_Flow()
    {
        // 1. Setup Client and DB
        var client = _factory.CreateClient();
        var scope = _factory.Services.CreateScope();
        var mainContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();

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

        if (await mainContext.UserMas.FindAsync(1) == null)
        {
            mainContext.UserMas.Add(user);
            await mainContext.SaveChangesAsync();
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

        _output.WriteLine("Login Successful. Token acquired.");

        // ----------------------------------------------------------------
        // 4. Test POST /items (Add Item)
        // ----------------------------------------------------------------
        var newItem = new AddItemDTO
        {
            itemCode = "ITM-TEST-001",
            itemType = "Stock",
            itemName = "Test Item 1",
            unitKey = 1,
            costPrice = 100,
            salesPrice = 150
            // defaulting other nullable props
        };

        var postResponse = await client.PostAsync("/api/ssms/v0.1/items", 
            new StringContent(JsonSerializer.Serialize(newItem), Encoding.UTF8, "application/json"));
        
        _output.WriteLine($"POST /items Status: {postResponse.StatusCode}");
        if (!postResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(await postResponse.Content.ReadAsStringAsync());
        }
        Assert.Equal(System.Net.HttpStatusCode.Created, postResponse.StatusCode);

        // ----------------------------------------------------------------
        // 5. Test GET /items (Check if item exists)
        // ----------------------------------------------------------------
        var getResponse = await client.GetAsync("/api/ssms/v0.1/items");
        _output.WriteLine($"GET /items Status: {getResponse.StatusCode}");
        Assert.True(getResponse.IsSuccessStatusCode);

        var getBody = await getResponse.Content.ReadAsStringAsync();
        // Since the return type is not a DTO directly or depends on how GetItemsAsync formats it, 
        // we'll parse as JsonElement array
        var itemsList = JsonSerializer.Deserialize<List<JsonElement>>(getBody, _jsonOptions);
        Assert.NotNull(itemsList);
        Assert.NotEmpty(itemsList);
        
        // Find the created item
        var createdItem = itemsList.FirstOrDefault(x => x.GetProperty("itmCd").GetString() == "ITM-TEST-001");
        // Warning: The property names depend on how Dapper/Service returns them. 
        // Typically strict casing unless configure otherwise. Let's assume standard Pascal or Camel.
        // Based on previous ViewFile, ItmMas properties are PascalCase (ItmCd).
        // BUT json serialization usually camelCases unless configured.
        // Let's print properties for debugging if needed.

        if (createdItem.ValueKind == JsonValueKind.Undefined)
        {
             // Try check other property casing or just dump
             _output.WriteLine($"Items returned: {getBody}");
             Assert.Fail("Created item ITM-TEST-001 not found in GET response");
        }
        
        int itemKy = 0;
        // Try getting ItmKy (or itmKy)
        if (createdItem.TryGetProperty("itmKy", out var kyProp)) itemKy = kyProp.GetInt32();
        else if (createdItem.TryGetProperty("ItmKy", out kyProp)) itemKy = kyProp.GetInt32();
        
        Assert.True(itemKy > 0, $"Failed to retrieve valid ItemKy for created item. Item: {createdItem}");
        _output.WriteLine($"Created Item Key: {itemKy}");

        // ----------------------------------------------------------------
        // 6. Test PUT /items (Update Item)
        // ----------------------------------------------------------------
        var updateItem = new UpdateItemDTO
        {
            itemKey = itemKy,
            itemCode = "ITM-TEST-001-UPD",
            itemName = "Test Item 1 Updated",
            // Required or optional? Logic in service might require validation
            partNo = "PN-001" 
        };
        
        var putResponse = await client.PutAsync("/api/ssms/v0.1/items",
             new StringContent(JsonSerializer.Serialize(updateItem), Encoding.UTF8, "application/json"));
        
        _output.WriteLine($"PUT /items Status: {putResponse.StatusCode}");
        Assert.True(putResponse.IsSuccessStatusCode);

        // Verify Update
        getResponse = await client.GetAsync("/api/ssms/v0.1/items");
        var updatedItemsList = JsonSerializer.Deserialize<List<JsonElement>>(await getResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var updatedItem = updatedItemsList.FirstOrDefault(x => x.GetProperty("itmKy").GetInt32() == itemKy); // assume camelCase from serializer default
        // If not found, try Pascal
        if (updatedItem.ValueKind == JsonValueKind.Undefined) 
            updatedItem = updatedItemsList.FirstOrDefault(x => x.GetProperty("ItmKy").GetInt32() == itemKy);

        // Check property ItmNm
        string updatedName = updatedItem.TryGetProperty("itmNm", out var nm) ? nm.GetString() : updatedItem.GetProperty("ItmNm").GetString();
        Assert.Equal("Test Item 1 Updated", updatedName);

        // ----------------------------------------------------------------
        // 7. Test GET /items/active
        // ----------------------------------------------------------------
        var activeResponse = await client.GetAsync("/api/ssms/v0.1/items/active");
        _output.WriteLine($"GET /items/active Status: {activeResponse.StatusCode}");
        Assert.True(activeResponse.IsSuccessStatusCode);
        var activeList = JsonSerializer.Deserialize<List<JsonElement>>(await activeResponse.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.Contains(activeList, x => 
             (x.TryGetProperty("itmKy", out var k) ? k.GetInt32() : x.GetProperty("ItmKy").GetInt32()) == itemKy);


        // ----------------------------------------------------------------
        // 8. Test POST /items/batch (Add Batch)
        // ----------------------------------------------------------------
        var addBatch = new AddItemBatchDTO
        {
            itemKey = itemKy,
            batchNo = "BATCH-001",
            quantity = 50,
            costPrice = 100,
            salePrice = 160,
            expirDt = DateTime.Now.AddYears(1)
        };

        var batchPostResponse = await client.PostAsync("/api/ssms/v0.1/items/batch",
              new StringContent(JsonSerializer.Serialize(addBatch), Encoding.UTF8, "application/json"));
        
        _output.WriteLine($"POST /items/batch Status: {batchPostResponse.StatusCode}");
        Assert.True(batchPostResponse.IsSuccessStatusCode);
        
        // Extract Batch Key
        var batchBody = await batchPostResponse.Content.ReadAsStringAsync();
        // format: { message = "", batchKey = ... } (from previous check of controller)
        // Note: property names in anonymous types use the casing defined in C# (message, batchKey) 
        // BUT api default serializer usually camelCases e.g. batchKey.
        var batchJson = JsonDocument.Parse(batchBody);
        int batchKy = batchJson.RootElement.GetProperty("batchKey").GetInt32();
        _output.WriteLine($"Created Batch Key: {batchKy}");

        // ----------------------------------------------------------------
        // 9. Test GET /items/batch/{itemKey}
        // ----------------------------------------------------------------
        var getBatchResponse = await client.GetAsync($"/api/ssms/v0.1/items/batch/{itemKy}");
        _output.WriteLine($"GET /items/batch/{itemKy} Status: {getBatchResponse.StatusCode}");
        Assert.True(getBatchResponse.IsSuccessStatusCode);
        var batches = JsonSerializer.Deserialize<List<JsonElement>>(await getBatchResponse.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotEmpty(batches);
        // Verify batchKy is in list. 
        // Need to check ItmBatchKy property name
        bool batchFound = batches.Any(b => 
        {
            if (b.TryGetProperty("itmBatchKy", out var bk)) return bk.GetInt32() == batchKy;
            if (b.TryGetProperty("ItmBatchKy", out bk)) return bk.GetInt32() == batchKy;
            return false; 
        });
        Assert.True(batchFound, "Created batch not found in item batches list");

        // ----------------------------------------------------------------
        // 10. Test PUT /items/batch (Update Batch)
        // ----------------------------------------------------------------
        var updateBatch = new UpdateItemBatchDTO
        {
            itemBatchKey = batchKy,
            itemKey = itemKy,
            batchNo = "BATCH-001-UPD",
            quantity = 60
        };

        var putBatchResponse = await client.PutAsync("/api/ssms/v0.1/items/batch",
              new StringContent(JsonSerializer.Serialize(updateBatch), Encoding.UTF8, "application/json"));
        
        _output.WriteLine($"PUT /items/batch Status: {putBatchResponse.StatusCode}");
        Assert.True(putBatchResponse.IsSuccessStatusCode);

        // ----------------------------------------------------------------
        // 11. Test DELETE /items/{itemKey}
        // ----------------------------------------------------------------
        var deleteResponse = await client.DeleteAsync($"/api/ssms/v0.1/items/{itemKy}");
        _output.WriteLine($"DELETE /items/{itemKy} Status: {deleteResponse.StatusCode}");
        Assert.True(deleteResponse.IsSuccessStatusCode);

        // Verify Deletion (In this system, delete often means soft delete / fInAct=1)
        var verifyDeleteResponse = await client.GetAsync("/api/ssms/v0.1/items/active");
        var verifyList = JsonSerializer.Deserialize<List<JsonElement>>(await verifyDeleteResponse.Content.ReadAsStringAsync(), _jsonOptions);
        
        bool stillActive = verifyList.Any(x => 
             (x.TryGetProperty("itmKy", out var k) ? k.GetInt32() : x.GetProperty("ItmKy").GetInt32()) == itemKy);
        
        Assert.False(stillActive, "Item should not be in active items list after deletion");
    }
}
