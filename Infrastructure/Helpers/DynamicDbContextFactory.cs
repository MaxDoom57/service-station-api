using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
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
            // Validate session
            if (_userContext.CompanyKey <= 0 || _userContext.ProjectKey <= 0)
                throw new Exception("Invalid session data.");

            var creds = await _mainDb.DbCredentials
                .FirstOrDefaultAsync(x =>
                    x.CKy == _userContext.CompanyKey &&
                    x.PrjKy == _userContext.ProjectKey);

            if (creds == null)
                throw new Exception("Database credentials not found.");

            if (string.IsNullOrWhiteSpace(creds.DbServer) ||
                string.IsNullOrWhiteSpace(creds.DbName))
            {
                throw new Exception("Server or database name missing.");
            }


            string connString;

            bool hasSqlAuth =
                !string.IsNullOrWhiteSpace(creds.DbUser) &&
                !string.IsNullOrWhiteSpace(creds.DbPassword);

            if (hasSqlAuth)
            {
                // SQL Authentication
                connString =
                    $"Server={creds.DbServer};" +
                    $"Database={creds.DbName};" +
                    $"User ID={creds.DbUser};" +
                    $"Password={creds.DbPassword};" +
                    "Encrypt=False;" +
                    "TrustServerCertificate=True;" +
                    "MultipleActiveResultSets=True;" +
                    "Connection Timeout=30;";
            }
            else
            {
                // No username/password provided
                // Build connection string WITHOUT auth fields
                connString =
                    $"Server={creds.DbServer};" +
                    $"Database={creds.DbName};" +
                    "Encrypt=False;" +
                    "TrustServerCertificate=True;" +
                    "MultipleActiveResultSets=True;" +
                    "Connection Timeout=30;";
            }

            var builder = new DbContextOptionsBuilder<DynamicDbContext>();

            builder.UseSqlServer(connString, options =>
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

#if DEBUG
            builder.EnableDetailedErrors();
            builder.EnableSensitiveDataLogging();
#endif

            return new DynamicDbContext(builder.Options);
        }
    }
}