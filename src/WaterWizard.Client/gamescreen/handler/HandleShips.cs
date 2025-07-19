// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 167 Zeilen
// - maxk2807: 76 Zeilen
// - Erickk0: 65 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using Raylib_cs;
using WaterWizard.Client.network;
using WaterWizard.Shared.ShipType;
using CellState = WaterWizard.Client.Gamescreen.CellState;
using WaterWizard.Client.gamescreen.ships;


namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles ship-related messages received from the server.
/// </summary>
public class HandleShips
{

    public readonly static Texture2D Ship1 = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship1.png");
    public readonly static Texture2D Ship1Rotated = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship1-Rotated.png");
    public readonly static Texture2D Ship2 = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship2.png");
    public readonly static Texture2D Ship2Rotated = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship2-Rotated.png");
    public readonly static Texture2D Ship3 = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship3.png");
    public readonly static Texture2D Ship3Rotated = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship3-Rotated.png");
    public readonly static Texture2D Ship4 = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship4.png");
    public readonly static Texture2D Ship4Rotated = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship4-Rotated.png");
    public readonly static Texture2D Ship5 = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship5.png");
    public readonly static Texture2D Ship5Rotated = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ships/Ship5-Rotated.png");

    public static Texture2D TextureFromLength(bool rotated, int length)
    {
        return length switch
        {
            1 => rotated ? Ship1Rotated : Ship1,
            2 => rotated ? Ship2Rotated : Ship2,
            3 => rotated ? Ship3Rotated : Ship3,
            4 => rotated ? Ship4Rotated : Ship4,
            5 => rotated ? Ship5Rotated : Ship5,
            _ => throw new Exception($"[Client] Invalid Ship Length: {length}"),
        };
    }

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

        // Determine ship type - largest ship is merchant
        int size = Math.Max(width, height);
        ShipType shipType = size == 5 ? ShipType.Merchant : ShipType.DEFAULT;
        
        playerBoard.putShip(
            new GameShip(
                GameStateManager.Instance.GameScreen,
                pixelX,
                pixelY,
                shipType,
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

    public static void HandleUpdateShipPosition(NetPacketReader reader)
    {
        try
        {
            var board = GameStateManager.Instance.GameScreen.playerBoard!;
            int oldX = reader.GetInt();
            int oldY = reader.GetInt();
            int newX = reader.GetInt();
            int newY = reader.GetInt();
            var ship = board.Ships.Find(ship =>
            {
                return (ship.X - (int)board.Position.X) / board.CellSize == oldX && (ship.Y - (int)board.Position.Y) / board.CellSize == oldY;
            });
            if (ship != null)
            {
                ship.X = (int)board.Position.X + newX * board.CellSize;
                ship.Y = (int)board.Position.Y + newY * board.CellSize;
                board.MoveShip(ship, new(oldX, oldY), new(newX, newY));
            }
            else
            {
                throw new Exception("Ship not found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error in HandleUpdateShipPosition: {ex.Message}");
            Console.WriteLine($"[Client] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Handles the ship healing message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void ShipHeal(NetPacketReader reader)
    {
        bool success = reader.GetBool();
        if (success)
        {
            int X = reader.GetInt();
            int Y = reader.GetInt();
            GameStateManager.Instance.GameScreen.playerBoard!.SetCellState(
                    X,
                    Y,
                    CellState.Ship
                );
            Console.WriteLine($"[Client] Healed At ({X},{Y})");
        }
        Console.WriteLine($"[Client] Could not Heal, Possible mismatch between Client and Server");
    }

    /// <summary>
    /// Updates ship position after toggling fullscreen
    /// </summary>
    /// <param name="screenWidth">New screen width</param>
    /// <param name="screenHeight">New screen height</param>
    internal static void UpdateShipPositionsFullScreen(Vector2 oldBoardPosition, float oldCellSize)
    {
        GameStateManager.Instance.GameScreen.playerBoard!.Ships.ForEach(ship =>
        {
            var board = GameStateManager.Instance.GameScreen.playerBoard!;
            var prevX = (ship.X - oldBoardPosition.X) / oldCellSize;
            var prevY = (ship.Y - oldBoardPosition.Y) / oldCellSize;
            ship.X = (int)(board.Position.X + prevX) * board.CellSize;
            ship.Y = (int)(board.Position.Y + prevY) * board.CellSize;
        });
    }
}
