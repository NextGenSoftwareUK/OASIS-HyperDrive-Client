#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-1.0.0}"
OUTPUT="dist/linux"
PROJECT="src/OasisHyperDriveClient/OasisHyperDriveClient.csproj"

echo "Building OASIS HyperDrive Client $VERSION for Linux..."

dotnet publish "$PROJECT" \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained true \
    --output "$OUTPUT" \
    -p:PublishSingleFile=true \
    -p:Version="$VERSION" \
    -p:IncludeNativeLibrariesForSelfExtract=true

echo "Output: $OUTPUT"
