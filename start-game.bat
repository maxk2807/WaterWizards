#!/bin/bash
dotnet build WaterWizards.sln

:: start dotnet run --project src\WaterWizard.Server\WaterWizard.Server.csproj

:: start client 
 start dotnet run --project src\WaterWizard.Client\WaterWizard.Client.csproj