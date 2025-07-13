using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.healing;

public class MassMendingCard : IHealingCard
{
    public CardVariant Variant => CardVariant.MassMending;

    public Vector2 AreaOfEffect => new(); // targets all own ships

    public int BaseHealing => 1;

    public bool HasSpecialTargeting => false;

    public bool ExecuteHealing(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        var ships = ShipHandler.GetShips(caster);
        foreach (var ship in ships)
        {
            if (ship.DamagedCells.Count > 0)
            {
                var (x, y) = ship.DamagedCells.First();
                ship.HealCell(x, y);
                ShipHandler.SendHealing(new(x, y), true, caster);
            }
        }
        return true;
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
        => true;
}