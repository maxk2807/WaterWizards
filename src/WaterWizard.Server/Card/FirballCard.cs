using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card;

/// <summary>
/// Implementation of the Fireball damage card
/// Deals damage to a 3x3 area centered on the target coordinate
/// </summary>
public class FireballCard : IDamageCard
{
    public CardVariant Variant => CardVariant.Fireball;
    public Vector2 AreaOfEffect => new(3, 3);
    public int BaseDamage => 3;
    public bool HasSpecialTargeting => false;

    public bool ExecuteDamage(GameState gameState, Vector2 targetCoords, NetPeer attacker, NetPeer defender)
    {
        int startX = (int)targetCoords.X - 1;
        int startY = (int)targetCoords.Y - 1;

        var ships = gameState.GetShips(defender);
        bool anyHit = false;

        for (int dx = 0; dx < (int)AreaOfEffect.X; dx++)
        {
            for (int dy = 0; dy < (int)AreaOfEffect.Y; dy++)
            {
                int x = startX + dx;
                int y = startY + dy;
                bool cellHit = false;

                foreach (var ship in ships)
                {
                    if (x >= ship.X && x < ship.X + ship.Width &&
                        y >= ship.Y && y < ship.Y + ship.Height)
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
                                gameState.SendCellReveal(attacker, defender, x, y, true);
                            }
                        }
                        else
                        {
                            gameState.SendCellReveal(attacker, defender, x, y, true);
                        }
                        break;
                    }
                }

                if (!cellHit)
                {
                    gameState.SendCellReveal(attacker, defender, x, y, false);
                }

                if (cellHit)
                {
                    anyHit = true;
                }
            }
        }

        return anyHit;
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer defender)
    {
        int boardWidth = 12;
        int boardHeight = 10;

        return targetCoords.X >= 1 &&
               targetCoords.Y >= 1 &&
               targetCoords.X + 2 < boardWidth &&
               targetCoords.Y + 2 < boardHeight;
    }
}
