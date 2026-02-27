#!/bin/bash
set -e

echo "=== Downloading cloudflared ==="
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64 \
  -o ./cloudflared
chmod +x ./cloudflared

echo "=== CF_ACCESS_CLIENT_ID is set: ${CF_ACCESS_CLIENT_ID:0:8}... ==="

echo "=== Starting cloudflared tunnel proxy ==="
CF_ACCESS_CLIENT_ID="$CF_ACCESS_CLIENT_ID" \
CF_ACCESS_CLIENT_SECRET="$CF_ACCESS_CLIENT_SECRET" \
./cloudflared access tcp \
  --hostname hat-01.eposmart.com \
  --url localhost:14333 &

echo "=== Waiting for tunnel to be ready ==="
sleep 5

echo "=== Starting ServiceStationApi ==="
exec dotnet ServiceStationApi.dll --urls "http://0.0.0.0:${PORT:-8080}"
```

The key change is that cloudflared reads the service token from the `CF_ACCESS_CLIENT_ID` and `CF_ACCESS_CLIENT_SECRET` **environment variables** automatically — there are no CLI flags for them. By passing them explicitly as environment variables before the command, you ensure they're picked up correctly.

After deploying, look for this in the logs — if the ID prints correctly it means the env vars are flowing through:
```
=== CF_ACCESS_CLIENT_ID is set: abc12345... ===