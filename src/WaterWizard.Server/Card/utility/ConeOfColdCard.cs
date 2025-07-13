// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 216 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new(2, 2);   (erick: 193 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.utility;

/// <summary>
/// Implementation of the ConeOfCold utility card
/// Deals damage to a 2x2 area and freezes gold generation for 2 seconds when it hits a ship
/// </summary>
public class ConeOfColdCard : IUtilityCard
{
    /// <summary>
    /// The variant of the card
    /// </summary>
    public CardVariant Variant => CardVariant.ConeOfCold;

    /// <summary>
    /// The area of effect as a Vector2 (2x2 area)
    /// </summary>
    public Vector2 AreaOfEffect => new(2, 2);

    /// <summary>
    /// Whether this card has special targeting rules
    /// </summary>
    public bool HasSpecialTargeting => false;

    /// <summary>
    /// The duration of gold freeze effect in seconds
    /// </summary>
    private const int GoldFreezeDuration = 2;

    /// <summary>
    /// Executes the utility effect of the ConeOfCold card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates targeted by the card (center of 2x2 area)</param>
    /// <param name="caster">The attacking player</param>
    /// <param name="opponent">The defending player</param>
    /// <returns>True if the effect was executed, false otherwise</returns>
    public bool ExecuteUtility(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        int centerX = (int)targetCoords.X;
        int centerY = (int)targetCoords.Y;
        bool anyHit = false;
        bool goldFreezeApplied = false;

        int opponentIndex = gameState.GetPlayerIndex(opponent);

        Console.WriteLine(
            $"[Server] ConeOfCold targeting 2x2 area centered at ({centerX}, {centerY})"
        );
        Console.WriteLine(
            $"[Server] Board dimensions: {GameState.boardWidth} x {GameState.boardHeight}"
        );

        int startX = centerX - (int)Math.Floor(AreaOfEffect.X / 2f);
        int startY = centerY - (int)Math.Floor(AreaOfEffect.Y / 2f);

        for (int dx = 0; dx < 2; dx++)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                int x = startX + dx;
                int y = startY + dy;

                Console.WriteLine(
                    $"[Server] ConeOfCold checking cell ({x}, {y}) [offset: dx={dx}, dy={dy}]"
                );

                if (x < 0 || x >= GameState.boardWidth || y < 0 || y >= GameState.boardHeight)
                {
                    Console.WriteLine($"[Server] ConeOfCold cell ({x}, {y}) is OUT OF BOUNDS");
                    continue;
                }

                if (
                    opponentIndex != -1
                    && gameState.IsCoordinateProtectedByShield(x, y, opponentIndex)
                )
                {
                    Console.WriteLine(
                        $"[Server] ConeOfCold attack at ({x}, {y}) blocked by shield!"
                    );
                    CellHandler.SendCellReveal(caster, opponent, x, y, false, "ConeOfCold");
                    continue;
                }

                var ships = ShipHandler.GetShips(opponent);
                bool hitAtThisPosition = false;

                Console.WriteLine(
                    $"[Server] ConeOfCold checking {ships.Count} ships for cell ({x}, {y})"
                );

                foreach (var ship in ships)
                {
                    Console.WriteLine(
                        $"[Server] Checking ship at ({ship.X}, {ship.Y}) size {ship.Width}x{ship.Height}"
                    );

                    if (
                        x >= ship.X
                        && x < ship.X + ship.Width
                        && y >= ship.Y
                        && y < ship.Y + ship.Height
                    )
                    {
                        hitAtThisPosition = true;
                        anyHit = true;
                        bool newDamage = ship.DamageCell(x, y);

                        Console.WriteLine(
                            $"[Server] ConeOfCold HIT ship at ({ship.X}, {ship.Y}) position ({x}, {y}), new damage: {newDamage}"
                        );

                        if (!goldFreezeApplied)
                        {
                            HandleGoldFreeze(gameState, opponent);
                            goldFreezeApplied = true;
                        }

                        if (newDamage)
                        {
                            if (ship.IsDestroyed)
                            {
                                Console.WriteLine(
                                    $"[Server] ConeOfCold destroyed ship at ({ship.X}, {ship.Y})!"
                                );
                                ShipHandler.SendShipReveal(caster, ship, gameState);
                                gameState.CheckGameOver();
                            }
                            else
                            {
                                CellHandler.SendCellReveal(
                                    caster,
                                    opponent,
                                    x,
                                    y,
                                    true,
                                    "ConeOfCold"
                                );
                            }
                        }
                        else
                        {
                            CellHandler.SendCellReveal(caster, opponent, x, y, true, "ConeOfCold");
                        }
                        break;
                    }
                }

                if (!hitAtThisPosition)
                {
                    Console.WriteLine($"[Server] ConeOfCold MISSED at ({x}, {y})");
                    CellHandler.SendCellReveal(caster, opponent, x, y, false, "ConeOfCold");
                }
            }
        }

        Console.WriteLine(
            $"[Server] ConeOfCold completed - Any hits: {anyHit}, Gold freeze applied: {goldFreezeApplied}"
        );
        return true;
    }

    /// <summary>
    /// Handles the gold freeze mechanic
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="opponent">The defending player whose gold gets frozen</param>
    private void HandleGoldFreeze(GameState gameState, NetPeer opponent)
    {
        int opponentIndex = gameState.GetPlayerIndex(opponent);

        if (opponentIndex == -1)
        {
            Console.WriteLine("[Server] Could not determine player index for gold freeze");
            return;
        }

        gameState.FreezeGoldGeneration(opponentIndex, GoldFreezeDuration);

        Console.WriteLine(
            $"[Server] ConeOfCold froze gold generation for Player {opponentIndex} for {GoldFreezeDuration} seconds"
        );

        SendGoldFreezeNotifications(gameState, opponentIndex, GoldFreezeDuration);
    }

    /// <summary>
    /// Sends gold freeze notifications to both players
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="frozenPlayerIndex">The index of the player whose gold is frozen</param>
    /// <param name="duration">The duration of the freeze in seconds</param>
    private static void SendGoldFreezeNotifications(
        GameState gameState,
        int frozenPlayerIndex,
        int duration
    )
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
    /// <param name="targetCoords">The coordinates targeted by the card (bottom-right of 2x2)</param>
    /// <param name="caster">The player casting the card</param>
    /// <param name="opponent">The defending player</param>
    /// <returns>True if the target area is within the board, otherwise false</returns>
    public bool IsValidTarget(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        int boardWidth = GameState.boardWidth;
        int boardHeight = GameState.boardHeight;

        int startX = (int)targetCoords.X - (int)Math.Floor(AreaOfEffect.X / 2f);
        int startY = (int)targetCoords.Y - (int)Math.Floor(AreaOfEffect.Y / 2f);
        int endX = startX + (int)AreaOfEffect.X - 1;
        int endY = startY + (int)AreaOfEffect.Y - 1;

        return startX >= 0 && startY >= 0 && endX < boardWidth && endY < boardHeight;
    }
}
