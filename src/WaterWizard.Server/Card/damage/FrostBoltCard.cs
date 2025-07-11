// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Erickk0: 185 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new(1, 1);   (Erickk0: 162 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card;

/// <summary>
/// Implementation of the FrostBolt damage card
/// Deals damage to a single cell and freezes gold generation for 8 seconds when it hits a ship
/// </summary>
public class FrostBoltCard : IDamageCard
{
    /// <summary>
    /// The variant of the card
    /// </summary>
    public CardVariant Variant => CardVariant.FrostBolt;

    /// <summary>
    /// The area of effect as a Vector2 (single cell)
    /// </summary>
    public Vector2 AreaOfEffect => new(1, 1);

    /// <summary>
    /// The base damage this card deals
    /// </summary>
    public int BaseDamage => 1;

    /// <summary>
    /// Whether this card has special targeting rules
    /// </summary>
    public bool HasSpecialTargeting => false;

    /// <summary>
    /// The duration of gold freeze effect in seconds
    /// </summary>
    private const int GoldFreezeDuration = 8;

    /// <summary>
    /// Executes the damage effect of the FrostBolt card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates targeted by the card</param>
    /// <param name="attacker">The attacking player</param>
    /// <param name="defender">The defending player</param>
    /// <returns>True if damage was dealt, false otherwise</returns>
    public bool ExecuteDamage(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer attacker,
        NetPeer defender
    )
    {
        int x = (int)targetCoords.X;
        int y = (int)targetCoords.Y;

        // Get defender's player index for shield check
        int defenderIndex = gameState.GetPlayerIndex(defender);

        // Check if this coordinate is protected by a shield
        if (defenderIndex != -1 && gameState.IsCoordinateProtectedByShield(x, y, defenderIndex))
        {
            Console.WriteLine($"[Server] FrostBolt attack at ({x}, {y}) blocked by shield!");
            CellHandler.SendCellReveal(attacker, defender, x, y, false);
            return false;
        }

        var ships = ShipHandler.GetShips(defender);
        bool hit = false;

        foreach (var ship in ships)
        {
            if (x >= ship.X && x < ship.X + ship.Width &&
                y >= ship.Y && y < ship.Y + ship.Height)
            {
                hit = true;
                bool newDamage = ship.DamageCell(x, y);

                Console.WriteLine($"[Server] FrostBolt hit ship at ({ship.X}, {ship.Y}), new damage: {newDamage}");

                HandleGoldFreeze(gameState, defender);

                if (newDamage)
                {
                    if (ship.IsDestroyed)
                    {
                        Console.WriteLine($"[Server] FrostBolt destroyed ship at ({ship.X}, {ship.Y})!");
                        ShipHandler.SendShipReveal(attacker, ship, gameState);
                        gameState.CheckGameOver();
                    }
                    else
                    {
                        CellHandler.SendCellReveal(attacker, defender, x, y, true);
                    }
                }
                else
                {
                    CellHandler.SendCellReveal(attacker, defender, x, y, true);
                }
                break;
            }
        }

        if (!hit)
        {
            Console.WriteLine($"[Server] FrostBolt missed at ({x}, {y})");
            CellHandler.SendCellReveal(attacker, defender, x, y, false);
        }

        return hit;
    }

    /// <summary>
    /// Handles the gold freeze mechanic
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="defender">The defending player whose gold gets frozen</param>
    private void HandleGoldFreeze(GameState gameState, NetPeer defender)
    {
        int defenderIndex = gameState.GetPlayerIndex(defender);

        if (defenderIndex == -1)
        {
            Console.WriteLine("[Server] Could not determine player index for gold freeze");
            return;
        }

        gameState.FreezeGoldGeneration(defenderIndex, GoldFreezeDuration);

        Console.WriteLine($"[Server] FrostBolt froze gold generation for Player {defenderIndex} for {GoldFreezeDuration} seconds");

        SendGoldFreezeNotifications(gameState, defenderIndex, GoldFreezeDuration);
    }

    /// <summary>
    /// Sends gold freeze notifications to both players (similar to paralysis notifications)
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="frozenPlayerIndex">The index of the player whose gold is frozen</param>
    /// <param name="duration">The duration of the freeze in seconds</param>
    private static void SendGoldFreezeNotifications(GameState gameState, int frozenPlayerIndex, int duration)
    {
        for (int i = 0; i < gameState.Server.ConnectedPeersCount; i++)
        {
            var peer = gameState.Server.ConnectedPeerList[i];
            var writer = new NetDataWriter();
            
            if (i == frozenPlayerIndex)
            {
                writer.Put("GoldFrozen");
                writer.Put(duration);
                writer.Put(true); 
            }
            else
            {
                writer.Put("EnemyGoldFrozen");
                writer.Put(duration);
                writer.Put(false); 
            }
            
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Server] Sent gold freeze notification to Player {i}");
        }
    }

    /// <summary>
    /// Validates if the target coordinates are valid for this card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates targeted by the card</param>
    /// <param name="defender">The defending player</param>
    /// <returns>True if the target area is within the board, otherwise false</returns>
    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer defender)
    {
        int boardWidth = GameState.boardWidth;
        int boardHeight = GameState.boardHeight;

        return targetCoords.X >= 0
            && targetCoords.Y >= 0
            && targetCoords.X < boardWidth
            && targetCoords.Y < boardHeight;
    }
}