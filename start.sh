#!/bin/bash
set -e

echo "=== Installing cloudflared ==="
curl -fsSL https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64 \
  -o /usr/local/bin/cloudflared
chmod +x /usr/local/bin/cloudflared
echo "cloudflared installed: $(cloudflared --version)"

echo "=== Starting cloudflared TCP proxy ==="
cloudflared access tcp \
  --hostname db.eposmart.com \
  --url localhost:1434 \
  --id "$CF_CLIENT_ID" \
  --secret "$CF_CLIENT_SECRET" &

CF_PID=$!
echo "cloudflared PID: $CF_PID"

sleep 10

if ! kill -0 $CF_PID 2>/dev/null; then
  echo "ERROR: cloudflared failed"
  exit 1
fi

echo "=== Tunnel ready, starting API ==="
exec dotnet ServiceStationApi.dll --urls "http://0.0.0.0:${PORT:-8080}"