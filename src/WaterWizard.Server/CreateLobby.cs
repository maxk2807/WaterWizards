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

/// <summary>
/// Verwaltet die Erstellung und Verwaltung von Lobbies auf dem Server.
/// </summary>
public static class LobbyManager
{
    private static readonly ConcurrentDictionary<string, Lobby> Lobbies = new();

    /// <summary>
    /// Erstellt eine neue Lobby und loggt deren SessionId.
    /// </summary>
    /// <returns>Die erstellte Lobby</returns>
    public static Lobby CreateLobby()
    {
        var sessionId = new GameSessionId();
        var lobby = new Lobby(sessionId);
        Lobbies[sessionId.SessionId] = lobby;
        Console.WriteLine($"[Server] Created new lobby with SessionId: {sessionId}");
        return lobby;
    }

    /// <summary>
    /// Gibt eine Lobby anhand der SessionId zurück.
    /// </summary>
    public static Lobby? GetLobby(string sessionId) =>
        Lobbies.TryGetValue(sessionId, out var lobby) ? lobby : null;

    /// <summary>
    /// Gibt alle aktiven Lobbies zurück.
    /// </summary>
    public static IEnumerable<Lobby> GetAllLobbies() => Lobbies.Values;

    /// <summary>
    /// Loggt alle aktiven Lobbies.
    /// </summary>
    public static void LogAllLobbies()
    {
        foreach (var lobby in LobbyManager.GetAllLobbies())
        {
            Console.WriteLine($"Lobby SessionId: {lobby.SessionId}");
        }
    }
}

/// <summary>
/// Repräsentiert eine Spiel-Lobby mit SessionId und Spielerlisten.
/// </summary>
public class Lobby
{
    /// <summary>
    /// Die SessionId der Lobby.
    /// </summary>
    public GameSessionId SessionId { get; }
    private readonly Dictionary<LiteNetLib.NetPeer, string> players = new();
    private readonly HashSet<LiteNetLib.NetPeer> readyPlayers = new();

    public Lobby(GameSessionId sessionId)
    {
        SessionId = sessionId;
    }
}
