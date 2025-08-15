// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 5 Zeilen
// - jdewi001: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Shared;

/// <summary>
/// Repräsentiert eine einzelne Zelle auf dem Spielfeld mit ihrem aktuellen Zustand.
/// </summary>
public class Cell(CellState cellState)
{
    /// <summary>
    /// Der aktuelle Zustand der Zelle (z.B. leer, Schiff, getroffen, etc.).
    /// </summary>
    public CellState CellState { get; set; } = cellState;
}
