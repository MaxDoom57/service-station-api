#!/bin/bash
set -e

echo "=== Installing cloudflared ==="
curl -fsSL https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64 \
  -o /usr/local/bin/cloudflared
chmod +x /usr/local/bin/cloudflared
echo "cloudflared version: $(cloudflared --version)"

echo "=== Testing outbound connectivity from Render ==="
curl -v https://db.eposmart.com 2>&1 | head -30

echo "=== Starting cloudflared access tcp ==="
cloudflared access tcp \
  --hostname db.eposmart.com \
  --url localhost:1434 \
  --log-level debug &

CF_PID=$!
sleep 15

echo "=== Testing if localhost:1434 is listening ==="
curl -v telnet://localhost:1434 2>&1 | head -10 || echo "port not open"

if ! kill -0 $CF_PID 2>/dev/null; then
  echo "ERROR: cloudflared died"
  cat /tmp/cf.log 2>/dev/null
  exit 1
fi

echo "=== Starting API ==="
exec dotnet ServiceStationApi.dll --urls "http://0.0.0.0:${PORT:-8080}"