// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Paul: 32 Zeilen
// - erick: 4 Zeilen
// - justinjd00: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.utility;

public class UtilityCardFactory
{
    /// <summary>
    /// Creates the appropriate utility card implementation based on the card variant
    /// </summary>
    /// <param name="variant">The card variant to create</param>
    /// <returns>An IUtilityCard implementation or null if the variant is not a utility card</returns>
    public static IUtilityCard? CreateUtilityCard(CardVariant variant)
    {
        return variant switch
        {
            CardVariant.Paralize => new ParalizeCard(),
            CardVariant.HoveringEye => new HoveringEyeCard(),
            CardVariant.SummonShip => new SummonShipCard(),
            CardVariant.Shield => new ShieldCard(),
            CardVariant.Teleport => new TeleportCard(),
            CardVariant.ConeOfCold => new ConeOfColdCard(),
            _ => null,
        };
    }

    /// <summary>
    /// Checks if the given card variant is a utility card
    /// </summary>
    /// <param name="variant">The card variant to check</param>
    /// <returns>True if it's a utility card, false otherwise</returns>
    public static bool IsUtilityCard(CardVariant variant)
    {
        var card = new Cards(variant);
        return card.Type == CardType.Utility;
    }
}
