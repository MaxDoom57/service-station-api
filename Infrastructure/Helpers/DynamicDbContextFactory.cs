using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public class DynamicDbContextFactory : IDynamicDbContextFactory
    {
        private readonly IConfiguration _config;
        private readonly MainDbContext _mainDb;
        private readonly IUserRequestContext _userContext;

        public DynamicDbContextFactory(
            IConfiguration config,
            MainDbContext mainDb,
            IUserRequestContext userContext)
        {
            _config = config;
            _mainDb = mainDb;
            _userContext = userContext;
        }

        public async Task<DynamicDbContext> CreateDbContextAsync()
        {
            Console.WriteLine("Creating DynamicDbContext in DynamicDbContextFactory........");
            Console.WriteLine($"user session cky {_userContext.CompanyKey}");
            Console.WriteLine($"user session pky {_userContext.ProjectKey}");
            // Step 1: Validate
            if (_userContext.CompanyKey <= 0 || _userContext.ProjectKey <= 0) 
                throw new Exception("Invalid session data.");

            Console.WriteLine("Session data valid........");
            // Step 2: Get credentials for this tenant
            var creds = await _mainDb.DbCredentials
                .FirstOrDefaultAsync(x =>
                    x.CKy == _userContext.CompanyKey &&
                    x.PrjKy == _userContext.ProjectKey
                    );
            Console.WriteLine("Got credentials........");

            if (creds == null)
                throw new Exception("Database credentials not found.");

            string connString;

            // Step 3: Choose authentication mode
            bool hasSqlAuth =
                !string.IsNullOrWhiteSpace(creds.DbUser) &&
                !string.IsNullOrWhiteSpace(creds.DbPassword);

            if (hasSqlAuth)
            {
                // SQL Login Authentication
                connString =
                    $"Server={creds.DbServer};" +
                    $"Database={creds.DbName};" +
                    $"User ID={creds.DbUser};" +
                    $"Password={creds.DbPassword};" +
                    "TrustServerCertificate=True;";
            }
            else
            {
                // Windows Authentication
                connString =
                    $"Server={creds.DbServer};" +
                    $"Database={creds.DbName};" +
                    "Trusted_Connection=True;" +
                    "Encrypt=False;" +
                    "TrustServerCertificate=True;";
            }
            Console.WriteLine($"Connection String: {connString}");

            // Step 4: Build dynamic context
            var builder = new DbContextOptionsBuilder<DynamicDbContext>();
            builder.UseSqlServer(connString);

            return new DynamicDbContext(builder.Options);
        }
    }
}
