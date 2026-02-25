using Infrastructure.Context;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    /// <summary>
    /// Hosted service that pre-warms every cloudflared TCP tunnel at application
    /// startup so that the Cloudflare approval URLs appear in the logs immediately
    /// rather than being created lazily on the first request per client.
    ///
    /// Uses IServiceScopeFactory to resolve the scoped MainDbContext safely from
    /// within this singleton — the scope is created and disposed within StartAsync.
    /// </summary>
    public class TunnelWarmupService : IHostedService
    {
        private readonly CloudflaredTunnelManager _tunnelManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TunnelWarmupService> _logger;

        public TunnelWarmupService(
            CloudflaredTunnelManager tunnelManager,
            IServiceScopeFactory scopeFactory,
            ILogger<TunnelWarmupService> logger)
        {
            _tunnelManager = tunnelManager;
            _scopeFactory  = scopeFactory;
            _logger        = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a short-lived scope just to resolve the scoped MainDbContext
            await using var scope = _scopeFactory.CreateAsyncScope();
            var mainDb = scope.ServiceProvider.GetRequiredService<MainDbContext>();

            // Fetch all CompanyProject rows that require a cloudflared tunnel
            var localClients = await mainDb.DbCredentials
                .Where(x => !x.IsCloudDb
                         && x.CfHostname     != null
                         && x.CfClientId     != null
                         && x.CfClientSecret != null)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "[TunnelWarmup] Pre-warming {Count} cloudflared tunnel(s)...",
                localClients.Count);

            foreach (var client in localClients)
            {
                try
                {
                    _logger.LogInformation(
                        "[TunnelWarmup] Starting tunnel for {Hostname}...",
                        client.CfHostname);

                    var port = await _tunnelManager.GetOrCreateTunnelAsync(
                        hostname:     client.CfHostname!,
                        clientId:     client.CfClientId!,
                        clientSecret: client.CfClientSecret!);

                    _logger.LogInformation(
                        "[TunnelWarmup] Tunnel ready: {Hostname} → localhost:{Port}",
                        client.CfHostname, port);
                }
                catch (Exception ex)
                {
                    // Log and continue — one failed tunnel must not block the rest
                    _logger.LogError(ex,
                        "[TunnelWarmup] Failed to start tunnel for {Hostname}",
                        client.CfHostname);
                }
            }

            _logger.LogInformation("[TunnelWarmup] Tunnel pre-warm complete.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
