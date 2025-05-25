using System.Numerics;
using LiteNetLib;
using WaterWizard.Shared;

namespace WaterWizard.Server;

public static class CardAbilities
{
    public static void HandleAbility(
        CardVariant variant,
        GameState gameState,
        Vector2 targetCoords,
        NetPeer defender
    )
    {
        switch (variant)
        {
            case CardVariant.MagicAttack:
                break;
            default:
                Console.WriteLine(
                    $"[Server] Cast Card Variant {variant} on coords ({targetCoords.X},{targetCoords.Y})"
                );
                PrintCardArea(variant, targetCoords, gameState, defender);
                break;
        }
        var durationString = new Cards(variant).Duration!;
        switch (durationString)
        {
            case "instant":
                break;
            case "permanent":
                break;
            default:
                try
                {
                    int duration = int.Parse(durationString);
                    gameState.CardActivation(variant, duration);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
        }
    }

    /// <summary>
    /// Handles permanent or over time effects of Cards. For consistent results, the degree of effect is dependant on 
    /// the time passed, i.e. for over time effects, if 1% of the total duration passed, the effect is 1% whatever 
    /// effect the Card has.
    /// </summary>
    /// <param name="card">The Card whose effect gets activated</param>
    /// <param name="passedTime">The time since last activation of the card. 0 if first time. Direct relationship 
    /// between degree of effect and passed time needs to be implemented.</param>
    internal static void HandleActivationEffect(Cards card, float passedTime)
    {
        Console.WriteLine($"[Server] Effect of Card {card.Variant} got activated. {passedTime} since last activation");
    }

    private static void PrintCardArea(
        CardVariant variant,
        Vector2 targetCoords,
        GameState gameState,
        NetPeer defender
    )
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
                    bool hit = gameState
                        .GetShips(defender)
                        .Any(ship =>
                            tx >= ship.X
                            && tx < ship.X + ship.Width
                            && ty >= ship.Y
                            && ty < ship.Y + ship.Height
                        );
                    Console.WriteLine($"  -> ({tx},{ty}) {(hit ? "[TREFFER]" : "[kein Treffer]")}");
                }
            }
        }
        else
        {
            bool hit = gameState
                .GetShips(defender)
                .Any(ship =>
                    (int)targetCoords.X >= ship.X
                    && (int)targetCoords.X < ship.X + ship.Width
                    && (int)targetCoords.Y >= ship.Y
                    && (int)targetCoords.Y < ship.Y + ship.Height
                );
            Console.WriteLine(
                $"[Server] Card {variant} trifft Einzelzelle ({targetCoords.X},{targetCoords.Y}) {(hit ? "[TREFFER]" : "[kein Treffer]")}"
            );
        }
    }
}
