// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 45 Zeilen
// - Erickk0: 5 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - private static readonly ConcurrentDictionary<string, Lobby> Lobbies = new();   (erick: 31 Zeilen)
// - private readonly Dictionary<LiteNetLib.NetPeer, string> players = new();   (erick: 1 Zeilen)
// - private readonly HashSet<LiteNetLib.NetPeer> readyPlayers = new();   (erick: 6 Zeilen)
// ===============================================

using System.Collections.Concurrent;
using WaterWizard.Shared;

namespace WaterWizard.Server;

public static class LobbyManager
{
    private static readonly ConcurrentDictionary<string, Lobby> Lobbies = new();

    /// <summary>
    /// Creates a new lobby and logs its session ID.
    /// /// </summary>
    /// <returns>The created lobby.</returns>
    public static Lobby CreateLobby()
    {
        var sessionId = new GameSessionId();
        var lobby = new Lobby(sessionId);
        Lobbies[sessionId.SessionId] = lobby;
        Console.WriteLine($"[Server] Created new lobby with SessionId: {sessionId}");
        return lobby;
    }

    public static Lobby? GetLobby(string sessionId) =>
        Lobbies.TryGetValue(sessionId, out var lobby) ? lobby : null;

    public static IEnumerable<Lobby> GetAllLobbies() => Lobbies.Values;

    /// <summary>
    /// Logs all active lobbies.
    /// /// </summary>
    public static void LogAllLobbies()
    {
        foreach (var lobby in LobbyManager.GetAllLobbies())
        {
            Console.WriteLine($"Lobby SessionId: {lobby.SessionId}");
        }
    }
}

public class Lobby
{
    public GameSessionId SessionId { get; }
    private readonly Dictionary<LiteNetLib.NetPeer, string> players = new();
    private readonly HashSet<LiteNetLib.NetPeer> readyPlayers = new();

    public Lobby(GameSessionId sessionId)
    {
        SessionId = sessionId;
    }
}
