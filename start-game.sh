#!/bin/bash

echo "🚀 WaterWizards - Game Launcher"
echo "================================"

# Build the solution first to avoid simultaneous builds
echo "📦 Building solution..."
dotnet build WaterWizards.sln

if [ $? -ne 0 ]; then
    echo "❌ Build failed! Please fix the errors and try again."
    exit 1
fi

echo "✅ Build successful!"

# Function to cleanup background processes on exit
cleanup() {
    echo ""
    echo "🛑 Shutting down WaterWizards..."
    kill $SERVER_PID $CLIENT1_PID $CLIENT2_PID 2>/dev/null
    exit 0
}

# Set up signal handlers for cleanup
trap cleanup SIGINT SIGTERM

echo ""
echo "🎮 Starting WaterWizards components..."
echo ""

# Start the server in background
echo "🖥️  Starting server..."
dotnet run --project src/WaterWizard.Server/WaterWizard.Server.csproj &
SERVER_PID=$!

# Wait a moment for server to start
sleep 2

# Start first client
echo "👤 Starting client 1..."
dotnet run --project src/WaterWizard.Client/WaterWizard.Client.csproj &
CLIENT1_PID=$!

# Wait a moment for first client to start
sleep 1

# Start second client
echo "👤 Starting client 2..."
dotnet run --project src/WaterWizard.Client/WaterWizard.Client.csproj &
CLIENT2_PID=$!

echo ""
echo "✅ All components started successfully!"
echo "📋 Process IDs:"
echo "   Server: $SERVER_PID"
echo "   Client 1: $CLIENT1_PID"
echo "   Client 2: $CLIENT2_PID"
echo ""
echo "🎯 Game is ready! Press Ctrl+C to stop all components."
echo ""

# Wait for user to press Ctrl+C
wait