# WaterWizards Game Launcher
# PowerShell Script for Windows

Write-Host "🚀 WaterWizards - Game Launcher" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Build the solution first to avoid simultaneous builds
Write-Host "📦 Building solution..." -ForegroundColor Yellow
dotnet build WaterWizards.sln

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed! Please fix the errors and try again." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""

# Function to cleanup processes on exit
function Cleanup-Processes {
    Write-Host ""
    Write-Host "🛑 Shutting down WaterWizards..." -ForegroundColor Yellow
    
    if ($serverJob) {
        Stop-Job $serverJob -ErrorAction SilentlyContinue
        Remove-Job $serverJob -ErrorAction SilentlyContinue
    }
    
    if ($client1Job) {
        Stop-Job $client1Job -ErrorAction SilentlyContinue
        Remove-Job $client1Job -ErrorAction SilentlyContinue
    }
    
    if ($client2Job) {
        Stop-Job $client2Job -ErrorAction SilentlyContinue
        Remove-Job $client2Job -ErrorAction SilentlyContinue
    }
    
    Write-Host "✅ All processes stopped." -ForegroundColor Green
}

# Set up cleanup on script exit
trap {
    Cleanup-Processes
    break
}

Write-Host "🎮 Starting WaterWizards components..." -ForegroundColor Yellow
Write-Host ""

# Start the server in background
Write-Host "🖥️  Starting server..." -ForegroundColor Green
$serverJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet run --project src/WaterWizard.Server/WaterWizard.Server.csproj
} -Name "WaterWizards-Server"

# Wait a moment for server to start
Start-Sleep -Seconds 2

# Start first client
Write-Host "👤 Starting client 1..." -ForegroundColor Green
$client1Job = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet run --project src/WaterWizard.Client/WaterWizard.Client.csproj
} -Name "WaterWizards-Client1"

# Wait a moment for first client to start
Start-Sleep -Seconds 1

# Start second client
Write-Host "👤 Starting client 2..." -ForegroundColor Green
$client2Job = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet run --project src/WaterWizard.Client/WaterWizard.Client.csproj
} -Name "WaterWizards-Client2"

Write-Host ""
Write-Host "✅ All components started successfully!" -ForegroundColor Green
Write-Host "📋 Job Information:" -ForegroundColor Cyan
Write-Host "   Server: $($serverJob.Id)" -ForegroundColor White
Write-Host "   Client 1: $($client1Job.Id)" -ForegroundColor White
Write-Host "   Client 2: $($client2Job.Id)" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Game is ready! Press Ctrl+C to stop all components." -ForegroundColor Yellow
Write-Host ""

# Wait for user to press Ctrl+C or close the window
try {
    while ($true) {
        Start-Sleep -Seconds 1
        
        # Check if any job has failed
        if ($serverJob.State -eq "Failed" -or $client1Job.State -eq "Failed" -or $client2Job.State -eq "Failed") {
            Write-Host "⚠️  One or more components have stopped unexpectedly." -ForegroundColor Red
            break
        }
    }
}
catch {
    Write-Host ""
    Write-Host "🛑 User requested shutdown." -ForegroundColor Yellow
}
finally {
    Cleanup-Processes
} 