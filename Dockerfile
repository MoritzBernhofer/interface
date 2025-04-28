#!/usr/bin/env bash
set -euo pipefail

# --- configuration ---
IMAGE_NAME="myapi:arm64"
CONTAINER_NAME="myapi_container"
DOCKERFILE="Dockerfile"
HOST_PORTS=(5000 5001)

# 1) Build the ARM64 image natively
docker build \
  --pull \
  --file "$DOCKERFILE" \
  --tag "$IMAGE_NAME" \
  .

# 2) Remove any old container
docker rm -f "$CONTAINER_NAME" 2>/dev/null || true

# 3) Map each exposed port and run
PORT_FLAGS=()
for p in "${HOST_PORTS[@]}"; do
  PORT_FLAGS+=( -p "${p}:${p}" )
done

docker run -d --name "$CONTAINER_NAME" "${PORT_FLAGS[@]}" "$IMAGE_NAME"

echo "✅ Built and running container '$CONTAINER_NAME' (image: $IMAGE_NAME)"
echo "   Ports exposed: ${HOST_PORTS[*]}"
