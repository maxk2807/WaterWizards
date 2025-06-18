@echo off
chcp 65001 >nul
echo 🚀 WaterWizards - Game Launcher
echo ================================

REM Build the solution first to avoid simultaneous builds
echo 📦 Building solution...
dotnet build WaterWizards.sln

if %ERRORLEVEL% neq 0 (
    echo ❌ Build failed! Please fix the errors and try again.
    pause
    exit /b 1
)

echo ✅ Build successful!
echo.
echo 🎮 Starting WaterWizards components...
echo.

REM Start the server in background
echo 🖥️  Starting server...
start "WaterWizards Server" dotnet run --project src\WaterWizard.Server\WaterWizard.Server.csproj

REM Wait a moment for server to start
timeout /t 2 /nobreak >nul

REM Start first client
echo 👤 Starting client 1...
start "WaterWizards Client 1" dotnet run --project src\WaterWizard.Client\WaterWizard.Client.csproj

REM Wait a moment for first client to start
timeout /t 1 /nobreak >nul

REM Start second client
echo 👤 Starting client 2...
start "WaterWizards Client 2" dotnet run --project src\WaterWizard.Client\WaterWizard.Client.csproj

echo.
echo ✅ All components started successfully!
echo.
echo 🎯 Game is ready! Close this window when you're done playing.
echo.
pause