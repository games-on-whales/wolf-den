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
      # Optional, enables the Compatibility Tools page (Proton-GE, Luxtorpeda, etc.).
      # Wolf Den will auto-create the per-OS subdirectories on startup.
      - /etc/wolf/compatibilitytools.d:/etc/wolf/compatibilitytools.d
```

### Compatibility Tools

Wolf Den can install and remove Steam-style compatibility tools (Proton-GE,
Luxtorpeda, Boxtron, Roberta, etc.) into per-OS directories on the host. Upload
accepts `.tar.gz`, `.tar.xz`, `.tar.bz2`, `.tar`, and `.zip` archives.

The defaults in `appsettings.json` are:

| OS Target | Host Path |
| --- | --- |
| Fedora 43 | `/etc/wolf/compatibilitytools.d/fedora43` |
| Ubuntu    | `/etc/wolf/compatibilitytools.d/ubuntu` |

With `/etc/wolf/compatibilitytools.d` mounted (see the compose example above),
Wolf Den creates both subdirectories for you on first launch.

> [!IMPORTANT]
>
> For Steam to *see* the installed tools, your Steam app's runner container
> must mount the matching OS directory at Steam's
> `compatibilitytools.d` location. Wolf Den cannot add this mount on your
> behalf — it runs in its own container and does not have authority over
> app-runner configuration.
>
> Add a mount to each Steam app via the **Apps → Edit → Advanced Options →
> Mounts** field. For example, for a Fedora 43-based Steam app:
>
> ```
> /etc/wolf/compatibilitytools.d/fedora43:/home/retro/.steam/root/compatibilitytools.d
> ```
>
> (Adjust the container-side path to match your Steam image's user home.)

To override the default targets, set them in `appsettings.json` or via
environment variables, e.g.:

```
WOLF_CompatibilityTools__Targets__0__Name=Fedora 43
WOLF_CompatibilityTools__Targets__0__Path=/etc/wolf/compatibilitytools.d/fedora43
```