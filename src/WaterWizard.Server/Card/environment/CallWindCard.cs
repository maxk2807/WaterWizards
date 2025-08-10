// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 77 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new();   (maxk2807: 63 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using Raylib_cs;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Server.ServerGameStates;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.environment;

public class CallWindCard : IEnvironmentCard
{
    public CardVariant Variant => CardVariant.CallWind;

    public Vector2 AreaOfEffect => new(); // battlefield

    public bool HasSpecialTargeting => true;

    public bool ExecuteEnvironment(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        Vector2 randomDirection = RandomDirection();
        HandleMoveShips(gameState, caster, randomDirection);
        HandleMoveShips(gameState, opponent, randomDirection);
        return true;
    }

    private static void HandleMoveShips(
        GameState gameState,
        NetPeer client,
        Vector2 randomDirection
    )
    {
        var ships = ShipHandler.GetShips(client);
        ships.ForEach(ship =>
        {
            //TODO: check for walls and rocks
            Vector2 oldCoords = new(ship.X, ship.Y);
            Vector2 newCoords = Vector2.Add(oldCoords, randomDirection);
            if (
                HandleOutsideBoard(ship, newCoords)
                || HandleOnRocks(gameState, client, ship, newCoords)
            )
            {
                return;
            }
            if (HandleOnShips(gameState, client, ship, newCoords, randomDirection))
            {
                return;
            }
            ship.X = (int)newCoords.X;
            ship.Y = (int)newCoords.Y;
            ShipHandler.HandlePositionUpdate(oldCoords, newCoords, client);
        });
    }

    /// <summary>
    /// Checks whether the given ship will end up on top of another ship after CallWind cast.
    /// That is only possible if there is a ship next to the given ship in the direction that the wind
    /// will be taking them, but the other ship cannot be moved due to a Board Boundary or a Rock
    /// </summary>
    /// <param name="gameState">The current GameState</param>
    /// <param name="client">The Casting Player</param>
    /// <param name="ship">The given ship to be checked</param>
    /// <param name="newCoords">The potential coordinates after CallWind cast</param>
    /// <returns>True if On another Ship after cast, False otherwise</returns>
    private static bool HandleOnShips(GameState gameState, NetPeer client, PlacedShip ship, Vector2 newCoords, Vector2 randomDirection)
    {
        var ships = ShipHandler.GetShips(client);
        var shipsOnNewCoords = ships.Where(ship1 =>
            {
                if (ship1.Equals(ship)) return false;
                return Raylib.CheckCollisionRecs(
                    new(ship1.X, ship1.Y, ship1.Width, ship1.Height),
                    new(newCoords, ship.Width, ship.Height)
                );
            }
        );

        return shipsOnNewCoords.Any(ship1 =>
        {
            Vector2 coords = Vector2.Add(new(ship1.X, ship1.Y), randomDirection);
            return HandleOutsideBoard(ship1, coords)
                || HandleOnRocks(gameState, client, ship1, coords)
                || HandleOnShips(gameState, client, ship1, coords, randomDirection);
        });
    }

    /// <summary>
    /// Checks whether the given Ship will end up on a Rock after CallWind cast
    /// </summary>
    /// <param name="gameState">The currest GameState</param>
    /// <param name="client">The Casting Player</param>
    /// <param name="ship">The Ship to be checked</param>
    /// <param name="newCoords">The potenital coords of the ship after CallWind cast</param>
    /// <returns>True if on Rocks after cast, False otherwise</returns>
    private static bool HandleOnRocks(
        GameState gameState,
        NetPeer client,
        PlacedShip ship,
        Vector2 newCoords
    )
    {
        int playerIndex = gameState.GetPlayerIndex(client);
        var rocks = RockHandler.GetRockPositions(gameState.boards[playerIndex]);
        return rocks.Any(rock =>
            rock.X >= newCoords.X
            && rock.X <= newCoords.X + ship.Width - 1
            && rock.Y >= newCoords.Y
            && rock.Y <= newCoords.Y + ship.Height - 1
        );
    }

    /// <summary>
    /// Checks whether the given Ship is going to end up outside the board after the CallWind cast
    /// </summary>
    /// <param name="ship">The Ship To Check</param>
    /// <param name="newCoords">The potential coordinates of the CallWind cast</param>
    /// <returns>True if Outside Board, false otherwise</returns>
    private static bool HandleOutsideBoard(PlacedShip ship, Vector2 newCoords)
    {
        return newCoords.X < 0
            || newCoords.Y < 0 //left and top of board
            || newCoords.X + ship.Width - 1 >= GameState.boardWidth //right of board (incl. Width of ship)
            || newCoords.Y + ship.Height - 1 >= GameState.boardHeight; //bottom of board (incl. Height of ship)
    }

    private static Vector2 RandomDirection()
    {
        Vector2[] directions =
        [
            new(1, 0), // Right
            new(-1, 0), // Left
            new(0, 1), // Up
            new(0, -1), // Down
        ];

        var random = new Random();
        return directions[random.Next(directions.Length)];
    }

    public bool IsValidTarget(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer defender
    ) => true;
}
