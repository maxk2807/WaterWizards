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
}