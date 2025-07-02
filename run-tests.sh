#!/bin/bash

# Skript zum lokalen Ausführen aller Unit-Tests
# Voraussetzung: dotnet SDK 8 installiert

set -e

echo "Restoring dependencies..."
dotnet restore WaterWizards.sln

echo "Building solution..."
dotnet build WaterWizards.sln --no-restore --configuration Release

echo "Running tests..."
dotnet test WaterWizards.sln --no-build --configuration Release --logger "trx;LogFileName=test-results.trx"

echo "Alle Tests wurden ausgeführt." 