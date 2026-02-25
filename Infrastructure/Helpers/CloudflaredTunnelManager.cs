using System.Collections.Concurrent;
using System.Diagnostics;

namespace Infrastructure.Helpers
{
    /// <summary>
    /// Manages a pool of long-running cloudflared TCP access tunnels.
    /// One tunnel is created per unique hostname and reused across all requests
    /// for that hostname, providing connection-level efficiency.
    ///
    /// Lifecycle: register as a Singleton in DI and it will be Disposed on
    /// application shutdown, which kills every child cloudflared process.
    /// </summary>
    public class CloudflaredTunnelManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, (int Port, Process Process)> _tunnels = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private int _nextPort = 15000;
        private bool _disposed;

        /// <summary>
        /// Returns the local port for an existing tunnel, or spins up a new
        /// cloudflared process and waits for it to be ready.
        /// </summary>
        public async Task<int> GetOrCreateTunnelAsync(
            string hostname,
            string clientId,
            string clientSecret)
        {
            // Fast path — tunnel already running
            if (_tunnels.TryGetValue(hostname, out var existing) && !existing.Process.HasExited)
                return existing.Port;

            await _lock.WaitAsync();
            try
            {
                // Double-check after acquiring the lock
                if (_tunnels.TryGetValue(hostname, out var tunnel) && !tunnel.Process.HasExited)
                    return tunnel.Port;

                var localPort = _nextPort++;

                var psi = new ProcessStartInfo
                {
                    FileName  = "cloudflared",
                    Arguments = $"access tcp " +
                                $"--hostname {hostname} " +
                                $"--url localhost:{localPort} " +
                                $"--service-token-id {clientId} " +
                                $"--service-token-secret {clientSecret}",
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                };

                var process = new Process { StartInfo = psi };

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                        Console.WriteLine($"[cloudflared:{hostname}] {e.Data}");
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                        Console.WriteLine($"[cloudflared:{hostname}] ERR: {e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Give the tunnel a few seconds to connect before handing back
                await Task.Delay(3000);

                if (process.HasExited)
                    throw new Exception(
                        $"cloudflared process exited immediately for hostname '{hostname}'. " +
                        $"Exit code: {process.ExitCode}");

                _tunnels[hostname] = (localPort, process);
                Console.WriteLine($"[CloudflaredTunnelManager] Tunnel up: {hostname} → localhost:{localPort}");

                return localPort;
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var (hostname, tunnel) in _tunnels)
            {
                try
                {
                    if (!tunnel.Process.HasExited)
                    {
                        tunnel.Process.Kill(entireProcessTree: true);
                        Console.WriteLine($"[CloudflaredTunnelManager] Killed tunnel: {hostname}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CloudflaredTunnelManager] Error killing tunnel '{hostname}': {ex.Message}");
                }
            }

            _tunnels.Clear();
            _lock.Dispose();
        }
    }
}
