// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 32 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.healing;

public class HealingCardFactory
{
    /// <summary>
    /// Creates the appropriate healing card implementation based on the card variant
    /// </summary>
    /// <param name="variant">The card variant to create</param>
    /// <returns>An IHealingCard implementation or null if the variant is not a healing card</returns>
    public static IHealingCard? CreateHealingCard(CardVariant variant)
    {
        return variant switch
        {
            CardVariant.Heal => new HealCard(),
            _ => null,
        };
    }

    /// <summary>
    /// Checks if the given card variant is a healing card
    /// </summary>
    /// <param name="variant">The card variant to check</param>
    /// <returns>True if it's a healing card, false otherwise</returns>
    public static bool IsHealingCard(CardVariant variant)
    {
        var card = new Cards(variant);
        return card.Type == CardType.Healing;
    }
}