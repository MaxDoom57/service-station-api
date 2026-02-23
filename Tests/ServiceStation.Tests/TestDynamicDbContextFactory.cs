using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ServiceStation.Tests;

public class TestDynamicDbContextFactory : IDynamicDbContextFactory
{
    // Configure options once to ensure all contexts use the same In-Memory DB
    private readonly DbContextOptions<DynamicDbContext> _options;

    public TestDynamicDbContextFactory()
    {
        _options = new DbContextOptionsBuilder<DynamicDbContext>()
            .UseInMemoryDatabase("TestTenantDb")
            .Options;
    }

    public Task<DynamicDbContext> CreateDbContextAsync()
    {
        // Return a new instance each time
        return Task.FromResult(new DynamicDbContext(_options));
    }
}
