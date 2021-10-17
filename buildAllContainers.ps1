docker buildx build --platform linux/amd64,linux/arm64 -t zeno15/zeno-torrent-api --push ./Zeno.Torrent.API
docker buildx build --platform linux/amd64,linux/arm64 -t zeno15/zeno-torrent-bff --push ./Zeno.Torrent.Bff
docker buildx build --platform linux/amd64,linux/arm64 -t zeno15/zeno-torrent-web --push ./Zeno.Torrent.Web