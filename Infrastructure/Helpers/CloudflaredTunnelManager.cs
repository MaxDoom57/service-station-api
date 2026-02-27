using System.Collections.Concurrent;
using System.Diagnostics;

namespace Infrastructure.Helpers
{
    /// <summary>
    /// Manages a pool of long-running cloudflared TCP access tunnels.
    /// One tunnel is created per unique hostname and reused across all requests
    /// for that hostname, providing connection-level efficiency.
    ///
    /// Each cloudflared child process receives its own isolated environment block
    /// carrying that tenant's service token — credentials never leak across tenants
    /// or into the host application's environment.
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
        ///
        /// Service token credentials are injected via the child process's own
        /// environment block (TUNNEL_SERVICE_TOKEN_ID / TUNNEL_SERVICE_TOKEN_SECRET).
        /// This is the correct mechanism for `cloudflared access tcp` — the
        /// --service-token-id / --service-token-secret CLI flags do not exist
        /// for this subcommand.
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
                    // ✅ No --service-token-id / --service-token-secret flags here.
                    // Those flags don't exist for `access tcp`. Credentials are
                    // passed via the environment block below instead.
                    Arguments = $"access tcp " +
                                $"--hostname {hostname} " +
                                $"--url localhost:{localPort}",
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                };

                // ✅ Per-process environment — isolated to THIS child process only.
                // Setting these here does NOT affect other cloudflared processes
                // or the host .NET application's own environment.
                psi.Environment["TUNNEL_SERVICE_TOKEN_ID"]     = clientId;
                psi.Environment["TUNNEL_SERVICE_TOKEN_SECRET"] = clientSecret;

                var tcs     = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var process = new Process { StartInfo = psi };

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is null) return;
                    Console.WriteLine($"[cloudflared:{hostname}] {e.Data}");
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is null) return;
                    Console.WriteLine($"[cloudflared:{hostname}] ERR: {e.Data}");

                    // ✅ Detect readiness from stderr instead of a fixed Task.Delay.
                    // "Start Websocket listener" is the line cloudflared emits once
                    // the local TCP listener is bound and the tunnel is ready.
                    if (!tcs.Task.IsCompleted &&
                        e.Data.Contains("Start Websocket listener", StringComparison.OrdinalIgnoreCase))
                    {
                        tcs.TrySetResult(true);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for the "ready" signal with a 10-second timeout fallback.
                // If the process dies before signalling, we throw immediately.
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                cts.Token.Register(() => tcs.TrySetCanceled());

                try
                {
                    await tcs.Task;
                }
                catch (OperationCanceledException)
                {
                    if (process.HasExited)
                        throw new Exception(
                            $"cloudflared exited immediately for '{hostname}'. " +
                            $"Exit code: {process.ExitCode}");

                    // Process is still running but didn't signal in time — proceed anyway.
                    Console.WriteLine($"[CloudflaredTunnelManager] Readiness timeout for {hostname}, proceeding...");
                }

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
