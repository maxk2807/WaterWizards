// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 130 Zeilen
// - Erickk0: 13 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.utils;

namespace WaterWizard.Server.handler;

/// <summary>
/// Verwaltet Angriffe zwischen Spielern, prüft Treffer und synchronisiert den Spielzustand.
/// </summary>
public class AttackHandler
{
    private static GameState? gameState;

    /// <summary>
    /// Initialisiert den Handler mit dem aktuellen Spielzustand.
    /// </summary>
    public static void Initialize(GameState state)
    {
        gameState = state;
    }

    /// <summary>
    /// Verarbeitet einen Angriff und prüft, ob ein Schiff getroffen oder zerstört wurde.
    /// </summary>
    /// <param name="attacker">Angreifender Spieler</param>
    /// <param name="defender">Verteidigender Spieler</param>
    /// <param name="x">X-Koordinate des Angriffs</param>
    /// <param name="y">Y-Koordinate des Angriffs</param>
    public static void HandleAttack(NetPeer attacker, NetPeer defender, int x, int y)
    {
        Console.WriteLine(
            $"[Server] HandleAttack called: attacker={attacker}, defender={defender}, coords=({x},{y})"
        );

        // Get defender's player index for shield check
        int defenderIndex = gameState?.GetPlayerIndex(defender) ?? -1;

        // Check if this coordinate is protected by a shield
        if (
            defenderIndex != -1
            && gameState != null
            && gameState.IsCoordinateProtectedByShield(x, y, defenderIndex)
        )
        {
            Console.WriteLine($"[Server] Attack at ({x}, {y}) blocked by shield!");
            CellHandler.SendCellReveal(attacker, defender, x, y, false, "Attack");
            SendAttackResult(attacker, defender, x, y, false, false);
            return;
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
                        CellHandler.SendCellReveal(attacker, defender, x, y, true, "Attack");
                    }
                }
                else
                {
                    Console.WriteLine($"[Server] Cell ({x},{y}) already damaged");
                    CellHandler.SendCellReveal(attacker, defender, x, y, true, "Attack");
                }
                break;
            }
        }

        if (!hit)
        {
            Console.WriteLine($"[Server] Miss at ({x},{y})");
            CellHandler.SendCellReveal(attacker, defender, x, y, false, "Attack"); // Updated to include defender
        }

        /// <summary>
        /// Sendet das Ergebnis eines Angriffs an beide Spieler.
        /// </summary>
        /// <param name="attacker">Angreifender Spieler</param>
        /// <param name="defender">Verteidigender Spieler</param>
        /// <param name="x">X-Koordinate</param>
        /// <param name="y">Y-Koordinate</param>
        /// <param name="hit">Ob ein Schiff getroffen wurde</param>
        /// <param name="shipDestroyed">Ob das Schiff zerstört wurde</param>
        SendAttackResult(attacker, defender, x, y, hit, hitShip?.IsDestroyed ?? false);

        if (hit && hitShip?.IsDestroyed == true)
        {
            gameState?.CheckGameOver();
        }
    }

    /// <summary>
    /// Sends the result of an attack to both players.
    /// </summary>
    /// <param name="attacker">The player who initiated the attack</param>
    /// <param name="defender">The player who was attacked</param>
    /// <param name="x">The x-coordinate of the attack</param>
    /// <param name="y">The y-coordinate of the attack</param>
    /// <param name="hit">Whether the attack hit a ship</param>
    /// <param name="shipDestroyed">Whether the ship was destroyed</param>
    public static void SendAttackResult(
        NetPeer attacker,
        NetPeer defender,
        int x,
        int y,
        bool hit,
        bool shipDestroyed
    )
    {
        var (attackerDisplayX, attackerDisplayY) = CoordinateTransform.UnrotateOpponentCoordinates(
            x, y, GameState.boardWidth, GameState.boardHeight);
            
        var attackerWriter = new NetDataWriter();
        attackerWriter.Put("AttackResult");
        attackerWriter.Put(attackerDisplayX);
        attackerWriter.Put(attackerDisplayY);
        attackerWriter.Put(hit);
        attackerWriter.Put(shipDestroyed);
        attackerWriter.Put(false); // isDefender = false for attacker
        attacker.Send(attackerWriter, DeliveryMethod.ReliableOrdered);

        // For the defender (viewing own board), use original coordinates
        var defenderWriter = new NetDataWriter();
        defenderWriter.Put("AttackResult");
        defenderWriter.Put(x);
        defenderWriter.Put(y);
        defenderWriter.Put(hit);
        defenderWriter.Put(shipDestroyed);
        defenderWriter.Put(true); // isDefender = true for defender
        defender.Send(defenderWriter, DeliveryMethod.ReliableOrdered);

        Console.WriteLine(
            $"[Server] Attack result sent: attacker sees ({attackerDisplayX},{attackerDisplayY}), defender sees ({x},{y})"
        );
    }
}
