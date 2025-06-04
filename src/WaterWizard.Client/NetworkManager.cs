using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using System.Net.Sockets;
using WaterWizard.Client.gamescreen;
using WaterWizard.Client.gamescreen.ships;
using WaterWizard.Shared;

namespace WaterWizard.Client;

public class NetworkManager
{
    private static NetworkManager? instance;
    public static NetworkManager Instance => instance ??= new NetworkManager();

    private readonly List<Player> connectedPlayers = [];
    private readonly List<LobbyInfo> discoveredLobbies = [];

    private NetManager? server;
    private NetManager? client;
    private EventBasedNetListener? serverListener;
    private EventBasedNetListener? clientListener;
    private readonly bool isPlayerConnected = false;
    private readonly int hostPort = 7777;

    private bool clientReady = false;

    private GameSessionId? sessionId;
    public GameSessionId? SessionId => sessionId;

    /// <summary>
    /// Stores the current lobby countdown seconds (null if no countdown active).
    /// </summary>
    public int? LobbyCountdownSeconds { get; private set; }

    
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
            connectedPlayers.Add(new Player("Host") { Name = "Host (You)", IsReady = false });

            sessionId = new GameSessionId();

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
            writer.Put(sessionId != null ? sessionId.ToString() : string.Empty);
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
            LobbyCountdownSeconds = null;
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

    /// <summary>
    /// Verarbeitet den Countdown für die Lobby.
    /// </summary>
    /// <param name="reader">Der NetPacketReader, der die Nachricht enthält.</param>
    /// <returns></returns>
    public void HandleLobbyCountdown(NetPacketReader reader)
    {
        int secondsLeft = reader.GetInt();
        if (secondsLeft <= 0)
        {
            LobbyCountdownSeconds = null;
        }
        else
        {
            LobbyCountdownSeconds = secondsLeft;
        }
        Console.WriteLine($"[Client] Lobby countdown: {LobbyCountdownSeconds}");
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
                    reader.GetString();
                    break;
                case "LobbyCountdown":
                    HandleLobbyCountdown(reader);
                    break;
                case "PlayerJoin":
                    string playerName = reader.GetString(); // Name sent by the client
                    var playerToUpdate = connectedPlayers.FirstOrDefault(p => p.Address == peer.ToString());
                    if (playerToUpdate != null)
                    {
                        playerToUpdate.Name = playerName;
                        UpdatePlayerList(); // Broadcast the updated player list to all clients
                    }
                    else
                    {
                        // This might indicate an unexpected state, e.g., PlayerJoin from an unrecognized peer.
                        Console.WriteLine($"[Host] PlayerJoin: Player with address {peer} not found in connectedPlayers. Name received: {playerName}");
                        // Optionally, handle this by adding the player if it's a valid scenario,
                        // though players are typically added during PeerConnectedEvent.
                        // connectedPlayers.Add(new Player(peer.ToString()) { Name = playerName, IsReady = false });
                        // UpdatePlayerList();
                    }
                    break;
                case "UpdateMana":
                {
                    int playerIndex = reader.GetInt();
                    int mana = reader.GetInt();
                    Console.WriteLine($"[Client] Spieler {playerIndex} hat nun {mana} Mana.");

                    GameStateManager.Instance.SetMana(playerIndex, mana);
                    break;
                }
                case "UpdateGold":
                {
                    int playerIndex = reader.GetInt();
                    int gold = reader.GetInt();
                    Console.WriteLine($"[Client] Spieler {playerIndex} hat nun {gold} Gold.");

                    GameStateManager.Instance.SetGold(playerIndex, gold);
                    // TODO: UI-Anzeige für Gold aktualisieren
                    break;
                }
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

                Console.WriteLine($"[Client] Lobby found: '{lobbyName}' with {playerCount} players at {remoteEndPoint}");

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
            Console.WriteLine($"[Client] Error processing lobby info: {ex.Message}");
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
            client.SendUnconnectedMessage(req, new IPEndPoint(IPAddress.Parse("208.77.246.27"), hostPort));
            Console.WriteLine("[Client] Discovery request sent to 208.77.246.27");

            client.SendUnconnectedMessage(req, new IPEndPoint(IPAddress.Loopback, hostPort));
            Console.WriteLine("[Client] Discovery request sent to localhost");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error sending discovery requests: {ex.Message}");
        }

        Console.WriteLine("[Client] Lobby discovery requests sent");
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

    public void ConnectToServer(string ip, int port = 7777)
    {
        if (client != null && client.FirstPeer != null && client.FirstPeer.ConnectionState == ConnectionState.Connected)
        {
            Console.WriteLine("[Client] Bereits mit dem Server verbunden.");
            return;
        }

        CleanupClientIfRunning();

        try
        {
            string cleanIp = ip;
            int connectionPort = port;

            if (ip.Contains(":"))
            {
                var parts = ip.Split(':');
                cleanIp = parts[0];

                if (parts.Length > 1 && int.TryParse(parts[1], out int specifiedPort))
                {
                    connectionPort = specifiedPort;
                }

                Console.WriteLine($"[Client] Extracted IP: {cleanIp}, Port: {connectionPort} from {ip}");
            }

            Console.WriteLine($"Versuche, eine Verbindung zum Server herzustellen: IP={cleanIp}, Port={connectionPort}");

            clientListener = new EventBasedNetListener();
            client = new NetManager(clientListener)
            {
                ReconnectDelay = 500,
                MaxConnectAttempts = 10,
                DisconnectTimeout = 10000,
                UpdateTime = 15,
                UnconnectedMessagesEnabled = true,
                IPv6Enabled = false,
                NatPunchEnabled = true, 
                EnableStatistics = true
            };

            if (!client.Start())
            {
                Console.WriteLine("[Client] Fehler beim Starten des Network Clients");
                GameStateManager.Instance.ChatLog.AddMessage("[Error] Failed to start network client");
                return;
            }

            SetupClientEventHandlers();

            bool pingAttemptFailed = true;

            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = ping.Send(cleanIp, 2000);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        Console.WriteLine($"[Client] Server at {cleanIp} is pingable. Latency: {reply.RoundtripTime}ms");
                        GameStateManager.Instance.ChatLog.AddMessage($"Server at {cleanIp} is reachable. Attempting connection...");
                        pingAttemptFailed = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Ping failed: {ex.Message}");
            }

            if (pingAttemptFailed)
            {
                GameStateManager.Instance.ChatLog.AddMessage($"Warning: Server at {cleanIp} did not respond to ping. Trying connection anyway...");
            }

            client.Connect(cleanIp, connectionPort, "WaterWizardClient");
            Console.WriteLine($"Verbindungsanfrage gesendet an {cleanIp}:{connectionPort}...");

            int connectionAttempts = 0;
            const int maxAttempts = 20;

            System.Timers.Timer connectionTimer = new System.Timers.Timer(500);
            connectionTimer.Elapsed += (s, e) =>
            {
                connectionAttempts++;

                client.PollEvents();

                bool isConnected = client.FirstPeer != null &&
                                   client.FirstPeer.ConnectionState == ConnectionState.Connected;

                if (isConnected)
                {
                    connectionTimer.Stop();
                    Console.WriteLine($"[Client] Connected to server after {connectionAttempts} attempts");
                }
                else if (connectionAttempts >= maxAttempts)
                {
                    connectionTimer.Stop();
                    Console.WriteLine($"[Client] Failed to connect after {maxAttempts} attempts");

                    GameStateManager.Instance.ChatLog.AddMessage($"Connection to server at {cleanIp}:{connectionPort} failed. Please check:");
                    GameStateManager.Instance.ChatLog.AddMessage("1. Is the server running?");
                    GameStateManager.Instance.ChatLog.AddMessage("2. Is the IP address correct?");
                    GameStateManager.Instance.ChatLog.AddMessage("3. Is port 7777 open in the server's firewall?");

                    TryDirectConnection(cleanIp, connectionPort);
                }
                else
                {
                    if (connectionAttempts % 5 == 0)
                    {
                        Console.WriteLine($"[Client] Connection attempt #{connectionAttempts}...");
                    }
                }
            };

            connectionTimer.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Verbinden mit dem Server: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            GameStateManager.Instance.ChatLog.AddMessage($"Error connecting to server: {ex.Message}");
        }
    }

    private void TryDirectConnection(string ip, int port)
    {
        GameStateManager.Instance.ChatLog.AddMessage("Attempting direct connection as last resort...");
        ConnectToServerDirect(ip, port);
    }



    private void SetupClientEventHandlers()
    {
        if (clientListener == null) return;

        clientListener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"[Client] Erfolgreich mit dem Server verbunden: {peer}");

            GameStateManager.Instance.ChatLog.AddMessage(
                $"Connected to server at {peer}"
            );
        };

        clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"[Client] Verbindung zum Server verloren: {peer}, Grund: {disconnectInfo.Reason}");

            string reasonExplanation = disconnectInfo.Reason switch
            {
                DisconnectReason.ConnectionFailed => "Could not establish connection (firewall or server offline?)",
                DisconnectReason.Timeout => "Connection timed out (network issues?)",
                DisconnectReason.RemoteConnectionClose => "Server closed the connection",
                _ => disconnectInfo.Reason.ToString()
            };

            Console.WriteLine($"[Client] Disconnect explanation: {reasonExplanation}");

            GameStateManager.Instance.ChatLog.AddMessage(
                $"Disconnected from server: {reasonExplanation}"
            );
            LobbyCountdownSeconds = null;
        };

        clientListener.NetworkErrorEvent += (endPoint, error) =>
        {
            Console.WriteLine($"[Client] Netzwerkfehler bei Verbindung zu {endPoint}: {error}");
            string errorExplanation = error switch
            {
                SocketError.HostUnreachable => "Host unreachable (check firewall settings or if server is online)",
                SocketError.ConnectionRefused => "Connection refused (server may be running but rejecting connections)",
                SocketError.TimedOut => "Connection timed out (network issues)",
                _ => error.ToString()
            };

            Console.WriteLine($"[Client] Error explanation: {errorExplanation}");

            GameStateManager.Instance.ChatLog.AddMessage(
                $"Network error: {errorExplanation}"
            );
        };

        clientListener.NetworkReceiveEvent += HandleClientReceiveEvent;
    }

    public void ConnectToServerDirect(string ip, int port)
    {
        try
        {
            Console.WriteLine($"[Client] Attempting direct connection to {ip}:{port}...");

            CleanupClientIfRunning();

            clientListener = new EventBasedNetListener();
            client = new NetManager(clientListener)
            {
                ReconnectDelay = 500,
                MaxConnectAttempts = 10,
                DisconnectTimeout = 10000,
                UpdateTime = 15,
                UnconnectedMessagesEnabled = true,
                IPv6Enabled = false,
                NatPunchEnabled = true,  
                EnableStatistics = true
            };

            if (!client.Start())
            {
                Console.WriteLine("[Client] Failed to start network client for direct connection");
                return;
            }

            SetupClientEventHandlers();

            // Try connection with a key (sometimes helps with certain NAT configurations)
            client.Connect(ip, port, "WaterWizardClientDirect");
            Console.WriteLine($"[Client] Direct connection request sent to {ip}:{port}");

            // Actively poll events to process connection
            System.Threading.Tasks.Task.Run(() =>
            {
                for (int i = 0; i < 100; i++) // Poll for 10 seconds
                {
                    client.PollEvents();
                    System.Threading.Thread.Sleep(100);

                    if (client.FirstPeer != null &&
                        client.FirstPeer.ConnectionState == ConnectionState.Connected)
                    {
                        Console.WriteLine("[Client] Direct connection successful!");
                        return;
                    }
                }
                Console.WriteLine("[Client] Direct connection attempt timed out");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error in direct connection attempt: {ex.Message}");
        }
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
                case "LobbyCountdown":
                    HandleLobbyCountdown(reader);
                    break;
                case "EnterLobby":
                    string receivedSessionId = reader.GetString();
                    if (!string.IsNullOrEmpty(receivedSessionId))
                        sessionId = new GameSessionId(receivedSessionId);
                    Console.WriteLine("[Client] Betrete die Lobby...");
                    // Sende eigenen Namen an den Server
                    if (client != null && client.FirstPeer != null)
                    {
                        var joinWriter = new NetDataWriter();
                        joinWriter.Put("PlayerJoin");
                        joinWriter.Put(Environment.UserName); // oder eigenen Namen aus UI
                        client.FirstPeer.Send(joinWriter, DeliveryMethod.ReliableOrdered);
                    }
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
                    break;
                case "SystemMessage":
                    string systemMsg = reader.GetString();
                    GameStateManager.Instance.ChatLog.AddMessage($"[System] {systemMsg}");
                    break;
                case "StartPlacementPhase":
                    GameStateManager.Instance.SetStateToPlacementPhase();
                    break;
                case "ShipPosition":
                    {
                        int x = reader.GetInt();
                        int y = reader.GetInt();
                        int width = reader.GetInt();
                        int height = reader.GetInt();
                        Console.WriteLine($"[Client] Schiff Platziert auf: {messageType} {x} {y} {width} {height}");
                    }
                    break;
                case "BoughtCard":
                    string cardVariant = reader.GetString();
                    GameStateManager.Instance.GameScreen.HandleBoughtCard(cardVariant);
                    break;
                case "OpponentBoughtCard":
                    string cardType = reader.GetString();
                    GameStateManager.Instance.GameScreen.HandleOpponentBoughtCard(cardType);
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
        LobbyCountdownSeconds = null; 
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
            //writer.Put("PlayerList");
            //writer.Put(connectedPlayers.Count);
            //string message = clientReady ? "PlayerReady" : "PlayerNotReady";
            //writer.Put(message);
            //client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            //Console.WriteLine($"[Client] Nachricht gesendet: {message}");

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
            // Zeige die eigene Nachricht sofort im Chatfenster an
            GameStateManager.Instance.ChatLog.AddMessage($"{Environment.UserName}: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error sending chat message: {ex.Message}");
        }
    }

    public void SendPlacementReady()
    {
        if (client != null && client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("PlacementReady");
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine("[Client] PlacementReady gesendet");
        }
        else
        {
            Console.WriteLine("[Client] Kein Server verbunden, PlacementReady konnte nicht gesendet werden.");
        }
    }

    public void SendShipPlacement(int x, int y, int width, int height){
        if(client != null && client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("PlaceShip");
            writer.Put(x);
            writer.Put(y);
            writer.Put(width);
            writer.Put(height);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine("[Client] PlaceShip gesendet");
        }
        else
        {
            Console.WriteLine("[Client] Kein Server verbunden, PlaceShip konnte nicht gesendet werden.");
        }
    }

    internal void RequestCardBuy(string cardType)
    {
        if(client != null && client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("BuyCard");
            writer.Put(cardType);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine("[Client] Kaufe Karte");
        }
        else
        {
            Console.WriteLine("[Client] Kein Server verbunden, PlaceShip konnte nicht gesendet werden.");
        }
    }
}
