using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.environment;

public class StormCard : IEnvironmentCard
{
    public CardVariant Variant => CardVariant.Storm;

    public Vector2 AreaOfEffect => new(); // battlefield

    public bool HasSpecialTargeting => true;

    public bool ExecuteEnvironment(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        Vector2 randomDirection = RandomDirection();
        var casterShips = ShipHandler.GetShips(caster);
        casterShips.ForEach(ship =>
        {
            //TODO: check for walls and rocks
            Vector2 oldCoords = new(ship.X, ship.Y);
            Vector2 newCoords = Vector2.Add(oldCoords, randomDirection);
            ship.X = (int)newCoords.X;
            ship.Y = (int)newCoords.Y;
            ShipHandler.HandlePositionUpdate(oldCoords, newCoords, caster);
        });
        var opponentShips = ShipHandler.GetShips(opponent);
        opponentShips.ForEach(ship =>
        {
            Vector2 oldCoords = new(ship.X, ship.Y);
            Vector2 newCoords = Vector2.Add(oldCoords, randomDirection);
            ship.X = (int)newCoords.X;
            ship.Y = (int)newCoords.Y;
            ShipHandler.HandlePositionUpdate(oldCoords, newCoords, opponent);
        });
        return true;
    }

    private static Vector2 RandomDirection()
    {
        Vector2[] directions =
        [
            new(1, 0),   // Right
            new(-1, 0),  // Left
            new(0, 1),   // Up
            new(0, -1)   // Down
        ];

        var random = new Random();
        return directions[random.Next(directions.Length)];
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer defender)
        => true;

}