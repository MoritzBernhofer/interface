docker buildx create --use
docker buildx build \
  --platform linux/amd64 \
  --load \
  -f Dockerfile \
  -t myapi:amd64 \
  .
