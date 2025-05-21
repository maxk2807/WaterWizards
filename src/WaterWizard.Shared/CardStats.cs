namespace WaterWizard.Shared;

/// <summary>
/// Enthält die statistischen Eigenschaften einer Spielkarte.
/// </summary>
public class CardStats
{
    /// <summary>
    /// Gibt die Menge an Mana an, die benötigt wird, um die Karte zu wirken.
    /// </summary>
    public int Mana { get; set; }

    /// <summary>
    /// Gibt an, wie lange es dauert, bis der Effekt der Karte ausgelöst wird.
    /// Werte können z. B. "instant" oder eine Zeit in Sekunden (als Text) sein.
    /// </summary>
    public string CastTime { get; set; } = string.Empty;   // e.g. "instant" or seconds

    /// <summary>
    /// Gibt an, wie lange der Effekt der Karte aktiv bleibt.
    /// Werte können "instant", "permanent" oder eine Zeit in Sekunden (als Text) sein.
    /// </summary>
    public string Duration { get; set; } = string.Empty;   // e.g. "instant", "permanent" or seconds

    /// <summary>
    /// Beschreibt das Zielmuster oder den Zieltyp der Karte.
    /// Beispiele: "1x1", "ship", "battlefield", "random 1x1".
    /// </summary>
    public CardTarget Target { get; set; } = new(string.Empty);      // e.g. "1x1", "ship", "battlefield", etc.
}