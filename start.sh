#!/bin/bash

# MiniExplorer Quick Start Script
# This script builds and runs the MiniExplorer application

echo "🚀 Starting MiniExplorer..."
echo ""

# Navigate to project directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Build the project
echo "📦 Building project..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo "✅ Build successful!"
echo ""

# Run the application
echo "🎨 Launching MiniExplorer..."
dotnet run --project MiniExplorer.UI/MiniExplorer.UI.csproj --configuration Release
