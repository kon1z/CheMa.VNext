#!/usr/bin/env sh
set -eu

TARGET="all"
BUILD="false"
DETACHED="false"
LOCAL="false"

while [ "$#" -gt 0 ]; do
    case "$1" in
        environment|services|all)
            TARGET="$1"
            ;;
        --build)
            BUILD="true"
            ;;
        --local)
            LOCAL="true"
            ;;
        -d|--detach|--detached)
            DETACHED="true"
            ;;
        -h|--help)
            echo "Usage: ./etc/start.sh [environment|services|all] [--local] [--build] [-d|--detach]"
            exit 0
            ;;
        *)
            echo "Unknown argument: $1" >&2
            exit 1
            ;;
    esac
    shift
done

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
COMPOSE_DIR="$SCRIPT_DIR/docker-compose"
ENV_FILE="$COMPOSE_DIR/.env"
COMPOSE_FILE="$COMPOSE_DIR/docker-compose.yml"
LOCAL_COMPOSE_FILE="$COMPOSE_DIR/docker-compose.local.yml"
INFRASTRUCTURE_FILE="$COMPOSE_DIR/docker-compose.infrastructure.yml"

if [ ! -f "$ENV_FILE" ]; then
    echo "Environment file not found: $ENV_FILE" >&2
    exit 1
fi

start_environment() {
    docker compose --env-file "$ENV_FILE" -f "$INFRASTRUCTURE_FILE" up -d
}

start_services() {
    SERVICE_COMPOSE_FILE="$COMPOSE_FILE"
    if [ "$LOCAL" = "true" ]; then
        SERVICE_COMPOSE_FILE="$LOCAL_COMPOSE_FILE"
    fi

    set -- --env-file "$ENV_FILE" -f "$SERVICE_COMPOSE_FILE" up

    if [ "$BUILD" = "true" ]; then
        set -- "$@" --build
    fi

    if [ "$DETACHED" = "true" ]; then
        set -- "$@" -d
    fi

    set -- "$@" dbmigrator httpapi-host background-worker gateway blazor

    docker compose "$@"
}

case "$TARGET" in
    environment)
        start_environment
        ;;
    services)
        start_services
        ;;
    all)
        start_environment
        start_services
        ;;
esac
