// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Erickk0: 43 Zeilen
// - erick: 5 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;
using System.Numerics;

namespace WaterWizard.Shared;

/// <summary>
/// Repräsentiert einen aktiven Schild-Effekt auf dem Spielfeld.
/// </summary>
public class ShieldEffect
{
    /// <summary>
    /// X-Position des Schilds.
    /// </summary>
    public int X { get; set; }
    /// <summary>
    /// Y-Position des Schilds.
    /// </summary>
    public int Y { get; set; }
    /// <summary>
    /// Dauer des Schilds in Sekunden.
    /// </summary>
    public float Duration { get; set; }
}
