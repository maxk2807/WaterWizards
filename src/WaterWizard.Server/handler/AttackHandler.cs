using LiteNetLib;
using LiteNetLib.Utils;

namespace WaterWizard.Server.handler;

public class AttackHandler
{
    private static GameState? gameState;

    // Add a method to set the game state
    public static void Initialize(GameState state)
    {
        gameState = state;
    }
    /// <summary>
    /// Handles the attack from one player to another.
    /// Checks if the attack hits a ship and updates the game state accordingly.
    /// If a ship is hit, it checks if the ship is destroyed and sends the result to both players.
    /// </summary>
    /// <param name="attacker">The player who initiated the attack</param>
    /// <param name="defender">The player who was attacked</param>
    /// <param name="x">The x-coordinate of the attack</param>
    /// <param name="y">The y-coordinate of the attack</param>
    public static void HandleAttack(NetPeer attacker, NetPeer defender, int x, int y)
    {
        Console.WriteLine(
            $"[Server] HandleAttack called: attacker={attacker}, defender={defender}, coords=({x},{y})"
        );

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
                }
                else
                {
                    Console.WriteLine($"[Server] Cell ({x},{y}) already damaged");
                    CellHandler.SendCellReveal(attacker, defender, x, y, true);
                }
                break;
            }
        }

        if (!hit)
        {
            Console.WriteLine($"[Server] Miss at ({x},{y})");
            CellHandler.SendCellReveal(attacker, defender, x, y, false); // Updated to include defender
        }

        /// <summary>
        /// Sends the result of the attack to both players.
        /// /// </summary>
        /// <param name="attacker">The player who initiated the attack</param>
        /// <param name="defender">The player who was attacked</param>
        /// <param name="x">The x-coordinate of the attack</param>
        /// <param name="y">The y-coordinate of the attack</param>
        /// <param name="hit">Whether the attack hit a ship</param>
        /// <param name="shipDestroyed">Whether the ship was destroyed</param>
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
    private static void SendAttackResult(
        NetPeer attacker,
        NetPeer defender,
        int x,
        int y,
        bool hit,
        bool shipDestroyed
    )
    {
        var attackerWriter = new NetDataWriter();
        attackerWriter.Put("AttackResult");
        attackerWriter.Put(x);
        attackerWriter.Put(y);
        attackerWriter.Put(hit);
        attackerWriter.Put(shipDestroyed);
        attackerWriter.Put(false);
        attacker.Send(attackerWriter, DeliveryMethod.ReliableOrdered);

        var defenderWriter = new NetDataWriter();
        defenderWriter.Put("AttackResult");
        defenderWriter.Put(x);
        defenderWriter.Put(y);
        defenderWriter.Put(hit);
        defenderWriter.Put(shipDestroyed);
        defenderWriter.Put(true);
        defender.Send(defenderWriter, DeliveryMethod.ReliableOrdered);

        Console.WriteLine(
            $"[Server] Attack result sent: attacker sees result, defender sees damage"
        );
    }
}