// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 11 Zeilen
// - Erickk0: 2 Zeilen
// - jdewi001: 1 Zeilen
// - erick: 1 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Shared;

/// <summary>
/// Beschreibt die möglichen Zustände einer Zelle auf dem Spielfeld.
/// </summary>
public enum CellState
{
    /// <summary>
    /// Die Zelle ist leer.
    /// </summary>
    Empty,
    /// <summary>
    /// Die Zelle enthält ein Schiff.
    /// </summary>
    Ship,
    /// <summary>
    /// Die Zelle enthält einen Felsen.
    /// </summary>
    Rock,
    /// <summary>
    /// Die Zelle wurde getroffen.
    /// </summary>
    Hit,
    /// <summary>
    /// Die Zelle wurde verfehlt.
    /// </summary>
    Miss,
    /// <summary>
    /// Der Zustand der Zelle ist unbekannt.
    /// </summary>
    Unknown,
    /// <summary>
    /// Die Zelle ist von einem Blitzeffekt betroffen.
    /// </summary>
    Thunder,
    /// <summary>
    /// Die Zelle wurde durch das "Hovering Eye" aufgedeckt.
    /// </summary>
    HoveringEyeRevealed,
    /// <summary>
    /// Die Zelle ist durch einen Schild geschützt.
    /// </summary>
    Shield
}
