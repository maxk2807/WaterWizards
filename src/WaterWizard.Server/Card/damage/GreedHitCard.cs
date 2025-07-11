// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Erickk0: 194 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new(1, 1);   (Erickk0: 171 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card;

/// <summary>
/// Implementation of the GreedHit damage card
/// Deals damage to a single cell and steals 2 gold from the opponent when it hits a ship
/// </summary>
public class GreedHitCard : IDamageCard
{
    /// <summary>
    /// The variant of the card
    /// </summary>
    public CardVariant Variant => CardVariant.GreedHit;

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
    /// The amount of gold stolen when hitting a ship
    /// </summary>
    private const int GoldStealAmount = 2;

    /// <summary>
    /// Executes the damage effect of the GreedHit card
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
            Console.WriteLine($"[Server] GreedHit attack at ({x}, {y}) blocked by shield!");
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

                Console.WriteLine($"[Server] GreedHit hit ship at ({ship.X}, {ship.Y}), new damage: {newDamage}");

                HandleGoldSteal(gameState, attacker, defender);

                if (newDamage)
                {
                    if (ship.IsDestroyed)
                    {
                        Console.WriteLine($"[Server] GreedHit destroyed ship at ({ship.X}, {ship.Y})!");
                        ShipHandler.SendShipReveal(attacker, ship, gameState);
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
            Console.WriteLine($"[Server] GreedHit missed at ({x}, {y})");
            CellHandler.SendCellReveal(attacker, defender, x, y, false);
        }

        return hit;
    }

    /// <summary>
    /// Handles the gold stealing mechanic
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="attacker">The attacking player</param>
    /// <param name="defender">The defending player</param>
    private void HandleGoldSteal(GameState gameState, NetPeer attacker, NetPeer defender)
    {
        int attackerIndex = gameState.GetPlayerIndex(attacker);
        int defenderIndex = gameState.GetPlayerIndex(defender);

        if (attackerIndex == -1 || defenderIndex == -1)
        {
            Console.WriteLine("[Server] Could not determine player indices for gold steal");
            return;
        }

        int defenderGold = defenderIndex == 0 ? gameState.Player1Gold : gameState.Player2Gold;
        int attackerGold = attackerIndex == 0 ? gameState.Player1Gold : gameState.Player2Gold;

        int actualStolen = Math.Min(GoldStealAmount, defenderGold);

        if (actualStolen > 0)
        {
            gameState.SetGold(defenderIndex, defenderGold - actualStolen);
            gameState.SetGold(attackerIndex, attackerGold + actualStolen);

            Console.WriteLine($"[Server] GreedHit stole {actualStolen} gold from Player {defenderIndex} to Player {attackerIndex}");

            gameState.SyncGoldToClient(attackerIndex);
            gameState.SyncGoldToClient(defenderIndex);

            SendGoldTheftNotification(attacker, defender, actualStolen);
        }
        else
        {
            Console.WriteLine($"[Server] GreedHit could not steal gold - defender has no gold");
        }
    }

    /// <summary>
    /// Sends notification about gold theft to both players
    /// </summary>
    /// <param name="attacker">The attacking player</param>
    /// <param name="defender">The defending player</param>
    /// <param name="stolenAmount">The amount of gold stolen</param>
    private static void SendGoldTheftNotification(NetPeer attacker, NetPeer defender, int stolenAmount)
    {
        var attackerWriter = new NetDataWriter();
        attackerWriter.Put("GoldStolen");
        attackerWriter.Put(stolenAmount);
        attackerWriter.Put(true); 
        attacker.Send(attackerWriter, DeliveryMethod.ReliableOrdered);

        var defenderWriter = new NetDataWriter();
        defenderWriter.Put("GoldStolen");
        defenderWriter.Put(stolenAmount);
        defenderWriter.Put(false);
        defender.Send(defenderWriter, DeliveryMethod.ReliableOrdered);

        Console.WriteLine($"[Server] Sent gold theft notifications - {stolenAmount} gold stolen");
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