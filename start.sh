#!/bin/bash
set -e

echo "=== Downloading cloudflared ==="
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64 \
  -o ./cloudflared
chmod +x ./cloudflared

echo "=== CF_ACCESS_CLIENT_ID is set: ${CF_ACCESS_CLIENT_ID:0:8}... ==="
echo "=== Starting cloudflared tunnel proxy ==="

./cloudflared access tcp \
  --hostname sahirupc.eposmart.com \
  --url localhost:14335 \
  --service-token-id "$CF_ACCESS_CLIENT_ID" \
  --service-token-secret "$CF_ACCESS_CLIENT_SECRET" &

echo "=== Waiting for tunnel to be ready ==="
for i in $(seq 1 20); do
  if ss -tlnp | grep -q 14335; then
    echo "=== Tunnel is ready ==="
    break
  fi
  echo "Waiting... ($i/20)"
  sleep 2
done

if ! ss -tlnp | grep -q 14335; then
  echo "ERROR: cloudflared failed to start. Check logs above."
  exit 1
fi

echo "=== Starting ServiceStationApi ==="
exec dotnet ServiceStationApi.dll --urls "http://0.0.0.0:${PORT:-10000}"