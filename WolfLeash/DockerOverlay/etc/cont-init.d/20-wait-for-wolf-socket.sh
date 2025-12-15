#!/usr/bin/env bash

set -e

SOCKET=${WOLF_SOCKET_PATH:-/etc/wolf/cfg/wolf.sock}
TIMEOUT=${WOLF_SOCKET_TIMEOUT:-30}
INTERVAL=1

echo -n "Waiting for Wolf socket: $SOCKET "

elapsed=0
while [ ! -S "$SOCKET" ]; do
    if [ $elapsed -ge $TIMEOUT ]; then
        echo ""
        echo "Timeout: Socket did not appear within ${TIMEOUT}s"
        exit 1
    fi
    echo -n "."
    sleep $INTERVAL
    elapsed=$((elapsed + INTERVAL))
done

echo "Socket file exists, testing connectivity..."

# Test if socket is actually working with netcat
if timeout 2 nc -U "$SOCKET" -z 2>/dev/null; then
    echo "Socket is available and responsive"
else
    echo "Socket exists but may not be ready, is Wolf up and running?"
fi