// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 176 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new(1, 1);   (erick: 159 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.utility;

/// <summary>
/// Implementation of the Teleport utility card
/// Teleports a ship to a new location while preserving its damage state
/// </summary>
public class TeleportCard : IUtilityCard
{
    public CardVariant Variant => CardVariant.Teleport;

    public Vector2 AreaOfEffect => new(1, 1);

    public bool HasSpecialTargeting => true;

    public bool ExecuteUtility(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        int destinationX = (int)targetCoords.X;
        int destinationY = (int)targetCoords.Y;

        int shipIndex = (int)(targetCoords.X) >> 16;

        destinationX &= 0xFFFF;

        Console.WriteLine(
            $"[Server] Teleport attempting to move ship index {shipIndex} to ({destinationX}, {destinationY})"
        );

        var ships = ShipHandler.GetShips(caster);

        if (shipIndex < 0 || shipIndex >= ships.Count)
        {
            Console.WriteLine($"[Server] Teleport failed - invalid ship index {shipIndex}");
            return false;
        }

        var shipToTeleport = ships[shipIndex];

        int originalX = shipToTeleport.X;
        int originalY = shipToTeleport.Y;

        if (!IsValidShipPosition(gameState, shipToTeleport, destinationX, destinationY, caster))
        {
            Console.WriteLine(
                $"[Server] Teleport failed - invalid destination position ({destinationX}, {destinationY})"
            );
            return false;
        }

        int playerIndex = gameState.GetPlayerIndex(caster);
        for (int dx = 0; dx < shipToTeleport.Width; dx++)
        {
            for (int dy = 0; dy < shipToTeleport.Height; dy++)
            {
                if (
                    originalX + dx >= 0
                    && originalX + dx < GameState.boardWidth
                    && originalY + dy >= 0
                    && originalY + dy < GameState.boardHeight
                )
                {
                    gameState.boards[playerIndex][originalX + dx, originalY + dy].CellState =
                        CellState.Empty;
                }
            }
        }

        shipToTeleport.X = destinationX;
        shipToTeleport.Y = destinationY;

        for (int dx = 0; dx < shipToTeleport.Width; dx++)
        {
            for (int dy = 0; dy < shipToTeleport.Height; dy++)
            {
                gameState.boards[playerIndex][destinationX + dx, destinationY + dy].CellState =
                    CellState.Ship;
            }
        }

        Console.WriteLine(
            $"[Server] Ship {shipIndex} teleported from ({originalX}, {originalY}) to ({destinationX}, {destinationY})"
        );

        SendTeleportNotification(
            caster,
            opponent,
            shipIndex,
            originalX,
            originalY,
            destinationX,
            destinationY
        );

        return true;
    }

    public bool IsValidTarget(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        int destinationX = (int)targetCoords.X & 0xFFFF;
        int destinationY = (int)targetCoords.Y;

        int shipIndex = (int)targetCoords.X >> 16;

        var ships = ShipHandler.GetShips(caster);
        if (shipIndex < 0 || shipIndex >= ships.Count)
        {
            return false;
        }

        var shipToTeleport = ships[shipIndex];

        return IsValidShipPosition(gameState, shipToTeleport, destinationX, destinationY, caster);
    }

    /// <summary>
    /// Checks if the new position for the ship is valid
    /// </summary>
    private bool IsValidShipPosition(
        GameState gameState,
        PlacedShip ship,
        int newX,
        int newY,
        NetPeer caster
    )
    {
        int boardWidth = GameState.boardWidth;
        int boardHeight = GameState.boardHeight;

        if (
            newX < 0
            || newY < 0
            || newX + ship.Width > boardWidth
            || newY + ship.Height > boardHeight
        )
        {
            return false;
        }

        // Check if the new position overlaps with any other ship
        var ships = ShipHandler.GetShips(caster);
        foreach (var otherShip in ships)
        {
            if (ReferenceEquals(otherShip, ship))
            {
                continue;
            }

            if (
                newX < otherShip.X + otherShip.Width
                && newX + ship.Width > otherShip.X
                && newY < otherShip.Y + otherShip.Height
                && newY + ship.Height > otherShip.Y
            )
            {
                return false;
            }
        }

        int playerIndex = gameState.GetPlayerIndex(caster);
        var rocks = RockHandler.GetRockPositions(gameState.boards[playerIndex]);
        bool collidesWithRocks = rocks.Any(rock =>
            rock.X >= newX
            && rock.X < newX + ship.Width
            && rock.Y >= newY
            && rock.Y < newY + ship.Height
        );

        if (collidesWithRocks)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sends teleport notification to both players
    /// </summary>
    private static void SendTeleportNotification(
        NetPeer caster,
        NetPeer opponent,
        int shipIndex,
        int originalX,
        int originalY,
        int newX,
        int newY
    )
    {
        var casterWriter = new NetDataWriter();
        casterWriter.Put("ShipTeleported");
        casterWriter.Put(shipIndex);
        casterWriter.Put(originalX);
        casterWriter.Put(originalY);
        casterWriter.Put(newX);
        casterWriter.Put(newY);
        casterWriter.Put(false);
        caster.Send(casterWriter, DeliveryMethod.ReliableOrdered);

        var opponentWriter = new NetDataWriter();
        opponentWriter.Put("ShipTeleported");
        opponentWriter.Put(shipIndex);
        opponentWriter.Put(originalX);
        opponentWriter.Put(originalY);
        opponentWriter.Put(newX);
        opponentWriter.Put(newY);
        opponentWriter.Put(true);
        opponent.Send(opponentWriter, DeliveryMethod.ReliableOrdered);

        Console.WriteLine(
            $"[Server] Teleport notification sent to both players for ship {shipIndex}"
        );
    }
}
