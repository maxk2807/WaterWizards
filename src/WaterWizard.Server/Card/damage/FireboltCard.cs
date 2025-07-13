// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 104 Zeilen
// - Erickk0: 33 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new(1, 1);   (erick: 84 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card;

/// <summary>
/// Implementation of the Firebolt damage card
/// Deals damage to a 2x1 area (as defined in the shared Cards class)
/// </summary>
public class FireboltCard : IDamageCard
{
    /// <summary>
    /// The variant of the card
    /// </summary>
    public CardVariant Variant => CardVariant.Firebolt;

    /// <summary>
    /// The area of effect as a Vector2 (width x height)
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
    /// Executes the damage effect of the Firebolt card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates targeted by the card</param>
    /// <param name="attacker">The attacking player</param>
    /// <param name="defender">The defending player</param>
    public bool ExecuteDamage(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer attacker,
        NetPeer defender
    )
    {
        int startX = (int)targetCoords.X;
        int startY = (int)targetCoords.Y;

        var ships = ShipHandler.GetShips(defender);
        bool anyHit = false;

        // Get defender's player index for shield check
        int defenderIndex = gameState.GetPlayerIndex(defender);

        for (int dx = 0; dx < (int)AreaOfEffect.X; dx++)
        {
            for (int dy = 0; dy < (int)AreaOfEffect.Y; dy++)
            {
                int x = startX + dx;
                int y = startY + dy;
                bool cellHit = false;

                // Check if this coordinate is protected by a shield
                if (defenderIndex != -1 && gameState.IsCoordinateProtectedByShield(x, y, defenderIndex))
                {
                    Console.WriteLine($"[Server] Firebolt attack at ({x}, {y}) blocked by shield!");
                    CellHandler.SendCellReveal(attacker, defender, x, y, false, "FireBolt");
                    continue;
                }

                foreach (var ship in ships)
                {
                    if (
                        x >= ship.X
                        && x < ship.X + ship.Width
                        && y >= ship.Y
                        && y < ship.Y + ship.Height
                    )
                    {
                        cellHit = true;
                        bool newDamage = ship.DamageCell(x, y);

                        if (newDamage)
                        {
                            if (ship.IsDestroyed)
                            {
                                gameState.CheckGameOver();
                            }
                            else
                            {
                                CellHandler.SendCellReveal(attacker, defender, x, y, true, "FireBolt");
                            }
                        }
                        else
                        {
                            CellHandler.SendCellReveal(attacker, defender, x, y, true, "FireBolt");
                        }
                        break;
                    }
                }

                if (!cellHit)
                {
                    CellHandler.SendCellReveal(attacker, defender, x, y, false, "FireBolt");
                }

                if (cellHit)
                {
                    anyHit = true;
                }
            }
        }

        return anyHit;
    }

    /// <summary>
    /// Validates if the target coordinates are valid for this card
    /// </summary>
    /// <param name="gameState">The current game state.</param>
    /// <param name="targetCoords">The coordinates targeted by the card.</param>
    /// <param name="defender">The defending player.</param>
    /// <returns>True if the target area is within the board, otherwise false.</returns>
    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer defender)
    {
        int boardWidth = 12;
        int boardHeight = 10;

        return targetCoords.X >= 0
            && targetCoords.Y >= 0
            && targetCoords.X + AreaOfEffect.X <= boardWidth
            && targetCoords.Y + AreaOfEffect.Y <= boardHeight;
    }
}
