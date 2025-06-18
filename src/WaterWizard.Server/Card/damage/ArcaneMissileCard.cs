using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card;

/// <summary>
/// Implementation of the Arcane Missile damage card
/// Hits 3 random fields on the opponent's board
/// </summary>
public class ArcaneMissileCard : IDamageCard
{
    /// <summary>
    /// The variant of the card
    /// </summary>
    public CardVariant Variant => CardVariant.ArcaneMissile;

    /// <summary>
    /// The area of effect as a Vector2 (not applicable for random targeting)
    /// </summary>
    public Vector2 AreaOfEffect => new(1, 1); // Each missile hits a single cell

    /// <summary>
    /// The base damage this card deals per missile
    /// </summary>
    public int BaseDamage => 1;

    /// <summary>
    /// Whether this card has special targeting rules
    /// </summary>
    public bool HasSpecialTargeting => true; // Uses random targeting

    /// <summary>
    /// The number of missiles fired
    /// </summary>
    private const int MissileCount = 3;

    /// <summary>
    /// Executes the damage effect of the Arcane Missile card
    /// Fires 3 missiles at random locations on the opponent's board
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates targeted by the card (ignored for random targeting)</param>
    /// <param name="attacker">The attacking player</param>
    /// <param name="defender">The defending player</param>
    /// <returns>True if any damage was dealt, false otherwise</returns>
    public bool ExecuteDamage(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer attacker,
        NetPeer defender
    )
    {
        var ships = ShipHandler.GetShips(defender);
        bool anyHit = false;
        var random = new Random();

        Console.WriteLine($"[Server] Firing {MissileCount} Arcane Missiles at random locations");

        for (int missile = 0; missile < MissileCount; missile++)
        {
            int x = random.Next(0, GameState.boardWidth);
            int y = random.Next(0, GameState.boardHeight);
            bool cellHit = false;

            Console.WriteLine($"[Server] Arcane Missile #{missile + 1} targeting ({x}, {y})");

            foreach (var ship in ships)
            {
                if (x >= ship.X && x < ship.X + ship.Width &&
                    y >= ship.Y && y < ship.Y + ship.Height)
                {
                    cellHit = true;
                    bool newDamage = ship.DamageCell(x, y);

                    Console.WriteLine($"[Server] Arcane Missile hit ship at ({ship.X}, {ship.Y}), new damage: {newDamage}");

                    if (newDamage)
                    {
                        if (ship.IsDestroyed)
                        {
                            Console.WriteLine($"[Server] Arcane Missile destroyed ship at ({ship.X}, {ship.Y})!");
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

            if (!cellHit)
            {
                Console.WriteLine($"[Server] Arcane Missile #{missile + 1} missed at ({x}, {y})");
                CellHandler.SendCellReveal(attacker, defender, x, y, false);
            }

            if (cellHit)
            {
                anyHit = true;
            }
        }

        Console.WriteLine($"[Server] Arcane Missile volley complete. Any hits: {anyHit}");
        return anyHit;
    }

    /// <summary>
    /// Validates if the target coordinates are valid for this card
    /// Since Arcane Missile uses random targeting, this always returns true
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates targeted by the card (ignored)</param>
    /// <param name="defender">The defending player</param>
    /// <returns>Always true since targeting is random</returns>
    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer defender)
    {
        return true;
    }
}