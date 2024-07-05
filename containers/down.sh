#!/bin/bash

if [ -z "$APP_PROJECT" ]; then
  APP_PROJECT="similarity-search"
fi

EXTRA_ARGS=""
if [ "$1" == "ALL" ]; then
    EXTRA_ARGS="--volumes"
fi

docker compose --project-name "$APP_PROJECT" --file docker-compose-cpu.yml down $EXTRA_ARGS
