#!/bin/bash
# Build the solution first to avoid simultaneous builds
dotnet build WaterWizards.sln

# Start the server and client
# dotnet run --project src/WaterWizard.Server/WaterWizard.Server.csproj 
dotnet run --project src/WaterWizard.Client/WaterWizard.Client.csproj