# Wolf Den

A web UI for managing Wolf

## Quickstart

> [!IMPORTANT]
>
> Wolf Den requires access to the Wolf UNIX socket to function correctly.
>
> To enable the Wolf socket:
>
> 1. Set the `WOLF_SOCKET_PATH` environment variable in your Wolf container:
> ```bash
> -e WOLF_SOCKET_PATH=/var/run/wolf/wolf.sock
> ```
> 2. Mount the socket location to the host machine:
> ```bash
> -v /var/run/wolf:/var/run/wolf
> ```
> 3. Ensure the `wolf.sock` file is created inside the container at `/var/run/wolf/wolf.sock`.

```bash
docker run --name wolf-den \
  -p 8080:8080 \
  -e WOLF_SOCKET_PATH=/var/run/wolf/wolf.sock
  -v /var/run/wolf:/var/run/wolf \
  -v /etc/wolf/wolf-den:/app/wolf-den/ \
  ghcr.io/games-on-whales/wolf-den:stable
```

### Example compose

Follow the [Wolf quickstart guide](https://games-on-whales.github.io/wolf/stable/user/quickstart.html) for your
platform; here's an example using the Intel/AMD platform

```yaml 
version: "3"
services:
  wolf:
    image: ghcr.io/games-on-whales/wolf:stable
    environment:
      # Add this line
      - WOLF_SOCKET_PATH=/var/run/wolf/wolf.sock
    volumes:
      - /etc/wolf/:/etc/wolf
      - /var/run/docker.sock:/var/run/docker.sock:rw
      - /dev/:/dev/:rw
      - /run/udev:/run/udev:rw
      # Wolf will create the socket in your host under this path
      - /var/run/wolf:/var/run/wolf
    device_cgroup_rules:
      - 'c 13:* rmw'
    devices:
      - /dev/dri
      - /dev/uinput
      - /dev/uhid
    network_mode: host
    restart: unless-stopped

  wolf-den:
    image: ghcr.io/games-on-whales/wolf-den:stable
    ports:
      - 8080:8080
    environment:
      - WOLF_SOCKET_PATH=/var/run/wolf/wolf.sock
    volumes:
      - /etc/wolf/wolf-den:/app/wolf-den/
      # Mount the Wolf socket from the host
      - /var/run/wolf:/var/run/wolf
      # Optional, enables Icon/Cover picker for Profiles/Apps.
      - /etc/wolf/covers:/etc/wolf/covers
```