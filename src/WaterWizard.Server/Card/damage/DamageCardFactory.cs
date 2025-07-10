// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jlnhsrm: 41 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Server.Card;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card;

/// <summary>
/// Factory class for creating damage card implementations
/// </summary>
public static class DamageCardFactory
{
    /// <summary>
    /// Creates the appropriate damage card implementation based on the card variant
    /// </summary>
    /// <param name="variant">The card variant to create</param>
    /// <returns>An IDamageCard implementation or null if the variant is not a damage card</returns>
    public static IDamageCard? CreateDamageCard(CardVariant variant)
    {
        return variant switch
        {
            CardVariant.Firebolt => new FireboltCard(),
            CardVariant.ArcaneMissile => new ArcaneMissileCard(),
            CardVariant.GreedHit => new GreedHitCard(),
            CardVariant.FrostBolt => new FrostBoltCard(),
            CardVariant.Fireball => new FireballCard(),
            CardVariant.LifeSteal => new LifeStealCard(),

            _ => null,
        };
    }

    /// <summary>
    /// Checks if the given card variant is a damage card
    /// </summary>
    /// <param name="variant">The card variant to check</param>
    /// <returns>True if it's a damage card, false otherwise</returns>
    public static bool IsDamageCard(CardVariant variant)
    {
        var card = new Cards(variant);
        return card.Type == CardType.Damage;
    }
}