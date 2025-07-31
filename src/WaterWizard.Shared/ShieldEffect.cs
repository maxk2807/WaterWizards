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
    public Vector2 Position { get; }
    public int PlayerIndex { get; }
    public float RemainingDuration { get; private set; }
    public bool IsActive => RemainingDuration > 0;

    public ShieldEffect(Vector2 position, int playerIndex, float duration)
    {
        Position = position;
        PlayerIndex = playerIndex;
        RemainingDuration = duration;
    }

    public void Update(float deltaTime)
    {
        if (RemainingDuration > 0)
        {
            RemainingDuration -= deltaTime;
            if (RemainingDuration <= 0)
            {
                RemainingDuration = 0;
            }
        }
    }

    /// <summary>
    /// Dauer des Schilds in Sekunden.
    /// </summary>
    /// <param name="x">X coordinate to check</param>
    /// <param name="y">Y coordinate to check</param>
    /// <returns>True if the coordinates are within the shield's 3x3 area</returns>
    public bool IsCoordinateProtected(int x, int y)
    {
        if (!IsActive)
            return false;

        bool isProtected =
            x >= Position.X - 1
            && x <= Position.X + 1
            && y >= Position.Y - 1
            && y <= Position.Y + 1;

        Console.WriteLine(
            $"[ShieldEffect] Checking protection at ({x}, {y}) vs center ({Position.X}, {Position.Y}): {(isProtected ? "PROTECTED" : "NOT PROTECTED")}"
        );
        return isProtected;
    }
}
