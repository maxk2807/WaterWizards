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

    public ManaUpdateService(GameDataLogger logger, NetManager server)
    {
        _logger = logger;
        _server = server;
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

                var peer = _server.ConnectedPeerList.FirstOrDefault(p => p.EndPoint.ToString().Contains(playerName));
                if (peer == null)
                    continue; // Spieler nicht verbunden

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
            }
        }
    }
}