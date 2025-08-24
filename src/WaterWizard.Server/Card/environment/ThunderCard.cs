// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 158 Zeilen
// - Erickk0: 11 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new();   (maxk2807: 145 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Server.utils;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.environment;

public class ThunderCard : IEnvironmentCard
{
    public CardVariant Variant => CardVariant.Thunder;

    public Vector2 AreaOfEffect => new(); //target is whole battlefield

    public bool HasSpecialTargeting => true;

    private static readonly float THUNDER_INTERVAL = 1.75f; // Intervall zwischen Blitzeinschlägen in Sekunden
    private static float thunderTimer = 0;

    public bool ExecuteEnvironment(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        try
        {
            var durationString = new Cards(Variant).Duration!;
            int duration = int.Parse(durationString);
            CardHandler.CardActivation(gameState, Variant, duration);
            return true;
        }
        catch (Exception)
        {
            return false;
            throw;
        }
    }

    public bool IsValidTarget(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer defender
    )
    {
        return true; //true because target is battlefield
    }

    public static void ThunderEffectExpired(GameState gameState)
    {
        Console.WriteLine("\n[Server] Thunder Card expired");
        Console.WriteLine("----------------------------------------");
        foreach (var player in gameState.players)
        {
            NetDataWriter resetWriter = new();
            resetWriter.Put("ThunderReset");
            player.Send(resetWriter, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"Sent ThunderReset to player: {player}");
        }
        Console.WriteLine("----------------------------------------\n");
    }

    public static void HandleActivationEffect(GameState gameState, float passedTime)
    {
        thunderTimer -= passedTime / 1000f;

        if (thunderTimer <= 0)
        {
            thunderTimer = THUNDER_INTERVAL;
            Console.WriteLine("\n[Server] Thunder Strike Round");
            Console.WriteLine("----------------------------------------");

            for (int boardIndex = 0; boardIndex < 2; boardIndex++)
            {
                var targetPlayer = gameState.players[boardIndex];
                var attacker = gameState.players[boardIndex == 0 ? 1 : 0];

                Console.WriteLine(
                    $"Generating 2 thunder strikes for Board[{boardIndex}] (Player: {targetPlayer})"
                );

                for (int strikeNum = 0; strikeNum < 3; strikeNum++)
                {
                    int x = Random.Shared.Next(0, GameState.boardWidth);
                    int y = Random.Shared.Next(0, GameState.boardHeight);

                    bool hit = HandleThunderStrike(gameState, attacker, targetPlayer, x, y);

                    SendThunderVisualEffect(gameState.players, boardIndex, x, y, hit);
                }
            }
            Console.WriteLine("----------------------------------------\n");
        }
    }

    /// <summary>
    /// Handles a single thunder strike and returns if it was a hit
    /// </summary>
    private static bool HandleThunderStrike(
        GameState gameState,
        NetPeer attacker,
        NetPeer targetPlayer,
        int x,
        int y
    )
    {
        // Get target player's index for shield check
        int targetPlayerIndex = gameState.GetPlayerIndex(targetPlayer);

        // Check if this coordinate is protected by a shield
        if (
            targetPlayerIndex != -1
            && gameState.IsCoordinateProtectedByShield(x, y, targetPlayerIndex)
        )
        {
            Console.WriteLine($"    Thunder strike at ({x}, {y}) blocked by shield!");
            CellHandler.SendCellReveal(attacker, targetPlayer, x, y, false, "Thunder");
            return false;
        }

        var ships = ShipHandler.GetShips(targetPlayer);
        bool hit = false;

        foreach (var ship in ships)
        {
            if (x >= ship.X && x < ship.X + ship.Width && y >= ship.Y && y < ship.Y + ship.Height)
            {
                hit = true;
                bool newDamage = ship.DamageCell(x, y);

                Console.WriteLine(
                    $"    Thunder hit ship at ({ship.X}, {ship.Y}), new damage: {newDamage}"
                );

                if (newDamage)
                {
                    if (ship.IsDestroyed)
                    {
                        Console.WriteLine($"    Thunder destroyed ship at ({ship.X}, {ship.Y})!");
                        CellHandler.SendCellReveal(attacker, targetPlayer, x, y, true, "Thunder");
                        ShipHandler.SendShipReveal(attacker, ship, gameState!);
                    }
                    else
                    {
                        CellHandler.SendCellReveal(attacker, targetPlayer, x, y, true, "Thunder");
                    }
                }
                else
                {
                    CellHandler.SendCellReveal(attacker, targetPlayer, x, y, true, "Thunder");
                }
                break;
            }
        }

        if (!hit)
        {
            Console.WriteLine($"    Thunder missed at ({x}, {y})");
            CellHandler.SendCellReveal(attacker, targetPlayer, x, y, false, "Thunder");
        }

        return hit;
    }

    /// <summary>
    /// Sends thunder visual effects to all clients
    /// </summary>
    /// <param name="boardIndex">The referenced board</param>
    /// <param name="hit">Boolean if a ship was hit or not</param>
    /// <param name="players">The connected player</param>
    /// <param name="x">x-Coordinate</param>
    /// <param name="y">y-Coordinate</param>
    private static void SendThunderVisualEffect(
        NetPeer[] players,
        int boardIndex,
        int x,
        int y,
        bool hit
    )
    {
        foreach (var player in players)
        {
            int playerIndex = Array.IndexOf(players, player);
            int displayX = x;
            int displayY = y;
            
            if (boardIndex != playerIndex)
            {
                var (transformedX, transformedY) = CoordinateTransform.UnrotateOpponentCoordinates(
                    x, y, GameState.boardWidth, GameState.boardHeight);
                displayX = transformedX;
                displayY = transformedY;
            }
            
            var writer = new NetDataWriter();
            writer.Put("ThunderVisualEffect");
            writer.Put(boardIndex);
            writer.Put(displayX);
            writer.Put(displayY);
            writer.Put(hit);
            player.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }
}
