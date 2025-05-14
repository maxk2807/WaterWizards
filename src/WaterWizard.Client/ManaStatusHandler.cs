namespace WaterWizard.Client;

/// <summary>
/// Verarbeitet eingehende Mana-Updates vom Server.
/// </summary>
public class ManaStatusHandler
{
    public int CurrentMana { get; private set; } = 0;

    /// <summary>
    /// Wird aufgerufen, wenn vom Server eine ManaUpdate-Nachricht eingeht.
    /// </summary>
    /// <param name="newMana">Der neue Mana-Wert vom Server</param>
    public void HandleManaUpdate(int newMana)
    {
        CurrentMana = newMana;
        Console.WriteLine($"[ManaStatusHandler] Neuer Mana-Wert: {CurrentMana}");
        // Optional: UI-Aktualisierung oder Weiterleitung an Spielzustand
    }
}