# Wolf Den

A web UI for managing Wolf

## Quickstart

```bash
docker run --name wolf-den \
  -p 8080:8080 \
  -v /etc/wolf/cfg/:/etc/wolf/cfg/ \
  -v /etc/wolf/wolf-den:/app/WolfLeash/ \
  ghcr.io/games-on-whales/wolf-den:stable
```
