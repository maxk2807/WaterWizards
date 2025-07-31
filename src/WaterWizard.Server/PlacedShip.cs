// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 21 Zeilen
// - maxk2807: 7 Zeilen
// - Erickk0: 2 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Shared.ShipType;

namespace WaterWizard.Server;

/// <summary>
/// Repräsentiert ein platziertes Schiff auf dem Spielfeld.
/// </summary>
public class PlacedShip
{
    /// <summary>
    /// X-Position des Schiffs auf dem Spielfeld.
    /// </summary>
    public int X { get; set; }
    /// <summary>
    /// Y-Position des Schiffs auf dem Spielfeld.
    /// </summary>
    public int Y { get; set; }
    /// <summary>
    /// Breite des Schiffs (in Feldern).
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// Höhe des Schiffs (in Feldern).
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Maximale Lebenspunkte des Schiffs (Width * Height).
    /// </summary>
    public int MaxHealth => Width * Height;
    /// <summary>
    /// Menge der beschädigten Zellen des Schiffs.
    /// </summary>
    public HashSet<(int X, int Y)> DamagedCells { get; set; } = new();
    /// <summary>
    /// Gibt an, ob das Schiff zerstört ist (alle Zellen beschädigt).
    /// </summary>
    public bool IsDestroyed => DamagedCells.Count >= MaxHealth;

    /// <summary>
    /// Repräsentiert ein platziertes Schiff auf dem Spielfeld.
    /// </summary>
    public ShipType ShipType { get; set; } = ShipType.DEFAULT;

    /// <summary>
    /// Prüft, ob eine bestimmte Zelle beschädigt ist.
    /// </summary>
    /// <param name="cellX">X-Koordinate der Zelle</param>
    /// <param name="cellY">Y-Koordinate der Zelle</param>
    /// <returns>True, wenn die Zelle beschädigt ist</returns>
    public bool IsCellDamaged(int cellX, int cellY) => DamagedCells.Contains((cellX, cellY));

    /// <summary>
    /// Markiert eine Zelle als beschädigt.
    /// </summary>
    /// <param name="cellX">X-Koordinate der Zelle</param>
    /// <param name="cellY">Y-Koordinate der Zelle</param>
    /// <returns>True, wenn die Zelle neu beschädigt wurde</returns>
    public bool DamageCell(int cellX, int cellY)
    {
        return DamagedCells.Add((cellX, cellY));
    }

    /// <summary>
    /// Heilt eine beschädigte Zelle.
    /// </summary>
    /// <param name="cellX">X-Koordinate der Zelle</param>
    /// <param name="cellY">Y-Koordinate der Zelle</param>
    /// <returns>True, wenn die Zelle geheilt wurde</returns>
    public bool HealCell(int cellX, int cellY)
    {
        return DamagedCells.Remove((cellX, cellY));
    }
}
