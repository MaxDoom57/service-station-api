#!/bin/bash
set -e

echo "=== Installing cloudflared ==="
curl -fsSL https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64 \
  -o /usr/local/bin/cloudflared
chmod +x /usr/local/bin/cloudflared
echo "cloudflared installed: $(cloudflared --version)"

echo "=== Starting cloudflared tunnel for Customer 2 (db.eposmart.com) ==="
cloudflared access tcp \
  --hostname db.eposmart.com \
  --url localhost:1434 &

echo "=== Waiting for tunnels to be ready ==="
sleep 10

echo "=== Checking tunnels are alive ==="
if ! pgrep -x cloudflared > /dev/null; then
  echo "ERROR: cloudflared failed to start!"
  exit 1
fi

echo "=== Starting ServiceStationApi ==="
exec dotnet ServiceStationApi.dll --urls "http://0.0.0.0:${PORT:-8080}"
