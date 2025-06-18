using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.utility;

public class ParalizeCard : IUtilityCard
{
    public CardVariant Variant => CardVariant.Paralize;

    public Vector2 AreaOfEffect => new(); // whole Battlefield

    public bool HasSpecialTargeting => true; // whole Battlefield

    public bool ExecuteUtility(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        throw new NotImplementedException();
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        return true;
    }
}