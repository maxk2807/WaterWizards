// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 38 Zeilen
// - jdewi001: 8 Zeilen
// - erick: 6 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Shared;

public class CardTarget
{
    /// <summary>
    /// Whether the CardTarget is "Ally", e.g. a player ship or on the player board.
    /// Default is false: Not Ally, e.g. opponent ship, board etc. or both boards
    /// </summary>
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

    public override string ToString()
    {
        string targetType = Ally ? "Ally" : "Opponent";
        return $"{targetType} - {Target}";
    }
}
