using System.Numerics;
using WaterWizard.Shared;

namespace WaterWizard.Server;

public static class CardAbilities{

    public static void HandleAbility(CardVariant variant, GameState gameState, Vector2 targetCoords)
    {
        switch(variant){
            case CardVariant.MagicAttack: 
                break;
            default:
                Console.WriteLine($"[Server] Cast Card Variant {variant} on coords ({targetCoords.X},{targetCoords.Y})");
                break;
        }
    }
}