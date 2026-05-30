#!/usr/bin/env bash

set -e

chown ${APP_UID} -R /app
chown ${APP_UID} -R /home/app

if [ -d "/etc/wolf/compatibilitytools.d" ]; then
  chown ${APP_UID} -R /etc/wolf/compatibilitytools.d
fi