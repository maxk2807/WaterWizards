# WaterWizards

A battleship game built using .NET 8, **[Raylib-cs](https://github.com/raysan5/raylib/wiki)** for the client-side graphics, and **[LiteNetLib](https://revenantx.github.io/LiteNetLib/api/index.html)** for networking.

## Project Structure

The solution ([WaterWizards.sln](WaterWizards.sln)) contains the following projects:

*   **[WaterWizard.Client](src/WaterWizard.Client/WaterWizard.Client.csproj)**: The game client using Raylib-cs for rendering and LiteNetLib for communication. ([Program.cs](WaterWizards/src/WaterWizard.Client/Program.cs))
*   **[WaterWizard.Server](src/WaterWizard.Server/WaterWizard.Server.csproj)**: The game server using LiteNetLib to handle game logic and client connections. ([Program.cs](WaterWizards/src/WaterWizard.Server/Program.cs))
*   **[WaterWizard.Shared](src/WaterWizard.Shared/WaterWizard.Shared.csproj)**: A shared library containing common code (e.g., network messages, game state) used by both the client and server.

## Prerequisites

*   .NET 8 SDK

# Setup and Running

1.  **Clone the repository (if you haven't already):**
    ```sh
    git clone SSH <repository-url>
    cd WaterWizards
    ```

2.  **Build the solution:**
    ```sh
    dotnet build WaterWizards.sln
    ```

    #### atm if u want to test local with 1 device, run the  Client first and then start the Server and after that, you could try to join your Localhost
3.  **Run the Client:**
    ```sh
    dotnet run --project src/WaterWizard.Client/WaterWizard.Client.csproj
    ```


4.  **Run the Server:**
    ```sh
    dotnet run --project src/WaterWizard.Server/WaterWizard.Server.csproj
    ```


## Dependencies

*   [Raylib-cs](https://github.com/ChrisDill/Raylib-cs) (Client)
*   [LiteNetLib](https://github.com/RevenantX/LiteNetLib) (Client, Server, Shared)  


## Game Launcher 

### **Unix/MacOS/Linux**
- **`./start-game`** - Universal Script
- **`./start-game.sh`** - Bash Skript for a quick start with 2 clients and with a server

### **Windows**
- **`start-game.bat`** - Batch-Script for CMD
- **`start-game.ps1`** - Powershell-Script


## Usage

### Easiest Method (all Platformen)
```bash
./start-game
```

### Windows Powershell
```powershell
.\start-game.ps1
```

### Windows CMD
```cmd
./start-game.bat
```

### Unix/MacOS/Linux
```bash
./start-game.sh
```


## Ending the Game

### On all platforms
```bash
CTRL + C
```


## Logging
Scripts are creating log files:
- **`server.log`** - Server-Logs
- **`client1.log`** - Client 1 Logs
- **`client2.log`** - Client 2 Logs

## Troubleshooting
### Build-Error
```bash
❌ Build failed! Please fix the errors and try again.
```
**Solution:** Fix the build errors

### Server-Start-Error
```bash
❌ Server failed to start. Check server.log for details.
```
**Solution:** Check the `server.log` for detailed logs.

### Port-Conflicts
**Symptom:** Server or Client can't connect to each other
**Solution:** 
- Check if port 7777 is available
- End other dotnet-process
- Use `netstat -an | grep 7777` (Unix) or `netstat -an | findstr 7777` (Windows)


## Start single Services

### Start only Server (dotnet)
```bash
dotnet run --project src/WaterWizard.Server/WaterWizard.Server.csproj
```

### Start only Server (docker)
- In the server directory run:
```bash
docker compose up --build
```

### Start only a Client
```bash
dotnet run --project src/WaterWizard.Client/WaterWizard.Client.csproj
```


## Plattform-Support

| Plattform | Recommended Script | Alternative |
|-----------|-------------------|-------------|
| macOS | `./start-game` | `./start-game.sh` |
| Linux | `./start-game` | `./start-game.sh` |
| Windows | `start-game.ps1` | `start-game.bat` |