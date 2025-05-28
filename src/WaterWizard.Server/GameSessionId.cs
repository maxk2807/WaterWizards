using System;
using WaterWizard.Shared;

namespace WaterWizard.Server;

/// <summary>
/// Responsible for creating and logging the server-side GameSessionId.
/// </summary>
public static class GameSessionIdManager
{
    private static GameSessionId? _sessionId;

    /// <summary>
    /// Creates a new GameSessionId and logs it.
    /// </summary>
    public static void CreateAndLogSessionId()
    {
        _sessionId = new GameSessionId();
        Log($"[Server] Created new GameSessionId: {_sessionId}");
    }

    /// <summary>
    /// Gets the current GameSessionId.
    /// </summary>
    public static GameSessionId? SessionId => _sessionId;

    /// <summary>
    /// Logs the current GameSessionId.
    /// </summary>
    /// <param name="message">The message to log.</param>
    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }
}
