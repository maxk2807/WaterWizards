// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 32 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;

namespace WaterWizard.Shared;

/// <summary>
/// Repräsentiert eine eindeutige Sitzungs-ID für ein Spiel.
/// </summary>
public class GameSessionId
{
    /// <summary>
    /// Die eindeutige Sitzungs-ID-Zeichenfolge (GUID).
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Erstellt eine neue eindeutige Sitzungs-ID.
    /// </summary>
    public GameSessionId()
    {
        SessionId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Erstellt eine Sitzungs-ID aus einer vorhandenen Zeichenfolge (z.B. von Server empfangen).
    /// </summary>
    public GameSessionId(string sessionId)
    {
        SessionId = sessionId;
    }

    public override string ToString() => SessionId;
}
