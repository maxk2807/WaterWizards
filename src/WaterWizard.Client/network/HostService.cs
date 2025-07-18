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
using WaterWizard.Shared;

namespace WaterWizard.Client.network;

public class HostService(NetworkManager manager)
{
    private NetManager? server;
    private EventBasedNetListener? serverListener;
    public List<Player> ConnectedPlayers { get; private set; } = [];

    private GameSessionId? sessionId;
    public GameSessionId? SessionId => sessionId;

    /// <summary>
    /// Startet einen Spielserver auf dem lokalen Rechner mit dem konfigurierten Port.
    /// Initialisiert die Netzwerkkomponenten und registriert Event-Handler fuer Clientverbindungen.
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

    public bool IsRunning()
    {
        return server != null && server.IsRunning;
    }

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
                    string playerName = reader.GetString(); // Name sent by the client
                    var playerToUpdate = ConnectedPlayers.FirstOrDefault(p =>
                        p.Address == peer.ToString()
                    );
                    if (playerToUpdate != null)
                    {
                        playerToUpdate.Name = playerName;
                        UpdatePlayerList(); // Broadcast the updated player list to all clients
                    }
                    else
                    {
                        // This might indicate an unexpected state, e.g., PlayerJoin from an unrecognized peer.
                        Console.WriteLine(
                            $"[Host] PlayerJoin: Player with address {peer} not found in connectedPlayers. Name received: {playerName}"
                        );
                        // Optionally, handle this by adding the player if it's a valid scenario,
                        // though players are typically added during PeerConnectedEvent.
                        // connectedPlayers.Add(new Player(peer.ToString()) { Name = playerName, IsReady = false });
                        // UpdatePlayerList();
                    }
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

    public bool ArePlayersConnected() =>
        server != null && server.IsRunning && server.ConnectedPeersCount > 0;

    public void PollEvents()
    {
        server?.PollEvents();
    }

    public void Shutdown()
    {
        server?.Stop();
        ConnectedPlayers.Clear();
    }

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
