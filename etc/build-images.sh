#!/usr/bin/env sh
set -eu

REGISTRY="${REGISTRY:-}"
REPOSITORY_PREFIX="${REPOSITORY_PREFIX:-chema-vnext}"
TAG="${TAG:-latest}"
PLATFORM="${PLATFORM:-}"
PUSH="false"
DRY_RUN="false"

usage() {
    cat <<'EOF'
Usage: ./etc/build-images.sh [options]

Options:
  -r, --registry <registry>              Registry host, for example registry.example.com or localhost:5000.
  -p, --repository-prefix <prefix>       Image repository prefix. Default: chema-vnext.
  -t, --tag <tag>                        Image tag. Default: latest.
      --platform <platform>              Docker target platform, for example linux/amd64.
      --push                             Push images after successful build.
      --dry-run                          Print docker commands without executing them.
  -h, --help                             Show this help.

Environment variables:
  REGISTRY, REPOSITORY_PREFIX, TAG, PLATFORM

Examples:
  ./etc/build-images.sh
  ./etc/build-images.sh --registry registry.example.com --tag 1.0.0
  ./etc/build-images.sh --platform linux/amd64 --dry-run
EOF
}

while [ "$#" -gt 0 ]; do
    case "$1" in
        -r|--registry)
            REGISTRY="$2"
            shift
            ;;
        -p|--repository-prefix)
            REPOSITORY_PREFIX="$2"
            shift
            ;;
        -t|--tag)
            TAG="$2"
            shift
            ;;
        --platform)
            PLATFORM="$2"
            shift
            ;;
        --push)
            PUSH="true"
            ;;
        --dry-run)
            DRY_RUN="true"
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown argument: $1" >&2
            usage >&2
            exit 1
            ;;
    esac
    shift
done

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPO_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/.." && pwd)

REGISTRY=${REGISTRY%/}
REPOSITORY_PREFIX=${REPOSITORY_PREFIX#/}
REPOSITORY_PREFIX=${REPOSITORY_PREFIX%/}

image_name() {
    image="$REPOSITORY_PREFIX/$1:$TAG"
    if [ -n "$REGISTRY" ]; then
        image="$REGISTRY/$image"
    fi
    printf '%s\n' "$image"
}

run_command() {
    printf '+ %s\n' "$*"
    if [ "$DRY_RUN" != "true" ]; then
        "$@"
    fi
}

build_image() {
    service_name="$1"
    dockerfile="$2"
    image=$(image_name "$service_name")

    set -- docker build -f "$REPO_ROOT/$dockerfile" -t "$image"

    if [ -n "$PLATFORM" ]; then
        set -- "$@" --platform "$PLATFORM"
    fi

    set -- "$@" "$REPO_ROOT"

    run_command "$@"

    if [ "$PUSH" = "true" ]; then
        run_command docker push "$image"
    fi
}

build_image "httpapi-host" "src/CheMa.VNext.HttpApi.Host/Dockerfile"
build_image "gateway" "src/CheMa.VNext.Gateway/Dockerfile"
build_image "blazor" "src/CheMa.VNext.Blazor/Dockerfile"
build_image "background-worker" "src/CheMa.VNext.BackgroundWorker/Dockerfile"
build_image "dbmigrator" "src/CheMa.VNext.DbMigrator/Dockerfile"
