using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.gamescreen.ships;
using WaterWizard.Client.Gamescreen;
using WaterWizard.Client.network;

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
        Console.WriteLine(
            $"[Client] Nach ShipSync sind {playerBoard.Ships.Count} Schiffe auf dem Board."
        );
        GameStateManager.Instance.SetStateToInGame();
        Console.WriteLine(
            $"[Client] Nach SetStateToInGame sind {playerBoard.Ships.Count} Schiffe auf dem Board."
        );
    }

    /// <summary>
    /// Handles the ship reveal message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleShipReveal(NetPacketReader reader)
    {
        try
        {
            int x = reader.GetInt();
            int y = reader.GetInt();
            int width = reader.GetInt();
            int height = reader.GetInt();
            
            int damageCount = reader.GetInt();
            var damagedCells = new HashSet<(int X, int Y)>();
            
            Console.WriteLine($"[Client] HandleShipReveal: Ship at ({x},{y}) size {width}x{height}, damageCount={damageCount}");
            
            for (int i = 0; i < damageCount; i++)
            {
                int damageX = reader.GetInt();
                int damageY = reader.GetInt();
                damagedCells.Add((damageX, damageY));
                Console.WriteLine($"[Client] HandleShipReveal: Damage at ({damageX},{damageY})");
            }

            var targetBoard = GameStateManager.Instance.GameScreen!.opponentBoard;

            if (targetBoard != null)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    for (int dy = 0; dy < height; dy++)
                    {
                        int cellX = x + dx;
                        int cellY = y + dy;
                        
                        if (cellX >= 0 && cellX < targetBoard.GridWidth && cellY >= 0 && cellY < targetBoard.GridHeight)
                        {
                            if (targetBoard._gridStates[cellX, cellY] != CellState.Hit)
                            {
                                targetBoard.SetCellState(cellX, cellY, CellState.Ship);
                            }
                        }
                    }
                }

                Console.WriteLine(
                    $"[Client] Ship revealed on opponent board: ({x},{y}) size {width}x{height} with {damageCount} damage cells - marked cells as Ship state"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error in HandleShipReveal: {ex.Message}");
            Console.WriteLine($"[Client] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Handles the ship placement error message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleShipPlacementError(NetPacketReader reader)
    {
        string errorMsg = reader.GetString();
        Console.WriteLine($"[Client] Fehler beim Platzieren des Schiffs: {errorMsg}");

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

    /// <summary>
    /// Sends a message to the server indicating that the ship placement is ready.
    /// </summary>
    /// <param name="manager">The NetworkManager instance</param>
    public static void SendPlacementReady(NetworkManager manager)
    {
        if (manager.clientService.client != null && manager.clientService.client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("PlacementReady");
            manager.clientService.client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine("[Client] PlacementReady gesendet");
        }
        else
        {
            Console.WriteLine(
                "[Client] Kein Server verbunden, PlacementReady konnte nicht gesendet werden."
            );
        }
    }

    /// <summary>
    /// Sends a ship placement message to the server with the specified coordinates and dimensions.
    /// </summary>
    /// <param name="x">The x-coordinate (in grid cells) where the ship should be placed.</param>
    /// <param name="y">The y-coordinate (in grid cells) where the ship should be placed.</param>
    /// <param name="width">The width (in grid cells) of the ship to be placed.</param>
    /// <param name="height">The height (in grid cells) of the ship to be placed.</param>
    /// <param name="manager">The NetworkManager instance used to send the placement message to the server.</param>
    public static void SendShipPlacement(
        int x,
        int y,
        int width,
        int height,
        NetworkManager manager
    )
    {
        if (manager.clientService.client != null && manager.clientService.client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("PlaceShip");
            writer.Put(x);
            writer.Put(y);
            writer.Put(width);
            writer.Put(height);
            manager.clientService.client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine("[Client] PlaceShip gesendet");
        }
        else
        {
            Console.WriteLine(
                "[Client] Kein Server verbunden, PlaceShip konnte nicht gesendet werden."
            );
        }
    }
}
