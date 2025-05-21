using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.Logging;
using WaterWizard.Server.Models;

namespace WaterWizard.Server.Services;

/// <summary>
/// Führt Mana-Logik für aktive Spieler aus.
/// </summary>
public class ManaUpdateService
{
    private readonly GameDataLogger _logger;
    private readonly NetManager _server;
    private readonly int _manaIncrease = 10; // Konstante Erhöhung

    private readonly Dictionary<string, NetPeer> _playerPeers;

    public ManaUpdateService(GameDataLogger logger, NetManager server, Dictionary<string, NetPeer> playerPeers)
    {
        _logger = logger;
        _server = server;
        _playerPeers = playerPeers;
    }
    
    /// <summary>
    /// Aktualisiert Mana aller verbundenen Spieler.
    /// </summary>
    public void UpdateAllPlayers()
    {
        foreach (var session in _logger.GetSessions().Values)
        {
            foreach (var playerEntry in session.Players)
            {
                string playerName = playerEntry.Key;
                PlayerHistory history = playerEntry.Value;

                if (!_playerPeers.TryGetValue(playerName, out var peer))
                    continue;

                var lastEntry = history.Entries.LastOrDefault();
                if (lastEntry == null)
                    continue;

                // Mana erhöhen
                int newMana = lastEntry.Mana + _manaIncrease;

                // Neuen Zustand loggen
                _logger.LogPlayerData(
                    session.SessionId,
                    playerName,
                    newMana,
                    lastEntry.Gold,
                    new List<string>(lastEntry.Cards),
                    new List<string>(lastEntry.ShipsInStock)
                );

                // An Spieler senden
                var writer = new NetDataWriter();
                writer.Put("ManaUpdate");
                writer.Put(newMana);
                peer.Send(writer, DeliveryMethod.Unreliable);
                Console.WriteLine($"[Server] Mana für Spieler '{playerName}' auf {newMana} erhöht und gesendet.");
            }
        }
    }
}