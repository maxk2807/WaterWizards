using LiteNetLib;
using System.Numerics;
using WaterWizard.Shared;

namespace WaterWizard.Server;

public static class CardAbilities{

    public static void HandleAbility(CardVariant variant, GameState gameState, Vector2 targetCoords, NetPeer defender)
    {
        switch(variant){
            case CardVariant.MagicAttack: 
                break;
            default:
                Console.WriteLine($"[Server] Cast Card Variant {variant} on coords ({targetCoords.X},{targetCoords.Y})");
                PrintCardArea(variant, targetCoords, gameState, defender);
                break;
        }
    }

private static void PrintCardArea(CardVariant variant, Vector2 targetCoords, GameState gameState, NetPeer defender)
{
    var card = new Cards(variant);
    var area = card.TargetAsVector();
    if (area.X > 0 && area.Y > 0)
    {
        Console.WriteLine($"[Server] Card {variant} trifft Bereich:");
        for (int dx = 0; dx < area.X; dx++)
        {
            for (int dy = 0; dy < area.Y; dy++)
            {
                int tx = (int)targetCoords.X + dx;
                int ty = (int)targetCoords.Y + dy;
                bool hit = gameState.GetShips(defender).Any(ship =>
                    tx >= ship.X && tx < ship.X + ship.Width &&
                    ty >= ship.Y && ty < ship.Y + ship.Height
                );
                Console.WriteLine($"  -> ({tx},{ty}) {(hit ? "[TREFFER]" : "[kein Treffer]")}");
            }
        }
    }
    else
    {
        bool hit = gameState.GetShips(defender).Any(ship =>
            (int)targetCoords.X >= ship.X && (int)targetCoords.X < ship.X + ship.Width &&
            (int)targetCoords.Y >= ship.Y && (int)targetCoords.Y < ship.Y + ship.Height
        );
        Console.WriteLine($"[Server] Card {variant} trifft Einzelzelle ({targetCoords.X},{targetCoords.Y}) {(hit ? "[TREFFER]" : "[kein Treffer]")}");
    }
     }
}