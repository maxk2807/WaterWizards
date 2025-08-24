// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 6 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Client.gamestates;

/// <summary>
/// Definiert das gemeinsame Verhalten für alle Spielzustände.
/// Jeder Zustand muss eine Methode zur Aktualisierung und Darstellung
/// seiner Inhalte implementieren.
/// </summary>
public interface IGameState
{
    /// <summary>
    /// Aktualisiert den Spielzustand und rendert dessen Darstellung
    /// für den aktuellen Frame.
    /// </summary>
    /// <param name="manager">Der GameStateManager, der Zustände und Bildschirmgrößen verwaltet.</param>
    void UpdateAndDraw(GameStateManager manager);
}
