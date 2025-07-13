// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 34 Zeilen
// - Erickk0: 1 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;
using WaterWizard.Shared;

namespace WaterWizard.Server;

/// <summary>
/// Verwaltet die Erstellung und das Logging der GameSessionId auf Serverseite.
/// </summary>
public static class GameSessionIdManager
{
    private static GameSessionId? _sessionId;

    /// <summary>
    /// Erstellt eine neue GameSessionId und loggt sie.
    /// </summary>
    public static void CreateAndLogSessionId()
    {
        _sessionId = new GameSessionId();
        Log($"[Server] Created new GameSessionId: {_sessionId}");
    }

    /// <summary>
    /// Gibt die aktuelle GameSessionId zurück.
    /// </summary>
    public static GameSessionId? SessionId => _sessionId;

    /// <summary>
    /// Loggt die aktuelle GameSessionId.
    /// </summary>
    /// <param name="message">Die zu loggende Nachricht</param>
    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }
}
