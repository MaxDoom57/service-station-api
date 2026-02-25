using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Helpers
{
    public class DynamicDbContextFactory : IDynamicDbContextFactory
    {
        private readonly IConfiguration _config;
        private readonly MainDbContext _mainDb;
        private readonly IUserRequestContext _userContext;
        private readonly CloudflaredTunnelManager _tunnelManager;

        public DynamicDbContextFactory(
            IConfiguration config,
            MainDbContext mainDb,
            IUserRequestContext userContext,
            CloudflaredTunnelManager tunnelManager)
        {
            _config        = config;
            _mainDb        = mainDb;
            _userContext   = userContext;
            _tunnelManager = tunnelManager;
        }

        public async Task<DynamicDbContext> CreateDbContextAsync()
        {
            if (_userContext.CompanyKey <= 0 || _userContext.ProjectKey <= 0)
                throw new Exception("Invalid session data.");

            var creds = await _mainDb.DbCredentials
                .FirstOrDefaultAsync(x =>
                    x.CKy    == _userContext.CompanyKey &&
                    x.PrjKy  == _userContext.ProjectKey);

            if (creds == null)
                throw new Exception("Database credentials not found.");

            // ------------------------------------------------------------------
            // Connection routing:
            //   • IsCloudDb == true  → direct connection to DbServer
            //                          (cloud-hosted SQL reachable from the API)
            //   • IsCloudDb == false → cloudflared TCP tunnel
            //                          (on-premise DB behind Cloudflare Access)
            // ------------------------------------------------------------------
            string server;

            if (creds.IsCloudDb)
            {
                // Cloud DB — connect directly using DbServer from credentials
                if (string.IsNullOrWhiteSpace(creds.DbServer) ||
                    string.IsNullOrWhiteSpace(creds.DbName))
                    throw new Exception("Server or database name missing.");

                server = creds.DbServer;
                Console.WriteLine($"[DynamicDb] Direct cloud connection → {server}");
            }
            else
            {
                // Local DB — route through cloudflared tunnel
                if (string.IsNullOrWhiteSpace(creds.CfHostname)  ||
                    string.IsNullOrWhiteSpace(creds.CfClientId)   ||
                    string.IsNullOrWhiteSpace(creds.CfClientSecret))
                    throw new Exception(
                        "Cloudflare tunnel credentials (CfHostname, CfClientId, CfClientSecret) " +
                        "are required when IsCloudDb is false.");

                var localPort = await _tunnelManager.GetOrCreateTunnelAsync(
                    hostname:     creds.CfHostname!,
                    clientId:     creds.CfClientId!,
                    clientSecret: creds.CfClientSecret!);

                server = $"localhost,{localPort}";
                Console.WriteLine($"[DynamicDb] Cloudflare tunnel → {creds.CfHostname} via {server}");
            }

            // Build connection string using the resolved server
            string connString;
            if (!string.IsNullOrWhiteSpace(creds.DbUser))
            {
                connString =
                    $"Server={server};"                      +
                    $"Database={creds.DbName};"              +
                    $"User ID={creds.DbUser};"               +
                    $"Password={creds.DbPassword ?? ""};"    +
                    "Encrypt=False;"                         +
                    "TrustServerCertificate=True;"           +
                    "MultipleActiveResultSets=True;"         +
                    "Connection Timeout=60;";
            }
            else
            {
                connString =
                    $"Server={server};"              +
                    $"Database={creds.DbName};"      +
                    "Integrated Security=True;"      +
                    "Encrypt=False;"                 +
                    "TrustServerCertificate=True;"   +
                    "MultipleActiveResultSets=True;" +
                    "Connection Timeout=60;";
            }

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

                // Force open connection immediately to surface auth/network errors early
                await context.Database.OpenConnectionAsync();

                return context;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DynamicDb] CONNECTION FAILED! {ex.Message}");
                throw;
            }
        }
    }
}