using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.gamescreen.ships;
using WaterWizard.Server;
using WaterWizard.Server.ServerGameStates;
using WaterWizard.Shared;

namespace WaterWizard.Server.handler;

/// <summary>
/// Handles ship-related operations for players in the game.
/// </summary>
public class ShipHandler
{
    public static readonly Dictionary<NetPeer, List<PlacedShip>> playerShips = [];

    /// <summary>
    /// Adds a ship to the player's collection of ships.
    /// </summary>
    /// <param name="player">The player to whom the ship will be added.</param>
    /// <param name="ship">The ship to add to the player's collection.</param>
    public static void AddShip(NetPeer player, PlacedShip ship)
    {
        if (!playerShips.ContainsKey(player))
            playerShips[player] = new List<PlacedShip>();
        playerShips[player].Add(ship);
    }

    /// <summary>
    /// Gets all ships for a specific player.
    /// </summary>
    /// <param name="player">The player whose ships to retrieve.</param>
    /// <returns>List of ships for the player.</returns>
    public static List<PlacedShip> GetShips(NetPeer player)
    {
        return playerShips.GetValueOrDefault(player, new List<PlacedShip>());
    }

    /// <summary>
    /// Clears all ships for all players (used when restarting game).
    /// </summary>
    public static void ClearAllShips()
    {
        playerShips.Clear();
    }

    /// <summary>
    /// Prints all ships for all players to the console.
    /// </summary>
    public static void PrintAllShips()
    {
        foreach (var kvp in playerShips)
        {
            Console.WriteLine($"Schiffe von Spieler {kvp.Key}:");
            foreach (var ship in kvp.Value)
            {
                Console.WriteLine(
                    $"  Schiff: X={ship.X}, Y={ship.Y}, W={ship.Width}, H={ship.Height}"
                );
            }
        }
    }

    /// <summary>
    /// Handles the Placement of the ships. Receives the Position of the ship placement
    /// from the Client. Validates placement and sends error messages if invalid.
    /// </summary>
    /// <param name="peer">The <see cref="NetPeer"/> Client sending the Placement Request</param>
    /// <param name="reader"><see cref="NetPacketReader"/> with the Request Data</param>
    public static void HandleShipPlacement(
        NetPeer peer,
        NetPacketReader reader,
        GameState gameState
    )
    {
        int x = reader.GetInt();
        int y = reader.GetInt();
        int width = reader.GetInt();
        int height = reader.GetInt();

        int size = Math.Max(width, height);

        if (gameState.IsPlacementPhase())
        {
            var allowedShips = new Dictionary<int, int>
            {
                { 5, 1 },
                { 4, 2 },
                { 3, 2 },
                { 2, 4 },
                { 1, 5 },
            };

            var playerShipList = GetShips(peer);
            int alreadyPlaced = playerShipList.Count(s => Math.Max(s.Width, s.Height) == size);

            // 1. Zu viele Schiffe dieser Länge?
            if (!allowedShips.ContainsKey(size) || alreadyPlaced >= allowedShips[size])
            {
                NetDataWriter errorWriter = new();
                errorWriter.Put("ShipPlacementError");
                errorWriter.Put(
                    $"Du darfst nur {allowedShips.GetValueOrDefault(size, 0)} Schiffe der Länge {size} platzieren!"
                );
                peer.Send(errorWriter, DeliveryMethod.ReliableOrdered);
                return;
            }

            // 2. Überlappung mit eigenen Schiffen verhindern
            foreach (var ship in playerShipList)
            {
                bool overlap =
                    x < ship.X + ship.Width
                    && x + width > ship.X
                    && y < ship.Y + ship.Height
                    && y + height > ship.Y;
                if (overlap)
                {
                    NetDataWriter errorWriter = new();
                    errorWriter.Put("ShipPlacementError");
                    errorWriter.Put("Schiffe dürfen sich nicht überlappen!");
                    peer.Send(errorWriter, DeliveryMethod.ReliableOrdered);
                    return;
                }
            }
        }

        // 3. Felder auf dem Board prüfen
        int playerIndex = Array.IndexOf(gameState.players, peer);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var cell = gameState.boards[playerIndex][x + i, y + j];
                if (cell.CellState != CellState.Empty)
                {
                    NetDataWriter errorWriter = new();
                    errorWriter.Put("ShipPlacementError");
                    errorWriter.Put("Feld ist bereits belegt!");
                    peer.Send(errorWriter, DeliveryMethod.ReliableOrdered);
                    return;
                }
            }
        }

        // Schiff platzieren
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                gameState.boards[playerIndex][x + i, y + j].CellState = CellState.Ship;
            }
        }
        AddShip(
            peer,
            new PlacedShip
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
            }
        );

        NetDataWriter writer = new();
        writer.Put("ShipPosition");
        writer.Put(x);
        writer.Put(y);
        writer.Put(width);
        writer.Put(height);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public static void SendShipReveal(NetPeer attacker, PlacedShip ship, GameState gameState)
    {
        var writer = new NetDataWriter();
        writer.Put("ShipReveal");
        writer.Put(ship.X);
        writer.Put(ship.Y);
        writer.Put(ship.Width);
        writer.Put(ship.Height);
        writer.Put(false);

        writer.Put(ship.DamagedCells.Count);
        foreach (var (damageX, damageY) in ship.DamagedCells)
        {
            writer.Put(damageX);
            writer.Put(damageY);
        }

        attacker.Send(writer, DeliveryMethod.ReliableOrdered);

        Console.WriteLine(
            $"[Server] Ship reveal sent to attacker: ({ship.X},{ship.Y}) size {ship.Width}x{ship.Height} with {ship.DamagedCells.Count} damage cells"
        );
    }

    /// <summary>
    /// Checks if all ships of a player are destroyed.
    /// </summary>
    /// <param name="player">The player to check</param>
    public static bool AreAllShipsDestroyed(NetPeer player)
    {
        var ships = GetShips(player);
        return ships.Count > 0 && ships.All(ship => ship.IsDestroyed);
    }

    public static void SendHealing(Vector2 healedCoords, bool success, NetPeer caster)
    {
        var writer = new NetDataWriter();
        writer.Put("ShipHeal");
        if (success)
        {
            writer.Put(true); // success
            writer.Put((int)healedCoords.X);
            writer.Put((int)healedCoords.Y);
            Console.WriteLine($"[Server] Sent ShipHeal on {(healedCoords.X, healedCoords.Y)}");
        }
        else
        {
            writer.Put(false); // failed
            Console.WriteLine($"[Server] Sent Failed ShipHeal, Possible Mismatch between Client and Server");
        }
        caster.Send(writer, DeliveryMethod.ReliableOrdered);

    }

    public static void HandlePositionUpdate(Vector2 oldCoords, Vector2 newCoords, NetPeer client)
    {
        Console.WriteLine($"oldCoords: {oldCoords}, newCoords: {newCoords}");
        var writer = new NetDataWriter();
        writer.Put("UpdateShipPosition");
        writer.Put((int)oldCoords.X);
        writer.Put((int)oldCoords.Y);
        writer.Put((int)newCoords.X);
        writer.Put((int)newCoords.Y);
        client.Send(writer, DeliveryMethod.ReliableOrdered);
    }
}
