#!/bin/bash
set -e

echo "=== Downloading cloudflared ==="
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64 \
  -o ./cloudflared
chmod +x ./cloudflared

echo "=== Starting cloudflared tunnel proxy ==="
./cloudflared access tcp \
  --hostname ssms-hat-02.eposmart.com \
  --url localhost:14330 \
  --service-token-id "$CF_ACCESS_CLIENT_ID" \
  --service-token-secret "$CF_ACCESS_CLIENT_SECRET" &

echo "=== Waiting for tunnel to be ready ==="
sleep 5

echo "=== Starting ServiceStationApi ==="
exec dotnet ServiceStationApi.dll --urls "http://0.0.0.0:${PORT:-8080}"