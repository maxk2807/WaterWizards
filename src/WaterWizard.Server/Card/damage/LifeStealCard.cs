using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card;

/// <summary>
/// Implementation of the Lifesteal damage card
/// Deals damage to a single cell and heals one of the caster's ships when it hits an enemy ship
/// </summary>
public class LifeStealCard : IDamageCard
{
    /// <summary>
    /// The variant of the card
    /// </summary>
    public CardVariant Variant => CardVariant.LifeSteal;

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
    /// The amount of healing done when hitting a ship
    /// </summary>
    private const int HealingAmount = 1;

    /// <summary>
    /// Executes the damage effect of the Lifesteal card
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

        int defenderIndex = gameState.GetPlayerIndex(defender);

        if (defenderIndex != -1 && gameState.IsCoordinateProtectedByShield(x, y, defenderIndex))
        {
            Console.WriteLine($"[Server] Lifesteal attack at ({x}, {y}) blocked by shield!");
            CellHandler.SendCellReveal(attacker, defender, x, y, false, "LifeSteal");
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

                Console.WriteLine($"[Server] Lifesteal hit ship at ({ship.X}, {ship.Y}), new damage: {newDamage}");

                HandleLifesteal(gameState, attacker);

                if (newDamage)
                {
                    if (ship.IsDestroyed)
                    {
                        Console.WriteLine($"[Server] Lifesteal destroyed ship at ({ship.X}, {ship.Y})!");
                        ShipHandler.SendShipReveal(attacker, ship, gameState);
                    }
                    else
                    {
                        CellHandler.SendCellReveal(attacker, defender, x, y, true, "LifeSteal");
                    }
                }
                else
                {
                    CellHandler.SendCellReveal(attacker, defender, x, y, true, "LifeSteal");
                }
                break;
            }
        }

        if (!hit)
        {
            Console.WriteLine($"[Server] Lifesteal missed at ({x}, {y})");
            CellHandler.SendCellReveal(attacker, defender, x, y, false, "LifeSteal");
        }

        return hit;
    }

    /// <summary>
    /// Handles the lifesteal healing mechanic
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="attacker">The attacking player who will receive healing</param>
    private void HandleLifesteal(GameState gameState, NetPeer attacker)
    {
        var attackerShips = ShipHandler.GetShips(attacker);
        
        var damagedShips = attackerShips.Where(ship => ship.DamagedCells.Count > 0).ToList();
        
        if (damagedShips.Count > 0)
        {
            var shipToHeal = damagedShips.First();
            var (healX, healY) = shipToHeal.DamagedCells.First();
            
            shipToHeal.HealCell(healX, healY);
            
            Console.WriteLine($"[Server] Lifesteal healed ship at ({shipToHeal.X}, {shipToHeal.Y}) cell ({healX}, {healY})");
            
            ShipHandler.SendHealing(new Vector2(healX, healY), true, attacker);
        }
        else
        {
            Console.WriteLine("[Server] Lifesteal: No damaged ships to heal");
        }
    }

    /// <summary>
    /// Validates if the target coordinates are valid for this card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates to validate</param>
    /// <param name="defender">The player being targeted</param>
    /// <returns>True if the target is valid, false otherwise</returns>
    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer defender)
    {
        int boardWidth = GameState.boardWidth;
        int boardHeight = GameState.boardHeight;

        return targetCoords.X >= 0
            && targetCoords.Y >= 0
            && targetCoords.X + AreaOfEffect.X <= boardWidth
            && targetCoords.Y + AreaOfEffect.Y <= boardHeight;
    }
}