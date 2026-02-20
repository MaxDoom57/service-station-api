#!/bin/bash
set -e
echo "=== Starting ServiceStationApi ==="
exec dotnet ServiceStationApi.dll --urls "http://0.0.0.0:${PORT:-8080}"