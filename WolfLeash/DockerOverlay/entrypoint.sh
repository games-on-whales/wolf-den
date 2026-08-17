#!/usr/bin/env bash

set -e

export APP_UID=1000

exit_script() {
    trap - SIGTERM SIGINT SIGQUIT SIGHUP ERR EXIT
    kill -- -$$
    if [ "$(id -u)" = "0" ]; then
        for init_script in /etc/cont-shutdown.d/*.sh ; do
            source "${init_script}"
        done
    fi
    exit 1
}

trap exit_script SIGTERM SIGINT SIGQUIT SIGHUP ERR EXIT

# Execute all container init scripts. Only run this if the container is started as the root user
if [ "$(id -u)" = "0" ]; then
    for init_script in /etc/cont-init.d/*.sh ; do
        source "${init_script}"
    done
fi

WOLF_SOCKET_PATH=${WOLF_SOCKET_PATH:-/var/run/wolf/wolf.sock}
socat UNIX-LISTEN:/app/wolf.sock,mode=600,user=${APP_UID},group=${APP_UID},reuseaddr,fork UNIX-CONNECT:${WOLF_SOCKET_PATH} 2> /dev/null &
export "WOLF_SOCKET_PATH=unix:///app/wolf.sock"

export "DOTNET_EnableDiagnostics=0"
export "DOTNET_DisableMetrics=1"
export "DOTNET_ThreadPool_UnfairSemaphoreSpinLimit=0"

echo "Starting Server"
exec gosu "${APP_UID}" dotnet WolfLeash.dll &
wait $!