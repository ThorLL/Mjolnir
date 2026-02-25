#!/bin/bash
set -e

# Use provided output directory or default to $HOME/NuGetPackages/
OUTPUT_DIR=${1:-"$HOME/NuGetPackages/"}
VERSION=${2:-"1.0.0"}

echo "Starting build and pack process..."
echo "Output directory: $OUTPUT_DIR"

dotnet restore
dotnet build --configuration Release --no-restore

# Ensure output directory exists
mkdir -p "$OUTPUT_DIR"

PACK_ARGS=("--configuration" "Release" "-o" "$OUTPUT_DIR")

if [ -n "$VERSION" ]; then
  echo "Packing with version override: $VERSION"
  PACK_ARGS+=("/p:Version=$VERSION")
fi

dotnet pack ./Mjolnir.Extensions.AspNetCore.Filtering/Mjolnir.Extensions.AspNetCore.Filtering.csproj "${PACK_ARGS[@]}"

echo "Successfully packed all projects to $OUTPUT_DIR"
