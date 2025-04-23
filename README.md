# WaterWizards

A battleship game built using .NET 8, **[Raylib-cs](https://github.com/raysan5/raylib/wiki)** for the client-side graphics, and **[LiteNetLib](https://revenantx.github.io/LiteNetLib/api/index.html)** for networking.

## Project Structure

The solution ([WaterWizards.sln](WaterWizards.sln)) contains the following projects:

*   **[WaterWizard.Client](src/WaterWizard.Client/WaterWizard.Client.csproj)**: The game client using Raylib-cs for rendering and LiteNetLib for communication. ([Program.cs](WaterWizards/src/WaterWizard.Client/Program.cs))
*   **[WaterWizard.Server](src/WaterWizard.Server/WaterWizard.Server.csproj)**: The game server using LiteNetLib to handle game logic and client connections. ([Program.cs](WaterWizards/src/WaterWizard.Server/Program.cs))
*   **[WaterWizard.Shared](src/WaterWizard.Shared/WaterWizard.Shared.csproj)**: A shared library containing common code (e.g., network messages, game state) used by both the client and server.

## Prerequisites

*   .NET 8 SDK

## Setup and Running

1.  **Clone the repository (if you haven't already):**
    ```sh
    git clone SSH <repository-url>
    cd WaterWizards
    ```

2.  **Build the solution:**
    ```sh
    dotnet build WaterWizards.sln
    ```


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