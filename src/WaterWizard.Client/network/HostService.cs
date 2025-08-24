// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 349 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.gamescreen.handler;
using WaterWizard.Shared;

namespace WaterWizard.Client.network;

/// <summary>
/// Verwaltet das Hosting einer Spiel-Lobby, die Verwaltung der verbundenen Spieler und die Server-Events.
/// </summary>
public class HostService(NetworkManager manager)
{
    private NetManager? server;
    private EventBasedNetListener? serverListener;
    /// <summary>
    /// Gibt die aktuell verbundenen Spieler zurück.
    /// </summary>
    public List<Player> ConnectedPlayers { get; private set; } = [];

    private GameSessionId? sessionId;
    /// <summary>
    /// Gibt die aktuelle SessionId zurück.
    /// </summary>
    public GameSessionId? SessionId => sessionId;

    /// <summary>
    /// Startet einen Spielserver auf dem lokalen Rechner mit dem konfigurierten Port.
    /// Initialisiert die Netzwerkkomponenten und registriert Event-Handler für Clientverbindungen.
    /// </summary>
    public void StartHosting()
    {
        try
        {
            manager.clientService.CleanupIfRunning();
            manager.discoveredLobbies.Clear();

            serverListener = new EventBasedNetListener();
            server = new NetManager(serverListener)
            {
                AutoRecycle = true,
                UnconnectedMessagesEnabled = true,
            };

            if (!server.Start(manager.hostPort))
            {
                Console.WriteLine("Server konnte nicht gestartet werden!");
                return;
            }

            ConnectedPlayers.Clear();
            ConnectedPlayers.Add(new Player("Host") { Name = "Host (You)", IsReady = false });

            sessionId = new GameSessionId();

            Console.WriteLine($"Server gestartet auf Port {manager.hostPort}");
            SetupServerEventHandlers();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Hosten: {ex.Message}");
        }
    }

    /// <summary>
    /// Registriert alle relevanten Event-Handler für den Server (Verbindungen, Nachrichten, Disconnects).
    /// </summary>
    private void SetupServerEventHandlers()
    {
        if (serverListener == null)
            return;

        serverListener.ConnectionRequestEvent += request => request.Accept();

        serverListener.NetworkReceiveUnconnectedEvent += HandleUnconnectedMessage;

        serverListener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"Client {peer} verbunden");

            string playerAddress = peer.ToString();
            string playerName =
                $"Player_{playerAddress.Split(':').LastOrDefault() ?? playerAddress}";

            if (!PlayerExists(playerAddress))
            {
                ConnectedPlayers.Add(new Player(playerAddress) { Name = playerName });
            }

            var writer = new NetDataWriter();
            writer.Put("EnterLobby");
            writer.Put(sessionId != null ? sessionId.ToString() : string.Empty);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            BroadcastSystemMessage($"{playerName} connected.");
            UpdatePlayerList();
        };

        serverListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"Client {peer} getrennt: {disconnectInfo.Reason}");

            string playerAddress = peer.ToString();
            string playerName =
                ConnectedPlayers.FirstOrDefault(p => p.Address == playerAddress)?.Name
                ?? $"Player_{playerAddress.Split(':').LastOrDefault()}";

            RemovePlayerByAddress(playerAddress);
            manager.LobbyCountdownSeconds = null;
            BroadcastSystemMessage($"{playerName} disconnected ({disconnectInfo.Reason}).");
            UpdatePlayerList();
        };

        serverListener.NetworkReceiveEvent += HandleServerReceiveEvent;
    }

    /// <summary>
    /// Verarbeitet unverbundene Nachrichten (z. B. Lobby-Suchanfragen) von Clients.
    /// </summary>
    /// <param name="remoteEndPoint">Der Absender-Endpunkt.</param>
    /// <param name="reader">Reader für die empfangene Nachricht.</param>
    /// <param name="msgType">Typ der unverbundenen Nachricht.</param>
    private void HandleUnconnectedMessage(
        IPEndPoint remoteEndPoint,
        NetPacketReader reader,
        UnconnectedMessageType msgType
    )
    {
        try
        {
            string message = reader.GetString();
            if (message == "DiscoverLobbies")
            {
                Console.WriteLine($"[Host] Lobby-Suchanfrage von {remoteEndPoint} erhalten");

                var response = new NetDataWriter();
                response.Put("LobbyInfo");
                response.Put("WaterWizards Lobby");
                response.Put(ConnectedPlayers.Count);
                server?.SendUnconnectedMessage(response, remoteEndPoint);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[Host] Fehler bei Verarbeitung unverbundener Nachricht: {ex.Message}"
            );
        }
    }

    private bool PlayerExists(string address)
    {
        return ConnectedPlayers.Any(p => p.Address == address);
    }

    private void RemovePlayerByAddress(string address)
    {
        ConnectedPlayers.RemoveAll(p => p.Address == address);
    }

    /// <summary>
    /// Prüft, ob der Server aktuell läuft.
    /// </summary>
    /// <returns>True, wenn der Server läuft</returns>
    public bool IsRunning()
    {
        return server != null && server.IsRunning;
    }

    /// <summary>
    /// Zentraler Empfangs-Handler für verbundene Clients.
    /// Leitet Nachrichten anhand ihres Typs an die zuständigen Handler weiter.
    /// </summary>
    /// <param name="peer">Der sendende Client-Peer.</param>
    /// <param name="reader">Reader für die empfangenen Daten.</param>
    /// <param name="channelNumber">Genutzter Übertragungskanal.</param>
    /// <param name="deliveryMethod">Zustellmethode (z. B. ReliableOrdered).</param>
    private void HandleServerReceiveEvent(
        NetPeer peer,
        NetPacketReader reader,
        byte channelNumber,
        DeliveryMethod deliveryMethod
    )
    {
        try
        {
            string messageType = reader.GetString();
            Console.WriteLine($"[Host] Nachricht von Client {peer} erhalten: {messageType}");

            switch (messageType)
            {
                case "PlayerReady":
                case "PlayerNotReady":
                    HandlePlayerReadyStatus(peer, messageType == "PlayerReady");
                    break;
                case "ChatMessage":
                    reader.GetString();
                    break;
                case "LobbyCountdown":
                    manager.HandleLobbyCountdown(reader);
                    break;
                case "PlayerJoin":
                    string playerName = reader.GetString();
                    LobbyHandler.HandlePlayerJoin(
                        peer,
                        playerName,
                        ConnectedPlayers,
                        UpdatePlayerList
                    );
                    break;
                default:
                    Console.WriteLine(
                        $"[Host] Unbekannter Nachrichtentyp empfangen: {messageType}"
                    );
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Host] Fehler beim Verarbeiten der Nachricht: {ex.Message}");
        }
        finally
        {
            reader.Recycle();
        }
    }

    /// <summary>
    /// Verarbeitet den Ready- oder Not-Ready-Status eines Spielers.
    /// </summary>
    /// <param name="peer">Peer des Spielers.</param>
    /// <param name="isReady">True, wenn der Spieler bereit ist, sonst False.</param>
    private void HandlePlayerReadyStatus(NetPeer peer, bool isReady)
    {
        var player = ConnectedPlayers.FirstOrDefault(p => p.Address == peer.ToString());
        if (player != null)
        {
            player.IsReady = isReady;
            Console.WriteLine(
                $"[Host] Spieler {player.Name} ist jetzt {(isReady ? "bereit" : "nicht bereit")}"
            );
            UpdatePlayerList();
        }
        else
        {
            Console.WriteLine($"[Host] Spieler mit Adresse {peer} nicht gefunden!");
        }
    }

    /// <summary>
    /// Aktualisiert die Spielerliste und sendet sie an alle verbundenen Clients.
    /// </summary>
    private void UpdatePlayerList()
    {
        if (server == null)
            return;

        var writer = new NetDataWriter();
        writer.Put("PlayerList");
        writer.Put(ConnectedPlayers.Count);

        foreach (var player in ConnectedPlayers)
        {
            writer.Put(player.Address);
            writer.Put(player.Name);
            writer.Put(player.IsReady);
            Console.WriteLine(
                $"[Host] Spieler: {player.Name}, Status: {(player.IsReady ? "bereit" : "nicht bereit")}"
            );
        }

        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        Console.WriteLine($"[Host] Spielerliste mit {ConnectedPlayers.Count} Spielern gesendet.");
    }

    /// <summary>
    /// Sendet eine Systemnachricht an alle verbundenen Clients und schreibt sie ins ChatLog.
    /// </summary>
    /// <param name="message">Die Systemnachricht.</param>
    private void BroadcastSystemMessage(string message)
    {
        if (server == null)
            return;
        var writer = new NetDataWriter();
        writer.Put("SystemMessage");
        writer.Put(message);
        server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        GameStateManager.Instance.ChatLog.AddMessage($"[System] {message}");
    }

    /// <summary>
    /// Sendet eine Chatnachricht eines Spielers an alle verbundenen Clients und schreibt sie ins ChatLog.
    /// </summary>
    /// <param name="senderName">Name des Senders.</param>
    /// <param name="message">Die Chatnachricht.</param>
    private void BroadcastChatMessage(string senderName, string message)
    {
        if (server == null)
            return;
        var writer = new NetDataWriter();
        writer.Put("ChatMessage");
        writer.Put(senderName);
        writer.Put(message);
        server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        GameStateManager.Instance.ChatLog.AddMessage($"{senderName}: {message}");
    }

    /// <summary>
    /// Sendet eine Nachricht an alle verbundenen Clients.
    /// </summary>
    /// <param name="message">Die zu sendende Nachricht</param>
    public void SendToAllClients(string message)
    {
        if (server == null)
            return;

        var writer = new NetDataWriter();
        writer.Put(message);

        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    /// <summary>
    /// Startet das Spiel für alle verbundenen Clients, wenn alle bereit sind.
    /// </summary>
    public void BroadcastStartGame()
    {
        if (!IsRunning() || server == null)
        {
            Console.WriteLine("[NetworkManager] Only the host can start the game.");
            return;
        }

        if (!ConnectedPlayers.All(p => p.IsReady))
        {
            Console.WriteLine("[NetworkManager] Cannot start game, not all players are ready.");
            BroadcastSystemMessage("Cannot start game, not all players are ready.");
            return;
        }

        var writer = new NetDataWriter();
        writer.Put("StartGame");
        server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        Console.WriteLine("[Host] Sent StartGame command to all clients.");

        GameStateManager.Instance.SetStateToInGame();
    }

    /// <summary>
    /// Prüft, ob Spieler verbunden sind.
    /// </summary>
    /// <returns>True, wenn mindestens ein Spieler verbunden ist</returns>
    public bool ArePlayersConnected() =>
        server != null && server.IsRunning && server.ConnectedPeersCount > 0;

    /// <summary>
    /// Verarbeitet Netzwerkereignisse (Empfang/Senden von Nachrichten).
    /// </summary>
    public void PollEvents()
    {
        server?.PollEvents();
    }

    /// <summary>
    /// Beendet den Server und entfernt alle Spieler.
    /// </summary>
    public void Shutdown()
    {
        server?.Stop();
        ConnectedPlayers.Clear();
    }

    /// <summary>
    /// Fügt die lokale Lobby zur Liste der entdeckten Lobbies hinzu.
    /// </summary>
    public void AddLocalLobbyToDiscoveredList()
    {
        string localIpAddress = NetworkUtils.GetLocalIPAddress();
        var existingLocalLobby = manager.discoveredLobbies.FirstOrDefault(l =>
            l.IP.Contains("127.0.0.1")
            || l.IP.Contains("localhost")
            || l.IP.Contains(localIpAddress)
        );

        if (existingLocalLobby == null)
        {
            manager.discoveredLobbies.Add(
                new LobbyInfo(
                    $"{localIpAddress}:{manager.hostPort}",
                    "WaterWizards Lobby (Lokal)",
                    ConnectedPlayers.Count
                )
            );
            Console.WriteLine("[Client] Lokale Lobby manuell zur Liste hinzugefügt");
        }
    }
}
