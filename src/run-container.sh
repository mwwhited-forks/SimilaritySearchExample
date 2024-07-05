#!/bin/bash

set -x
set -e

CONTAINER_STORE=$(dirname "$0")
pushd "$CONTAINER_STORE"

if [ -z "$APP_PROJECT" ]; then
    APP_PROJECT=green-onion-dev
fi

EXTRA_ARGS=""
if [ "$1" = "DETACH" ]; then
    EXTRA_ARGS="--detach"
fi

./build.sh

docker compose --project-name "$APP_PROJECT" --file docker-compose.webapi.yml build --no-cache
docker compose --project-name "$APP_PROJECT" --file docker-compose.webapi.yml up $EXTRA_ARGS

popd
set +x
