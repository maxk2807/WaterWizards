using LiteNetLib;
using WaterWizard.Client.gamescreen;

namespace WaterWizard.Client.gamescreen.ships;

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
}
