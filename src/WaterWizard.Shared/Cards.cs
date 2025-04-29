namespace WaterWizard.Shared;

using System;
using System.Collections.Generic;

public enum CardType
{
    Damage,
    Utility,
    Environment,
    Healing
}

public enum CardVariant
{
    // Damage Variants
    ArcaneMissile,
    Firebolt,
    Fireball,
    GreedHit,
    FrostBolt,

    // Utility Variants
    HoveringEye,
    SummonShip,
    Teleport,
    Paralize,
    ConeOfCold,
    MinorIllusion,
    Polymorph,

    // Environment Variants
    Thunder,
    Storm,
    SpawnRocks,
    RiseSun,
    CallWind,

    // Healing Variants
    Heal,
    Mending,
    MassMending,
    PerfectMending,
    Lifesteal
}

public class CardStats
{
    public int Mana { get; set; }
    public string? CastTime { get; set; }    // e.g. "instant" or seconds
    public string? Duration { get; set; }   // e.g. "instant", "permanent" or seconds
    public string? Target { get; set; }      // e.g. "1x1", "ship", "battlefield", etc.
}

public class Cards
{
    public CardType Type { get; private set; }
    public CardVariant Variant { get; private set; }
    public int Mana { get; private set; }
    // TODO: Mana class einbeziehen
    public string? CastTime { get; private set; }
    public string? Duration { get; private set; }
    public string? Target { get; private set; }

    private static readonly Dictionary<CardVariant, CardType> cardTypeMapping = new Dictionary<CardVariant, CardType>
    {
        // Damage Variants
        { CardVariant.ArcaneMissile, CardType.Damage },
        { CardVariant.Firebolt, CardType.Damage },
        { CardVariant.Fireball, CardType.Damage },
        { CardVariant.GreedHit, CardType.Damage },
        { CardVariant.FrostBolt, CardType.Damage },

        // Utility Variants
        { CardVariant.HoveringEye, CardType.Utility },
        { CardVariant.SummonShip, CardType.Utility },
        { CardVariant.Teleport, CardType.Utility },
        { CardVariant.Paralize, CardType.Utility },
        { CardVariant.ConeOfCold, CardType.Utility },
        { CardVariant.MinorIllusion, CardType.Utility },
        { CardVariant.Polymorph, CardType.Utility },

        // Environment Variants
        { CardVariant.Thunder, CardType.Environment },
        { CardVariant.Storm, CardType.Environment },
        { CardVariant.SpawnRocks, CardType.Environment },
        { CardVariant.RiseSun, CardType.Environment },
        { CardVariant.CallWind, CardType.Environment },

        // Healing Variants
        { CardVariant.Heal, CardType.Healing },
        { CardVariant.Mending, CardType.Healing },
        { CardVariant.MassMending, CardType.Healing },
        { CardVariant.PerfectMending, CardType.Healing },
        { CardVariant.Lifesteal, CardType.Healing }
    };

    private static readonly Dictionary<CardVariant, CardStats> cardStatsMapping = new Dictionary<CardVariant, CardStats>
    {
        // Damage
        { CardVariant.ArcaneMissile, new CardStats { Mana = 2, CastTime = "instant", Duration = "instant", Target = "random 1x1" } },
        { CardVariant.Firebolt,      new CardStats { Mana = 2, CastTime = "instant", Duration = "instant", Target = "2x1" } },
        { CardVariant.Fireball,      new CardStats { Mana = 7, CastTime = "3",       Duration = "instant", Target = "3x3" } },
        { CardVariant.GreedHit,      new CardStats { Mana = 5, CastTime = "2",       Duration = "instant", Target = "random 1x1" } },
        { CardVariant.FrostBolt,     new CardStats { Mana = 2, CastTime = "instant", Duration = "3",       Target = "1x1" } },

        // Utility
        { CardVariant.HoveringEye,   new CardStats { Mana = 2, CastTime = "instant", Duration = "permanent", Target = "1x1" } },
        { CardVariant.SummonShip,    new CardStats { Mana = 10, CastTime = "4",      Duration = "permanent", Target = "ship" } },
        { CardVariant.Teleport,      new CardStats { Mana = 5, CastTime = "1",       Duration = "permanent", Target = "ship" } },
        { CardVariant.Paralize,      new CardStats { Mana = 4, CastTime = "instant", Duration = "2",         Target = "ship" } },
        { CardVariant.ConeOfCold,   new CardStats { Mana = 5, CastTime = "2",       Duration = "5",         Target = "3x3" } },
        { CardVariant.MinorIllusion, new CardStats { Mana = 3, CastTime = "instant", Duration = "10",        Target = "1x1" } },
        { CardVariant.Polymorph,     new CardStats { Mana = 3, CastTime = "3",       Duration = "permanent", Target = "ship" } },

        // Environment
        { CardVariant.Thunder,       new CardStats { Mana = 6, CastTime = "instant", Duration = "5",         Target = "battlefield" } },
        { CardVariant.Storm,         new CardStats { Mana = 6, CastTime = "instant", Duration = "permanent", Target = "battlefield" } },
        { CardVariant.SpawnRocks,    new CardStats { Mana = 5, CastTime = "instant", Duration = "permanent", Target = "random 1x1" } },
        { CardVariant.RiseSun,       new CardStats { Mana = 4, CastTime = "instant", Duration = "permanent", Target = "battlefield" } },
        { CardVariant.CallWind,      new CardStats { Mana = 4, CastTime = "instant", Duration = "instant",   Target = "ship" } },

        // Healing
        { CardVariant.Heal,          new CardStats { Mana = 4, CastTime = "instant", Duration = "instant",   Target = "ship" } },
        { CardVariant.Mending,       new CardStats { Mana = 2, CastTime = "1",      Duration = "6",         Target = "ship" } },
        { CardVariant.MassMending,   new CardStats { Mana = 6, CastTime = "3",      Duration = "instant",   Target = "ship" } },
        { CardVariant.PerfectMending,new CardStats { Mana = 6, CastTime = "2",      Duration = "instant",   Target = "ship" } },
        { CardVariant.Lifesteal,     new CardStats { Mana = 4, CastTime = "instant", Duration = "instant",   Target = "ship" } }
    };

    public Cards(CardVariant variant)
    {
        if (!cardTypeMapping.ContainsKey(variant))
            throw new ArgumentException("Unknown card variant.");

        Type = cardTypeMapping[variant];
        Variant = variant;

        if (cardStatsMapping.TryGetValue(variant, out var stats))
        {
            Mana = stats.Mana;
            CastTime = stats.CastTime;
            Duration = stats.Duration;
            Target = stats.Target;
        }
        else
        {
            throw new ArgumentException("No stats defined for this card variant.");
        }
    }

    // Methode um alle Eigenschaften anzuzeigen
    public void DisplayCardInfo()
    {
        Console.WriteLine($"Card: {Variant}");
        Console.WriteLine($"Type: {Type}");
        Console.WriteLine($"Mana: {Mana}");
        Console.WriteLine($"Cast Time: {CastTime}");
        Console.WriteLine($"Duration: {Duration}");
        Console.WriteLine($"Target: {Target}");
        Console.WriteLine();
    }
}