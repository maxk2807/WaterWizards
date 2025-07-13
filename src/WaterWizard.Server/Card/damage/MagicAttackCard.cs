using System.Numerics;
using LiteNetLib;
using Raylib_cs;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.damage;

public class MagicAttackCard : IDamageCard
{
    public CardVariant Variant => CardVariant.MagicAttack;

    public Vector2 AreaOfEffect => new(1, 1);

    public int BaseDamage => 1;

    public bool HasSpecialTargeting => false;

    public bool ExecuteDamage(GameState gameState, Vector2 targetCoords, NetPeer attacker, NetPeer defender)
    {
        bool didDamage = false;

        int x = (int)targetCoords.X;
        int y = (int)targetCoords.Y;

        // Get defender's player index for shield check
        int defenderIndex = gameState?.GetPlayerIndex(defender) ?? -1;

        // Check if this coordinate is protected by a shield
        if (defenderIndex != -1 && gameState != null && gameState.IsCoordinateProtectedByShield(x, y, defenderIndex))
        {
            Console.WriteLine($"[Server] Attack at ({x}, {y}) blocked by shield!");
            CellHandler.SendCellReveal(attacker, defender, x, y, false);
            AttackHandler.SendAttackResult(attacker, defender, x, y, false, false);
            return false;
        }

        var ships = ShipHandler.GetShips(defender);
        bool hit = false;
        PlacedShip? hitShip = null;

        foreach (var ship in ships)
        {
            if (x >= ship.X && x < ship.X + ship.Width && y >= ship.Y && y < ship.Y + ship.Height)
            {
                hit = true;
                hitShip = ship;

                bool newDamage = ship.DamageCell(x, y);

                if (newDamage)
                {
                    Console.WriteLine(
                        $"[Server] New damage at ({x},{y}) on ship at ({ship.X},{ship.Y})"
                    );

                    if (ship.IsDestroyed)
                    {
                        Console.WriteLine($"[Server] Ship at ({ship.X},{ship.Y}) destroyed!");
                        ShipHandler.SendShipReveal(attacker, ship, gameState!);
                    }
                    else
                    {
                        CellHandler.SendCellReveal(attacker, defender, x, y, true);
                    }
                    didDamage = true;
                }
                else
                {
                    Console.WriteLine($"[Server] Cell ({x},{y}) already damaged");
                    CellHandler.SendCellReveal(attacker, defender, x, y, true);
                    didDamage = false;
                }
                break;
            }
        }

        if (!hit)
        {
            Console.WriteLine($"[Server] Miss at ({x},{y})");
            CellHandler.SendCellReveal(attacker, defender, x, y, false); // Updated to include defender
            didDamage = false;
        }
        AttackHandler.SendAttackResult(attacker, defender, x, y, hit, hitShip?.IsDestroyed ?? false);

        if (hit && hitShip?.IsDestroyed == true)
        {
            gameState?.CheckGameOver();
        }
        return didDamage;
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer defender)
    {
        return targetCoords.X >= 0 && targetCoords.X < 12 && targetCoords.Y >= 0 && targetCoords.Y < 12;
    }
}