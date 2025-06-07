using LiteNetLib;
using WaterWizard.Client.gamescreen.ships;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles ship-related messages received from the server.
/// </summary>
public class HandleShips
{
    /// <summary>
    /// Handles the ship position message received from the server.
    /// </summary>
    /// <param name="messageType">Message Type that is getting received from the Server</param>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleShipPosition(string messageType, NetPacketReader reader)
    {
        var playerBoard = GameStateManager.Instance.GameScreen!.playerBoard;
        if (playerBoard == null)
        {
            Console.WriteLine("[Client] Fehler: playerBoard ist null bei ShipPosition.");
            return;
        }
        if (reader == null)
        {
            Console.WriteLine("[Client] Fehler: reader ist null bei ShipPosition.");
            return;
        }
        int x = reader.GetInt();
        int y = reader.GetInt();
        int width = reader.GetInt();
        int height = reader.GetInt();
        Console.WriteLine($"[Client] Schiff Platziert auf: {messageType} {x} {y} {width} {height}");
        int pixelX = (int)playerBoard.Position.X + x * playerBoard.CellSize;
        int pixelY = (int)playerBoard.Position.Y + y * playerBoard.CellSize;
        int pixelWidth = width * playerBoard.CellSize;
        int pixelHeight = height * playerBoard.CellSize;
        playerBoard.putShip(
            new GameShip(
                GameStateManager.Instance.GameScreen,
                pixelX,
                pixelY,
                ShipType.DEFAULT,
                pixelWidth,
                pixelHeight
            )
        );
    }

    /// <summary>
    /// Handles the ship sync message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleShipSync(NetPacketReader reader)
    {
        int count = reader.GetInt();
        var playerBoard = GameStateManager.Instance.GameScreen!.playerBoard;
        if (playerBoard == null)
        {
            Console.WriteLine("[Client] Fehler: playerBoard ist null bei ShipSync.");
            return;
        }
        playerBoard.Ships.Clear();
        for (int i = 0; i < count; i++)
        {
            int x = reader.GetInt();
            int y = reader.GetInt();
            int width = reader.GetInt();
            int height = reader.GetInt();
            int pixelX = (int)playerBoard.Position.X + x * playerBoard.CellSize;
            int pixelY = (int)playerBoard.Position.Y + y * playerBoard.CellSize;
            int pixelWidth = width * playerBoard.CellSize;
            int pixelHeight = height * playerBoard.CellSize;
            playerBoard.putShip(
                new GameShip(
                    GameStateManager.Instance.GameScreen,
                    pixelX,
                    pixelY,
                    ShipType.DEFAULT,
                    pixelWidth,
                    pixelHeight
                )
            );
        }
        Console.WriteLine($"[Client] Nach ShipSync sind {playerBoard.Ships.Count} Schiffe auf dem Board.");
        GameStateManager.Instance.SetStateToInGame();
        Console.WriteLine($"[Client] Nach SetStateToInGame sind {playerBoard.Ships.Count} Schiffe auf dem Board.");
    }

    /// <summary>
    /// Handles the ship reveal message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleShipReveal(NetPacketReader reader)
    {
        int x = reader.GetInt();
        int y = reader.GetInt();
        int width = reader.GetInt();
        int height = reader.GetInt();

        var opponentBoard = GameStateManager.Instance.GameScreen!.opponentBoard;
        if (opponentBoard != null)
        {
            int pixelX = (int)opponentBoard.Position.X + x * opponentBoard.CellSize;
            int pixelY = (int)opponentBoard.Position.Y + y * opponentBoard.CellSize;
            int pixelWidth = width * opponentBoard.CellSize;
            int pixelHeight = height * opponentBoard.CellSize;

            opponentBoard.putShip(
                new GameShip(
                    GameStateManager.Instance.GameScreen,
                    pixelX,
                    pixelY,
                    ShipType.DEFAULT,
                    pixelWidth,
                    pixelHeight
                )
            );

            Console.WriteLine(
                $"[Client] Ship revealed: ({x},{y}) size {width}x{height}"
            );
        }
    }

    /// <summary>
    /// Handles the ship placement error message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleShipPlacementError(NetPacketReader reader)
    {
        string errorMsg = reader.GetString();
        Console.WriteLine(
            $"[Client] Fehler beim Platzieren des Schiffs: {errorMsg}"
        );

        // Sperre das Draggen für diese Größe, wenn das Limit erreicht ist
        var match = System.Text.RegularExpressions.Regex.Match(
            errorMsg,
            @"nur (\d+) Schiffe der Länge (\d+)"
        );
        if (match.Success)
        {
            int size = int.Parse(match.Groups[2].Value);
            GameStateManager.Instance.GameScreen?.MarkShipSizeLimitReached(size);
        }
    }
}
