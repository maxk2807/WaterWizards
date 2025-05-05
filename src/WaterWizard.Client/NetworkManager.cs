using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using WaterWizard.Shared;

namespace WaterWizard.Client;

public class NetworkManager
{
    private static NetworkManager? instance;
    public static NetworkManager Instance => instance ??= new NetworkManager();

    private List<Player> connectedPlayers = new List<Player>();
    private List<LobbyInfo> discoveredLobbies = new List<LobbyInfo>();


    private NetManager? server;
    private NetManager? client;
    private EventBasedNetListener? serverListener;
    private EventBasedNetListener? clientListener;
    private bool isPlayerConnected = false;
    private int hostPort = 7777;

    private bool clientReady = false;

    private NetworkManager() { }

    /// <summary>
    /// Startet einen Spielserver auf dem lokalen Rechner mit dem konfigurierten Port.
    /// Initialisiert die Netzwerkkomponenten und registriert Event-Handler fuer Clientverbindungen.
    /// </summary>
    public void StartHosting()
    {
        try
        {
            CleanupClientIfRunning();
            discoveredLobbies.Clear();

            serverListener = new EventBasedNetListener();
            server = new NetManager(serverListener)
            {
                AutoRecycle = true,
                UnconnectedMessagesEnabled = true
            };

            if (!server.Start(hostPort))
            {
                Console.WriteLine("Server konnte nicht gestartet werden!");
                return;
            }

            connectedPlayers.Clear();
            connectedPlayers.Add(new Player("Host") { Name = "Host (You)", IsReady = true });

            Console.WriteLine($"Server gestartet auf Port {hostPort}");
            SetupServerEventHandlers();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Hosten: {ex.Message}");
        }
    }

    private void SetupServerEventHandlers()
    {
        if (serverListener == null) return;

        serverListener.ConnectionRequestEvent += request => request.Accept();

        serverListener.NetworkReceiveUnconnectedEvent += HandleUnconnectedMessage;

        serverListener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"Client {peer} verbunden");

            string playerAddress = peer.ToString();
            string playerName = $"Player_{playerAddress.Split(':').LastOrDefault() ?? playerAddress}";

            if (!PlayerExists(playerAddress))
            {
                connectedPlayers.Add(new Player(playerAddress) { Name = playerName });
            }

            var writer = new NetDataWriter();
            writer.Put("EnterLobby");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            BroadcastSystemMessage($"{playerName} connected.");
            UpdatePlayerList();
        };

        serverListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"Client {peer} getrennt: {disconnectInfo.Reason}");

            string playerAddress = peer.ToString();
            string playerName = connectedPlayers.FirstOrDefault(p => p.Address == playerAddress)?.Name ??
                               $"Player_{playerAddress.Split(':').LastOrDefault()}";

            RemovePlayerByAddress(playerAddress);
            BroadcastSystemMessage($"{playerName} disconnected ({disconnectInfo.Reason}).");
            UpdatePlayerList();
        };

        serverListener.NetworkReceiveEvent += HandleServerReceiveEvent;
    }

    private void HandleUnconnectedMessage(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType msgType)
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
                response.Put(connectedPlayers.Count);
                server?.SendUnconnectedMessage(response, remoteEndPoint);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Host] Fehler bei Verarbeitung unverbundener Nachricht: {ex.Message}");
        }
    }

    private void HandleServerReceiveEvent(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
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
                    string senderName = reader.GetString();
                    string chatMsg = reader.GetString();
                    GameStateManager.Instance.ChatLog.AddMessage($"{senderName}: {chatMsg}");
                    break;
                default:
                    Console.WriteLine($"[Host] Unbekannter Nachrichtentyp empfangen: {messageType}");
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
        var player = connectedPlayers.FirstOrDefault(p => p.Address == peer.ToString());
        if (player != null)
        {
            player.IsReady = isReady;
            Console.WriteLine($"[Host] Spieler {player.Name} ist jetzt {(isReady ? "bereit" : "nicht bereit")}");
            UpdatePlayerList();
        }
        else
        {
            Console.WriteLine($"[Host] Spieler mit Adresse {peer} nicht gefunden!");
        }
    }

    private bool PlayerExists(string address)
    {
        return connectedPlayers.Any(p => p.Address == address);
    }

    private void RemovePlayerByAddress(string address)
    {
        connectedPlayers.RemoveAll(p => p.Address == address);
    }

    public bool IsHost()
    {
        return server != null && server.IsRunning;
    }

    private void UpdatePlayerList()
    {
        if (server == null) return;

        var writer = new NetDataWriter();
        writer.Put("PlayerList");
        writer.Put(connectedPlayers.Count);

        foreach (var player in connectedPlayers)
        {
            writer.Put(player.Address);
            writer.Put(player.Name);
            writer.Put(player.IsReady);
            Console.WriteLine($"[Host] Spieler: {player.Name}, Status: {(player.IsReady ? "bereit" : "nicht bereit")}");
        }

        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        Console.WriteLine($"[Host] Spielerliste mit {connectedPlayers.Count} Spielern gesendet.");
    }

    /// <summary>
    /// Starts discovering available server lobbies on the network.
    /// </summary>
    public void DiscoverLobbies()
    {
        if (IsHost())
        {
            Console.WriteLine("[Client] Suche nach entfernten Lobbies (lokale Lobby wird ausgeblendet)...");
        }
        else
        {
            discoveredLobbies.Clear();
            Console.WriteLine("[Client] Suche nach verfügbaren Lobbies...");
        }

        InitializeClientForDiscovery();
        SendDiscoveryRequests();
    }

    private void InitializeClientForDiscovery()
    {
        CleanupClientIfRunning();

        clientListener = new EventBasedNetListener();
        client = new NetManager(clientListener)
        {
            UnconnectedMessagesEnabled = true
        };

        if (!client.Start())
        {
            Console.WriteLine("[Client] Fehler beim Starten des Discovery-Clients");
            return;
        }

        clientListener.NetworkReceiveUnconnectedEvent += HandleLobbyInfoResponse;
    }

    private void CleanupClientIfRunning()
    {
        if (client != null && client.IsRunning)
        {
            client.Stop();
            client = null;
        }
    }

    private void HandleLobbyInfoResponse(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        try
        {
            string msgType = reader.GetString();
            if (msgType == "LobbyInfo")
            {
                string lobbyName = reader.GetString();
                int playerCount = reader.GetInt();

                bool isLocalAddress = remoteEndPoint.Address.Equals(IPAddress.Loopback) ||
                                     remoteEndPoint.Address.ToString() == NetworkUtils.GetLocalIPAddress();

                if (IsHost() && isLocalAddress)
                {
                    Console.WriteLine($"[Client] Eigene Lobby gefunden und ignoriert: '{lobbyName}' auf {remoteEndPoint}");
                    return;
                }

                Console.WriteLine($"[Client] Lobby gefunden: '{lobbyName}' mit {playerCount} Spieler(n) auf {remoteEndPoint}");

                var existingLobby = discoveredLobbies.FirstOrDefault(l => l.IP == remoteEndPoint.ToString());
                if (existingLobby != null)
                {
                    existingLobby.Name = lobbyName;
                    existingLobby.PlayerCount = playerCount;
                }
                else
                {
                    discoveredLobbies.Add(new LobbyInfo(remoteEndPoint.ToString(), lobbyName, playerCount));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Fehler beim Verarbeiten der Lobby-Info: {ex.Message}");
        }
    }

    private void SendDiscoveryRequests()
    {
        if (client == null || !client.IsRunning) return;

        var req = new NetDataWriter();
        req.Put("DiscoverLobbies");

        client.SendBroadcast(req, hostPort);

        try
        {
            client.SendUnconnectedMessage(req, new IPEndPoint(IPAddress.Loopback, hostPort));
            Console.WriteLine("[Client] Discovery-Request an localhost gesendet");

            string localIp = NetworkUtils.GetLocalIPAddress();
            if (localIp != "127.0.0.1")
            {
                client.SendUnconnectedMessage(req, new IPEndPoint(IPAddress.Parse(localIp), hostPort));
                Console.WriteLine($"[Client] Discovery-Request an lokale IP {localIp} gesendet");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Fehler beim Senden von gezielten Anfragen: {ex.Message}");
        }

        Console.WriteLine("[Client] Lobby-Suchanfragen gesendet");
    }

    /// <summary>
    /// Returns the list of discovered lobbies.
    /// </summary>
    public List<LobbyInfo> GetDiscoveredLobbies() => discoveredLobbies;

    /// <summary>
    /// Refreshes the list of discovered lobbies.
    /// </summary>
    public void RefreshLobbies()
    {
        if (client != null && client.IsRunning)
        {
            SendDiscoveryRequests();
            Console.WriteLine("[Client] Lobby-Liste aktualisiert");

            if (IsHost())
            {
                AddLocalLobbyToDiscoveredList();
            }
        }
        else
        {
            DiscoverLobbies();
        }
    }

    private void AddLocalLobbyToDiscoveredList()
    {
        string localIpAddress = NetworkUtils.GetLocalIPAddress();
        var existingLocalLobby = discoveredLobbies.FirstOrDefault(l =>
            l.IP.Contains("127.0.0.1") ||
            l.IP.Contains("localhost") ||
            l.IP.Contains(localIpAddress));

        if (existingLocalLobby == null)
        {
            discoveredLobbies.Add(new LobbyInfo($"{localIpAddress}:{hostPort}",
                "WaterWizards Lobby (Lokal)", connectedPlayers.Count));
            Console.WriteLine("[Client] Lokale Lobby manuell zur Liste hinzugefügt");
        }
    }

    public void ConnectToServer(string ip, int port)
    {
        if (client != null && client.FirstPeer != null && client.FirstPeer.ConnectionState == ConnectionState.Connected)
        {
            Console.WriteLine("[Client] Bereits mit dem Server verbunden.");
            return;
        }

        try
        {
            Console.WriteLine($"Versuche, eine Verbindung zum Server herzustellen: IP={ip}, Port={port}");

            clientListener = new EventBasedNetListener();
            client = new NetManager(clientListener);
            client.Start();

            client.Connect(ip, port, "");
            Console.WriteLine("Verbindungsanfrage gesendet...");

            SetupClientEventHandlers();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Verbinden mit dem Server: {ex.Message}");
        }
    }

    private void SetupClientEventHandlers()
    {
        if (clientListener == null) return;

        clientListener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"Erfolgreich mit dem Server verbunden: {peer}");
        };

        clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"Verbindung zum Server verloren: {peer}, Grund: {disconnectInfo.Reason}");
        };

        clientListener.NetworkErrorEvent += (endPoint, error) =>
        {
            Console.WriteLine($"Netzwerkfehler bei Verbindung zu {endPoint}: {error}");
        };

        clientListener.NetworkReceiveEvent += HandleClientReceiveEvent;
    }

    private void HandleClientReceiveEvent(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        try
        {
            string messageType = reader.GetString();
            Console.WriteLine($"[Client] Nachricht vom Server empfangen: {messageType}");

            switch (messageType)
            {
                case "StartGame":
                    GameStateManager.Instance.SetStateToInGame();
                    break;
                case "EnterLobby":
                    Console.WriteLine("[Client] Betrete die Lobby...");
                    GameStateManager.Instance.SetStateToLobby();
                    break;
                case "PlayerList":
                    HandlePlayerListUpdate(reader);
                    break;
                case "TimerUpdate":
                    float serverTimeSeconds = reader.GetFloat();
                    break;
                case "ChatMessage":
                    string senderName = reader.GetString();
                    string chatMsg = reader.GetString();
                    GameStateManager.Instance.ChatLog.AddMessage($"{senderName}: {chatMsg}");
                    if (senderName != "You")
                    {
                        GameStateManager.Instance.ChatLog.AddMessage($"{senderName}: {chatMsg}");
                    }
                    break;
                case "SystemMessage":
                    string systemMsg = reader.GetString();
                    GameStateManager.Instance.ChatLog.AddMessage($"[System] {systemMsg}");
                    break;
                default:
                    Console.WriteLine($"[Client] Unbekannter Nachrichtentyp empfangen: {messageType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Fehler beim Verarbeiten der Nachricht: {ex.Message}");
        }
        finally
        {
            reader.Recycle();
        }
    }

    private void HandlePlayerListUpdate(NetDataReader reader)
    {
        try
        {
            int count = reader.GetInt();
            Console.WriteLine($"[Client] Empfange Spielerliste mit {count} Spielern.");
            connectedPlayers.Clear();

            for (int i = 0; i < count; i++)
            {
                string address = reader.GetString();
                string name = reader.GetString();
                bool isReady = reader.GetBool();
                connectedPlayers.Add(new Player(address) { Name = name, IsReady = isReady });
                Console.WriteLine($"[Client] Spieler empfangen: {name} ({address}), Bereit: {isReady}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Fehler beim Verarbeiten der Spielerliste: {ex.Message}");
        }
    }

    public int GetHostPort() => hostPort;

    public bool IsPlayerConnected() => server != null && server.IsRunning && server.ConnectedPeersCount > 0 ||
                                     (client != null && client.FirstPeer != null && client.FirstPeer.ConnectionState == ConnectionState.Connected);

    /// <summary>
    /// Verarbeitet eingehende und ausgehende Netzwerkereignisse.
    /// Muss aufgerufen werden, um Nachrichten zu empfangen und zu senden.
    /// </summary>
    public void PollEvents()
    {
        server?.PollEvents();
        client?.PollEvents();
    }

    public void Shutdown()
    {
        server?.Stop();
        client?.Stop();
        discoveredLobbies.Clear();
        connectedPlayers.Clear();
    }


    private void BroadcastSystemMessage(string message)
    {
        if (server == null) return;
        var writer = new NetDataWriter();
        writer.Put("SystemMessage");
        writer.Put(message);
        server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        GameStateManager.Instance.ChatLog.AddMessage($"[System] {message}");
    }

    private void BroadcastChatMessage(string senderName, string message)
    {
        if (server == null) return;
        var writer = new NetDataWriter();
        writer.Put("ChatMessage");
        writer.Put(senderName);
        writer.Put(message);
        server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        GameStateManager.Instance.ChatLog.AddMessage($"{senderName}: {message}");
    }

    public void SendToAllClients(string message)
    {
        if (server == null) return;

        var writer = new NetDataWriter();
        writer.Put(message);

        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    public List<Player> GetConnectedPlayers()
    {
        return connectedPlayers;
    }

    public bool IsClientReady()
    {
        return clientReady;
    }

    public void ToggleReadyStatus()
    {
        clientReady = !clientReady;

        if (client != null && client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            string message = clientReady ? "PlayerReady" : "PlayerNotReady";
            writer.Put(message);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Nachricht gesendet: {message}");
        }
        else
        {
            Console.WriteLine("[Client] Kein Server verbunden, Nachricht konnte nicht gesendet werden.");
        }
    }

    public void BroadcastStartGame()
    {
        if (!IsHost() || server == null)
        {
            Console.WriteLine("[NetworkManager] Only the host can start the game.");
            return;

        }
    }

        if (!connectedPlayers.All(p => p.IsReady))
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
    /// Sends a chat message from the client to the server/host.
    /// </summary>
    /// <param name="message">The chat message text.</param>
    public void SendChatMessage(string message)
    {
        if (client == null || client.FirstPeer == null || client.FirstPeer.ConnectionState != ConnectionState.Connected)
        {
            Console.WriteLine("[Client] Cannot send chat message: Not connected to a server.");
            return;
        }

        try
        {
            var writer = new NetDataWriter();
            writer.Put("ChatMessage");
            writer.Put(message);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error sending chat message: {ex.Message}");
        }
    }
}
