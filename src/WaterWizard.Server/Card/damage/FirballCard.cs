// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jlnhsrm: 118 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new(3, 3);   (jlnhsrm: 96 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.damage
{
    /// <summary>
    /// Implementation of the Fireball damage card
    /// Deals damage to a 3x3 area centered on the target coordinate
    /// </summary>
    public class FireballCard : IDamageCard
    {
        /// <summary>
        /// The variant of the card
        /// </summary>
        public CardVariant Variant => CardVariant.Fireball;

        /// <summary>
        /// The area of effect as a Vector2 (width x height)
        /// </summary>
        public Vector2 AreaOfEffect => new(3, 3);

        /// <summary>
        /// The base damage this card deals
        /// </summary>
        public int BaseDamage => 3;

        /// <summary>
        /// Whether this card has special targeting rules
        /// </summary>
        public bool HasSpecialTargeting => false;

        /// <summary>
        /// Executes the damage effect of the Fireball card
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
            int startX = (int)targetCoords.X - 1;
            int startY = (int)targetCoords.Y - 1;

            var ships = ShipHandler.GetShips(defender);
            bool anyHit = false;

            int defenderIndex = gameState.GetPlayerIndex(defender);

            for (int dx = 0; dx < (int)AreaOfEffect.X; dx++)
            {
                for (int dy = 0; dy < (int)AreaOfEffect.Y; dy++)
                {
                    int x = startX + dx;
                    int y = startY + dy;
                    bool cellHit = false;

                    if (defenderIndex != -1 && gameState.IsCoordinateProtectedByShield(x, y, defenderIndex))
                    {
                        Console.WriteLine($"[Server] Fireball attack at ({x}, {y}) blocked by shield!");
                        CellHandler.SendCellReveal(attacker, defender, x, y, false, "Fireball");
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

                            Console.WriteLine(
                                $"[Server] Fireball hit ship at ({ship.X}, {ship.Y}), new damage: {newDamage}"
                            );

                            if (newDamage)
                            {
                                if (ship.IsDestroyed)
                                {
                                    Console.WriteLine(
                                        $"[Server] Fireball destroyed ship at ({ship.X}, {ship.Y})!"
                                    );
                                    CellHandler.SendCellReveal(attacker, defender, x, y, true, "Fireball");
                                    ShipHandler.SendShipReveal(attacker, ship, gameState);
                                    gameState.CheckGameOver();
                                }
                                else
                                {
                                    CellHandler.SendCellReveal(attacker, defender, x, y, true, "Fireball");
                                }
                            }
                            else
                            {
                                CellHandler.SendCellReveal(attacker, defender, x, y, true, "Fireball");
                            }
                            break;
                        }
                    }

                    if (!cellHit)
                    {
                        Console.WriteLine($"[Server] Fireball missed at ({x}, {y})");
                        CellHandler.SendCellReveal(attacker, defender, x, y, false, "Fireball");
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

            return targetCoords.X >= 1
                && targetCoords.Y >= 1
                && targetCoords.X + 2 < boardWidth
                && targetCoords.Y + 2 < boardHeight;
        }
    }
}
