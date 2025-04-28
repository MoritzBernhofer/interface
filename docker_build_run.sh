#!/usr/bin/env bash
set -euo pipefail

# --- configuration ---
BUILDER_NAME="nativebuilder"
IMAGE_NAME="myapi:amd64"
CONTAINER_NAME="myapi_container"
DOCKERFILE="Dockerfile"
HOST_PORTS=(5000 5001)

# 1) Tear down any old builder
docker buildx rm "$BUILDER_NAME" 2>/dev/null || true

# 2) Create a new Buildx builder using the 'docker' driver (in-daemon, privileged)
docker buildx create \
  --name "$BUILDER_NAME" \
  --driver docker \
  --use

# 3) (Optional) inspect & bootstrap to ensure it's ready
docker buildx inspect "$BUILDER_NAME" --bootstrap

# 4) Build & load the amd64 image
docker buildx build \
  --platform linux/amd64 \
  --load \
  -f "$DOCKERFILE" \
  -t "$IMAGE_NAME" \
  .

# 5) Remove any old container and run the new one
docker rm -f "$CONTAINER_NAME" 2>/dev/null || true

# map each exposed port
PORT_FLAGS=()
for p in "${HOST_PORTS[@]}"; do
  PORT_FLAGS+=( -p "${p}:${p}" )
done

docker run -d --name "$CONTAINER_NAME" "${PORT_FLAGS[@]}" "$IMAGE_NAME"

echo "✅ Built and running container '$CONTAINER_NAME' (image: $IMAGE_NAME)"
echo "   Ports exposed: ${HOST_PORTS[*]}"
