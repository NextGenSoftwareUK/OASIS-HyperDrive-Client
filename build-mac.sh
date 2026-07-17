#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-1.0.0}"
PROJECT="src/OasisHyperDriveClient/OasisHyperDriveClient.csproj"

echo "Building OASIS HyperDrive Client $VERSION for macOS..."

# x64
dotnet publish "$PROJECT" \
    --configuration Release \
    --runtime osx-x64 \
    --self-contained true \
    --output "dist/mac-x64" \
    -p:PublishSingleFile=true \
    -p:Version="$VERSION" \
    -p:IncludeNativeLibrariesForSelfExtract=true

# arm64 (Apple Silicon)
dotnet publish "$PROJECT" \
    --configuration Release \
    --runtime osx-arm64 \
    --self-contained true \
    --output "dist/mac-arm64" \
    -p:PublishSingleFile=true \
    -p:Version="$VERSION" \
    -p:IncludeNativeLibrariesForSelfExtract=true

echo "Outputs: dist/mac-x64  dist/mac-arm64"
