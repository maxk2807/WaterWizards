namespace WaterWizard.Shared;

public class CardTarget
{
    public bool Ally { get; }
    public string Target { get; }

    /// <summary>
    /// Describes the target of the card as a string.
    /// Examples: "1x1", "ship", "battlefield", "random 1x1".
    /// </summary>
    /// <param name="ally">
    /// Whether the target is the player (e.g. to heal) or the opponent (e.g. to attack)
    /// </param>
    /// <param name="target">
    /// The String to describe the target.
    /// Value may include "*x*" to show area of effect,
    /// "ship" to target ships, "battlefield" to target an unspecified target or
    /// "random *x*" to target a random area.
    /// </param>
    public CardTarget(bool ally, string target)
    {
        Ally = ally;
        Target = target;
    }

    /// <summary>
    /// Describes the target of the card as a string.
    /// Examples: "1x1", "ship", "battlefield", "random 1x1".
    /// By defaults attacks the opponent
    /// </summary>
    /// <param name="target">The String to describe the target.
    /// Value may include "*x*" to show area of effect,
    /// "ship" to target ships, "battlefield" to target an unspecified target or
    /// "random *x*" to target a random area.
    /// </param>
    public CardTarget(string target)
    {
        Ally = false;
        Target = target;
    }
}
