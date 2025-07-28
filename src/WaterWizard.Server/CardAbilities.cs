// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 176 Zeilen
// - justinjd00: 91 Zeilen
// - jdewi001: 65 Zeilen
// - erick: 35 Zeilen
// - Erickk0: 11 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - private static readonly Random random = new();   (maxk2807: 169 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.Card;
using WaterWizard.Server.Card.environment;
using WaterWizard.Server.Card.healing;
using WaterWizard.Server.Card.utility;
using WaterWizard.Server.handler;
using WaterWizard.Shared;

namespace WaterWizard.Server;

public static class CardAbilities
{
    private static readonly Random random = new();
    public static void HandleAbility(
        CardVariant variant,
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer defender
    )
    {
        Console.WriteLine($"[CardAbilities] HandleAbility called for {variant}");
        
        var durationString = new Cards(variant).Duration!;
        Console.WriteLine($"[CardAbilities] Card {variant} has duration: {durationString}");
        
        switch (durationString)
        {
            case "instant":
                Console.WriteLine($"[CardAbilities] Processing instant card {variant}");
                CardHandler.CardActivation(gameState, variant, 0); 
                break;
            case "permanent":
                Console.WriteLine($"[CardAbilities] Processing permanent card {variant}");
                CardHandler.CardActivation(gameState, variant, 0);
                break;
            default: 
                try
                {
                    int duration = int.Parse(durationString);
                    Console.WriteLine($"[CardAbilities] Processing timed card {variant} with duration {duration} seconds");
                    CardHandler.CardActivation(gameState, variant, duration);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CardAbilities] Error parsing duration for {variant}: {ex.Message}");
                    CardHandler.CardActivation(gameState, variant, 0);
                    break;
                }
        }
    }

    public static void HandleAbilityWithHandlers(
        CardVariant variant,
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer defender,
        ParalizeHandler paralizeHandler,
        UtilityCardHandler utilityCardHandler
    )
    {
        Console.WriteLine($"[CardAbilities] HandleAbilityWithHandlers called for {variant}");
        
        HandleAbility(variant, gameState, targetCoords, caster, defender);
        
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
        else if (HealingCardFactory.IsHealingCard(variant))
        {
            var healingCard = HealingCardFactory.CreateHealingCard(variant);
            if (healingCard != null)
            {
                Console.WriteLine($"[Server] Executing healing card {variant}");

                if (healingCard.IsValidTarget(gameState, targetCoords, caster, defender))
                {
                    if (caster != null)
                    {
                        bool healingDone = healingCard.ExecuteHealing(
                            gameState,
                            targetCoords,
                            caster,
                            defender
                        );
                        Console.WriteLine(
                            $"[Server] {variant} healing result: {(healingDone ? "healing done" : "no healing")}"
                        );

                        if (healingDone)
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

        // Prüfe, ob es eine Utility-Karte ist
        var card = new Cards(variant);
        if (card.Type == CardType.Utility)
        {
            switch (variant)
            {
                case CardVariant.HoveringEye:
                    var hoveringEyeCard = new HoveringEyeCard();
                    if (hoveringEyeCard.IsValidTarget(gameState, targetCoords, caster, defender))
                    {
                        bool executed = hoveringEyeCard.ExecuteUtility(gameState, targetCoords, caster, defender);
                        Console.WriteLine($"[Server] {variant} utility result: {(executed ? "executed successfully" : "execution failed")}");
                    }
                    else
                    {
                        Console.WriteLine($"[Server] Invalid target for {variant} at ({targetCoords.X}, {targetCoords.Y})");
                    }
                    break;
                case CardVariant.Paralize:
                    var paralizeCard = new ParalizeCard();
                    if (paralizeCard.IsValidTarget(gameState, targetCoords, caster, defender))
                    {
                        paralizeCard.ExecuteUtility(gameState, targetCoords, caster, defender);
                    }
                    break;
                default:
                    utilityCardHandler.HandleUtilityCard(variant, targetCoords, caster, defender);
                    break;
            }
            return;
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
    }

    /// <summary>
    /// Handles permanent or over time effects of Cards. For consistent results, the degree of effect is dependant on
    /// the time passed, i.e. for over time effects, if 1% of the total duration passed, the effect is 1% whatever
    /// effect the Card has.
    /// </summary>
    /// <param name="card">The Card whose effect gets activated</param>
    /// <param name="passedTime">The time since last activation of the card. 0 if first time. Direct relationship
    /// between degree of effect and passed time needs to be implemented.</param>
    internal static void HandleActivationEffect(GameState gameState, Cards card, float passedTime)
    {
        if (card.Variant == CardVariant.Thunder)
        {
            ThunderCard.HandleActivationEffect(gameState, passedTime);
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
