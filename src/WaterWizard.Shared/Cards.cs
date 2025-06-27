using System.Linq;

namespace WaterWizard.Shared;

using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Repräsentiert eine einzelne Spielkarte mit Typ, Variante und zugehörigen Eigenschaften.
/// </summary>
public class Cards
{
    /// <summary>
    /// Der übergeordnete Typ der Karte (z. B. Damage, Utility, Environment, Healing).
    /// </summary>
    public CardType Type { get; private set; }

    /// <summary>
    /// Die spezifische Variante der Karte innerhalb ihres Typs.
    /// </summary>
    public CardVariant Variant { get; private set; }

    /// <summary>
    /// Der Mana-Wert, der zum Ausspielen dieser Karte erforderlich ist.
    /// </summary>
    public int Mana { get; private set; }

    // TODO: Mana class einbeziehen

    /// <summary>
    /// Die Zeit (in Sekunden oder als "instant"), die zum Wirken der Karte benötigt wird.
    /// </summary>
    public string? CastTime { get; private set; }

    /// <summary>
    /// Die Dauer (in Sekunden, "instant" oder "permanent") des Karteneffekts.
    /// </summary>
    public string? Duration { get; private set; }
    public float remainingDuration = 0;

    /// <summary>
    /// Gibt an, welches Ziel die Karte betrifft (z. B. "1x1", "ship", "battlefield").
    /// </summary>
    public CardTarget? Target { get; private set; }

    private static readonly Dictionary<CardVariant, CardType> cardTypeMapping = new Dictionary<
        CardVariant,
        CardType
    >
    {
        // Damage Variants
        { CardVariant.MagicAttack, CardType.Damage },
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
        { CardVariant.Shield, CardType.Utility },
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
        { CardVariant.Lifesteal, CardType.Healing },
    };

    private static readonly Dictionary<CardVariant, CardStats> cardStatsMapping = new Dictionary<
        CardVariant,
        CardStats
    >
    {
        // Damage
        {
            CardVariant.MagicAttack,
            new CardStats
            {
                Mana = 1,
                CastTime = "instant",
                Duration = "instant",
                Target = new("1x1"),
            }
        },
        {
            CardVariant.ArcaneMissile,
            new CardStats
            {
                Mana = 2,
                CastTime = "instant",
                Duration = "instant",
                Target = new("random 1x1"),
            }
        },
        {
            CardVariant.Firebolt,
            new CardStats
            {
                Mana = 2,
                CastTime = "instant",
                Duration = "instant",
                Target = new("1x1"),
            }
        },
        {
            CardVariant.Fireball,
            new CardStats
            {
                Mana = 7,
                CastTime = "3",
                Duration = "instant",
                Target = new("3x3"),
            }
        },
        {
            CardVariant.GreedHit,
            new CardStats
            {
                Mana = 5,
                CastTime = "2",
                Duration = "instant",
                Target = new("1x1"),
            }
        },
        {
            CardVariant.FrostBolt,
            new CardStats
            {
                Mana = 2,
                CastTime = "instant",
                Duration = "3",
                Target = new("1x1"),
            }
        },
        // Utility
        {
            CardVariant.HoveringEye,
            new CardStats
            {
                Mana = 2,
                CastTime = "instant",
                Duration = "permanent",
                Target = new("2x1"),
            }
        },
        {
            CardVariant.SummonShip,
            new CardStats
            {
                Mana = 10,
                CastTime = "4",
                Duration = "permanent",
                Target = new("ship"),
            }
        },
        {
            CardVariant.Teleport,
            new CardStats
            {
                Mana = 5,
                CastTime = "1",
                Duration = "permanent",
                Target = new(true, "ship"),
            }
        },
        {
            CardVariant.Paralize,
            new CardStats
            {
                Mana = 4,
                CastTime = "instant",
                Duration = "6",
                Target = new("battlefield"),
            }
        },
        {
            CardVariant.ConeOfCold,
            new CardStats
            {
                Mana = 5,
                CastTime = "2",
                Duration = "5",
                Target = new("3x3"),
            }
        },
        {
            CardVariant.MinorIllusion,
            new CardStats
            {
                Mana = 3,
                CastTime = "instant",
                Duration = "10",
                Target = new("1x1"),
            }
        },
        {
            CardVariant.Polymorph,
            new CardStats
            {
                Mana = 3,
                CastTime = "3",
                Duration = "permanent",
                Target = new("ship"),
            }
        },
        // Environment
        {
            CardVariant.Thunder,
            new CardStats
            {
                Mana = 6,
                CastTime = "instant",
                Duration = "5",
                Target = new("battlefield"),
            }
        },
        {
            CardVariant.Storm,
            new CardStats
            {
                Mana = 6,
                CastTime = "instant",
                Duration = "permanent",
                Target = new("battlefield"),
            }
        },
        {
            CardVariant.SpawnRocks,
            new CardStats
            {
                Mana = 5,
                CastTime = "instant",
                Duration = "permanent",
                Target = new("random 1x1"),
            }
        },
        {
            CardVariant.RiseSun,
            new CardStats
            {
                Mana = 4,
                CastTime = "instant",
                Duration = "permanent",
                Target = new("battlefield"),
            }
        },
        {
            CardVariant.CallWind,
            new CardStats
            {
                Mana = 4,
                CastTime = "instant",
                Duration = "instant",
                Target = new("ship"),
            }
        },
        // Healing
        {
            CardVariant.Heal,
            new CardStats
            {
                Mana = 4,
                CastTime = "instant",
                Duration = "instant",
                Target = new(true, "ship"),
            }
        },
        {
            CardVariant.Mending,
            new CardStats
            {
                Mana = 2,
                CastTime = "1",
                Duration = "6",
                Target = new(true, "ship"),
            }
        },
        {
            CardVariant.MassMending,
            new CardStats
            {
                Mana = 6,
                CastTime = "3",
                Duration = "instant",
                Target = new(true, "ship"),
            }
        },
        {
            CardVariant.PerfectMending,
            new CardStats
            {
                Mana = 6,
                CastTime = "2",
                Duration = "instant",
                Target = new(true, "ship"),
            }
        },
        {
            CardVariant.Lifesteal,
            new CardStats
            {
                Mana = 4,
                CastTime = "instant",
                Duration = "instant",
                Target = new(true, "ship"),
            }
        },
        {
            CardVariant.Shield,
            new CardStats
            {
                Mana = 5,
                CastTime = "instant",
                Duration = "6",
                Target = new(true, "3x3"),  // Set Ally = true to target player's own board
            }
        },
    };

    private ThunderEffect? _activeThunderEffect;
    private List<Cell[,]>? _battlefields;
    private int _gridSize;

    /// <summary>
    /// Erstellt eine neue Karteninstanz anhand der angegebenen Kartenvariante.
    /// Die zugehörigen Typ- und Statuseigenschaften werden automatisch gesetzt.
    /// </summary>
    /// <param name="variant">Die spezifische Variante der Karte.</param>
    /// <exception cref="ArgumentException">Wird ausgelöst, wenn die Variante unbekannt ist oder keine zugehörigen Statistiken vorhanden sind.</exception>
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

    /// <summary>
    /// Gibt die aktuellen Kartendaten auf der Konsole aus (Debug-Zwecke).
    /// </summary>
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

    /// <summary>
    /// Get all Cards that correspond to the CardType <see cref="type"/>.
    /// First Find All Variants with the given type, then creates a new List
    /// of Cards from those <see cref="CardVariant"/>s.
    /// </summary>
    /// <param name="type">The <see cref="CardType"/> using which the Cards get filtered</param>
    /// <returns>A new List of Cards that all correspond to the CardType given</returns>
    public static List<Cards> GetCardsOfType(CardType type)
    {
        List<Cards> cards = [];
        var cardVariants = cardTypeMapping.Where(kVPair => kVPair.Value == type);
        cards.AddRange(from variant in cardVariants select new Cards(variant.Key));
        return cards;
    }

    /// <summary>
    /// Get the Target of this Card as a Vector when it applies, otherwise (0,0)
    /// </summary>
    /// <returns>
    /// Returns the Size of the Target of the Card as a Vector2.
    /// <para/>
    /// or
    /// <para/>
    /// Returns 0x0 Vector if target is: random, ship or battlefield.
    /// </returns>
    public Vector2 TargetAsVector()
    {
        if (Target!.Target.Contains('x') && !Target.Target.Contains("random"))
        {
            int i = Target.Target.IndexOf('x');
            return new(int.Parse(Target.Target[i - 1] + ""), int.Parse(Target.Target[i + 1] + ""));
        }
        else
        {
            return new();
        }
    }

    public void SetBattlefieldInfo(List<Cell[,]> battlefields, int gridSize)
    {
        _battlefields = battlefields;
        _gridSize = gridSize;
    }

    public void Update(float deltaTime)
    {
        if (_activeThunderEffect != null)
        {
            _activeThunderEffect.Update(deltaTime);

            if (!_activeThunderEffect.IsActive)
            {
                _activeThunderEffect = null;
            }
        }
    }

    public void ActivateEffect()
    {
        if (Variant == CardVariant.Thunder && _battlefields != null)
        {
            _activeThunderEffect = new ThunderEffect(_battlefields, _gridSize);
        }
    }

    public bool HasActiveEffect => _activeThunderEffect != null && _activeThunderEffect.IsActive;
}
