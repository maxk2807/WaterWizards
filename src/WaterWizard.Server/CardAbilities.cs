using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.Card;
using WaterWizard.Server.handler;
using WaterWizard.Shared;

namespace WaterWizard.Server;

public static class CardAbilities
{
    private static readonly Random random = new();
    private static readonly float THUNDER_INTERVAL = 1.75f;
    private static float thunderTimer = 0;

    public static void HandleAbility(
        CardVariant variant,
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer defender
    )
    {
        if (DamageCardFactory.IsDamageCard(variant))
        {
            var damageCard = DamageCardFactory.CreateDamageCard(variant);
            if (damageCard != null)
            {
                Console.WriteLine($"[Server] Executing damage card {variant}");

                if (damageCard.IsValidTarget(gameState, targetCoords, defender))
                {
                    var attacker = gameState.players.FirstOrDefault(p => p != defender);
                    if (attacker != null)
                    {
                        bool damageDealt = damageCard.ExecuteDamage(
                            gameState,
                            targetCoords,
                            attacker,
                            defender
                        );
                        Console.WriteLine(
                            $"[Server] {variant} damage result: {(damageDealt ? "damage dealt" : "no damage")}"
                        );

                        if (damageDealt)
                        {
                            gameState.CheckGameOver();
                        }
                    }
                }
                else
                {
                    Console.WriteLine(
                        $"[Server] Invalid target for {variant} at ({targetCoords.X}, {targetCoords.Y})"
                    );
                }
                return;
            }
        }

        switch (variant)
        {
            case CardVariant.Thunder:
                Console.WriteLine($"[Server] Thunder-Karte aktiviert!");
                break;
            default:
                Console.WriteLine(
                    $"[Server] Cast Card Variant {variant} on coords ({targetCoords.X},{targetCoords.Y})"
                );
                PrintCardArea(variant, targetCoords, gameState, defender);
                break;
        }
        CardHandler cardHandler = new(gameState);
        var durationString = new Cards(variant).Duration!;
        switch (durationString)
        {
            case "instant":
                if (variant == CardVariant.Heal)
                {
                    var ships = ShipHandler.GetShips(caster);
                    var healed = ships.Find(ship => ship.X == targetCoords.X && ship.Y == targetCoords.Y);
                    ShipHandler.HandleShipHealing(caster, healed, variant);
                }
                break;
            case "permanent":
                Console.WriteLine($"[Server] Activated Card: {variant}");
                break;
            default:
                try
                {
                    int duration = int.Parse(durationString);
                    cardHandler.CardActivation(variant, duration);
                    Console.WriteLine(
                        $"[Server] Activated Card: {variant} for {duration} seconds"
                    );
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
        if (card.Variant == CardVariant.Thunder)
        {
            thunderTimer -= passedTime / 1000f; // Konvertiere zu Sekunden

            if (thunderTimer <= 0)
            {
                // Erzeuge neue Donnereinschläge
                Console.WriteLine(
                    $"[Server] Thunder strikes! Time since last activation: {passedTime}ms"
                );
                thunderTimer = THUNDER_INTERVAL;

                // TODO: Implementiere die Logik für den Donnereinschlag
                // Hier müssen wir die Koordinaten an den Client senden
                // und die Treffer überprüfen
            }
        }
        else
        {
            Console.WriteLine(
                $"[Server] Effect of Card {card.Variant} got activated. {passedTime} since last activation"
            );
        }
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
                    bool hit = ShipHandler
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
            bool hit = ShipHandler
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
