using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.healing;

public class HealCard : IHealingCard
{
    public CardVariant Variant => CardVariant.Heal;

    public Vector2 AreaOfEffect => new(); //indeterminate, targets ships

    public int BaseHealing => 1;

    public bool HasSpecialTargeting => true; // targets ships

    public bool ExecuteHealing(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        var shipToHeal = FindShip(targetCoords, caster);
        if (shipToHeal != null)
        {
            for (int i = 0; i < BaseHealing; i++)
            {
                if (shipToHeal.DamagedCells.Count > 0)
                {
                    var (X, Y) = shipToHeal.DamagedCells.First();
                    shipToHeal.HealCell(X, Y);
                    ShipHandler.SendHealing(new(X, Y), true, caster);
                }
            }
            return true;
        }
        return false;
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        PlacedShip? targetShip = FindShip(targetCoords, caster); //valid target when targeting ship
        return targetShip != null;
    }

    private static PlacedShip? FindShip(Vector2 targetCoords, NetPeer caster)
    {
        var ships = ShipHandler.GetShips(caster);
        var ship = ships.Find(ship => ship.X == (int)targetCoords.X && ship.Y == (int)targetCoords.Y);
        return ship;
    }
}