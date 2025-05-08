FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY WaterWizards.sln .

COPY src/WaterWizard.Server/WaterWizard.Server.csproj ./src/WaterWizard.Server/
COPY src/WaterWizard.Shared/WaterWizard.Shared.csproj ./src/WaterWizard.Shared/
COPY src/WaterWizard.Client/WaterWizard.Client.csproj ./src/WaterWizard.Client/
COPY src/WaterWizardTests/WaterWizardTests.csproj ./src/WaterWizardTests/

RUN dotnet restore WaterWizards.sln

COPY src/WaterWizard.Server/ ./src/WaterWizard.Server/
COPY src/WaterWizard.Shared/ ./src/WaterWizard.Shared/
COPY src/WaterWizard.Client/ ./src/WaterWizard.Client/
COPY src/WaterWizardTests/ ./src/WaterWizardTests/

RUN dotnet publish src/WaterWizard.Server/WaterWizard.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .


EXPOSE 7777/udp


ENTRYPOINT ["dotnet", "WaterWizard.Server.dll"]