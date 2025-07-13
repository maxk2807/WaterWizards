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
/// Provides a unique session ID for each game session.
/// </summary>
public class GameSessionId
{
    /// <summary>
    /// The unique session ID string (GUID).
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Creates a new unique session ID.
    /// </summary>
    public GameSessionId()
    {
        SessionId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Creates a session ID from an existing string (e.g., received from server).
    /// </summary>
    public GameSessionId(string sessionId)
    {
        SessionId = sessionId;
    }

    public override string ToString() => SessionId;
}
