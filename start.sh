#!/bin/bash
set -e

echo "=== Installing cloudflared ==="
curl -fsSL https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64 \
  -o /usr/local/bin/cloudflared
chmod +x /usr/local/bin/cloudflared
echo "cloudflared installed: $(cloudflared --version)"

echo "=== Connecting to Cloudflare tunnel ==="
cloudflared tunnel --no-autoupdate run \
  --token "$CF_TUNNEL_TOKEN" &

CF_PID=$!
echo "cloudflared PID: $CF_PID"

echo "=== Waiting 15s for tunnel to establish ==="
sleep 15

if ! kill -0 $CF_PID 2>/dev/null; then
  echo "ERROR: cloudflared tunnel failed to connect"
  exit 1
fi

echo "=== Tunnel established, starting API ==="
exec dotnet ServiceStationApi.dll --urls "http://0.0.0.0:${PORT:-8080}"