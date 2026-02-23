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
                throw new Exception("Server or database name missing.");

            string connString;
            if (!string.IsNullOrWhiteSpace(creds.DbUser))
            {
                connString =
                    $"Server={creds.DbServer};" +
                    $"Database={creds.DbName};" +
                    $"User ID={creds.DbUser};" +
                    $"Password={creds.DbPassword ?? ""};" +
                    "Encrypt=False;" +
                    "TrustServerCertificate=True;" +
                    "MultipleActiveResultSets=True;" +
                    "Connection Timeout=60;";
            }
            else
            {
                connString =
                    $"Server={creds.DbServer};" +
                    $"Database={creds.DbName};" +
                    "Integrated Security=True;" +
                    "Encrypt=False;" +
                    "TrustServerCertificate=True;" +
                    "MultipleActiveResultSets=True;" +
                    "Connection Timeout=60;";
            }

            // LOG the connection attempt (mask password)
            var displayConn = connString
                .Replace(creds.DbPassword ?? "", "***");
            Console.WriteLine($"[DynamicDb] Attempting connection: {displayConn}");

            try
            {
                var builder = new DbContextOptionsBuilder<DynamicDbContext>();
                builder.UseSqlServer(connString, options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

                var context = new DynamicDbContext(builder.Options);

                // Force open connection immediately to catch errors here
                await context.Database.OpenConnectionAsync();
                Console.WriteLine("[DynamicDb] Connection successful!");

                return context;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DynamicDb] CONNECTION FAILED!");
                Console.WriteLine($"[DynamicDb] Server: {creds.DbServer}");
                Console.WriteLine($"[DynamicDb] Database: {creds.DbName}");
                Console.WriteLine($"[DynamicDb] Error: {ex.Message}");
                Console.WriteLine($"[DynamicDb] Inner: {ex.InnerException?.Message}");
                throw;
            }
        }

        //        public async Task<DynamicDbContext> CreateDbContextAsync()
        //        {
        //            // Validate session
        //            if (_userContext.CompanyKey <= 0 || _userContext.ProjectKey <= 0)
        //                throw new Exception("Invalid session data.");

        //            var creds = await _mainDb.DbCredentials
        //                .FirstOrDefaultAsync(x =>
        //                    x.CKy == _userContext.CompanyKey &&
        //                    x.PrjKy == _userContext.ProjectKey);

        //            if (creds == null)
        //                throw new Exception("Database credentials not found.");

        //            if (string.IsNullOrWhiteSpace(creds.DbServer) ||
        //                string.IsNullOrWhiteSpace(creds.DbName))
        //            {
        //                throw new Exception("Server or database name missing.");
        //            }


        //            string connString;

        //            if (!string.IsNullOrWhiteSpace(creds.DbUser))
        //            {
        //                // SQL Authentication
        //                connString =
        //                    $"Server={creds.DbServer};" +
        //                    $"Database={creds.DbName};" +
        //                    $"User ID={creds.DbUser};" +
        //                    $"Password={creds.DbPassword ?? ""};" +
        //                    "Encrypt=False;" +
        //                    "TrustServerCertificate=True;" +
        //                    "MultipleActiveResultSets=True;" +
        //                    "Connection Timeout=30;";
        //            }
        //            else
        //            {
        //                // Integrated Security (Windows Authentication)
        //                connString =
        //                    $"Server={creds.DbServer};" +
        //                    $"Database={creds.DbName};" +
        //                    "Integrated Security=True;" +
        //                    "Encrypt=False;" +
        //                    "TrustServerCertificate=True;" +
        //                    "MultipleActiveResultSets=True;" +
        //                    "Connection Timeout=30;";
        //            }

        //            var builder = new DbContextOptionsBuilder<DynamicDbContext>();

        //            builder.UseSqlServer(connString, options =>
        //            {
        //                options.EnableRetryOnFailure(
        //                    maxRetryCount: 3,
        //                    maxRetryDelay: TimeSpan.FromSeconds(5),
        //                    errorNumbersToAdd: null);
        //            });

        //#if DEBUG
        //            builder.EnableDetailedErrors();
        //            builder.EnableSensitiveDataLogging();
        //#endif

        //            return new DynamicDbContext(builder.Options);
        //        }
    }
}