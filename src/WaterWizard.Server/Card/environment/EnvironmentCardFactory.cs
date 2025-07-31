// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 32 Zeilen
// - justinjd00: 2 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.environment;

public class EnvironmentCardFactory
{
    /// <summary>
    /// Creates the appropriate utility card implementation based on the card variant
    /// </summary>
    /// <param name="variant">The card variant to create</param>
    /// <returns>An IUtilityCard implementation or null if the variant is not a utility card</returns>
    public static IEnvironmentCard? CreateEnvironmentCard(CardVariant variant)
    {
        return variant switch
        {
            CardVariant.Thunder => new ThunderCard(),
            CardVariant.CallWind => new CallWindCard(),
            CardVariant.SpawnRocks => new SpawnRocksCard(),
            CardVariant.RiseSun => new RiseSunCard(),
            _ => null,
        };
    }

    /// <summary>
    /// Checks if the given card variant is a environment card
    /// </summary>
    /// <param name="variant">The card variant to check</param>
    /// <returns>True if it's a environment card, false otherwise</returns>
    public static bool IsEnvironmentCard(CardVariant variant)
    {
        var card = new Cards(variant);
        return card.Type == CardType.Environment;
    }

    /// <summary>
    /// Resets all currently active environment effects on the battlefield.
    /// This includes stopping ongoing global spells such as Thunder, Wind, etc.
    /// Also clears the list of active environment cards if available.
    /// </summary>
    /// <param name="gameState">The current game state</param>
    public static void ResetEnvironmentEffects(GameState gameState)
    {
        Console.WriteLine("[CardHandler] Resetting environment effects");

        // Thunder stoppen
        ThunderCard.ThunderEffectExpired(gameState);
    }
}