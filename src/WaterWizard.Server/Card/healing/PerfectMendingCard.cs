using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.healing;

public class PerfectMendingCard : IHealingCard
{
    public CardVariant Variant => CardVariant.PerfectMending;

    public Vector2 AreaOfEffect => new(); // targets a single ship

    public int BaseHealing => int.MaxValue; // symbolisch: heilt alles

    public bool HasSpecialTargeting => true;

    public bool ExecuteHealing(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        var shipToHeal = FindShip(targetCoords, caster);
        if (shipToHeal != null)
        {
            foreach (var (X, Y) in shipToHeal.DamagedCells.ToList()) // copy of list, so we can modify it while iterating
            {
                shipToHeal.HealCell(X, Y);
                ShipHandler.SendHealing(new(X, Y), true, caster);
            }
            return true;
        }
        return false;
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        var ship = FindShip(targetCoords, caster);
        return ship != null && ship.DamagedCells.Count > 0;
    }

    private static PlacedShip? FindShip(Vector2 targetCoords, NetPeer caster)
    {
        var ships = ShipHandler.GetShips(caster);
        return ships.Find(ship => ship.X == (int)targetCoords.X && ship.Y == (int)targetCoords.Y);
    }
}