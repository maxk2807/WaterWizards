using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.gamescreen;
using WaterWizard.Client.gamescreen.ships;
using WaterWizard.Shared;

namespace WaterWizard.Client.network;

public class ClientService(NetworkManager manager)
{
    private NetManager? client;
    private EventBasedNetListener? clientListener;
    private bool clientReady = false;
    public List<Player> ConnectedPlayers { get; private set; } = [];

    private GameSessionId? sessionId;
    public GameSessionId? SessionId => sessionId;

    public void InitializeClientForDiscovery()
    {
        CleanupIfRunning();

        clientListener = new EventBasedNetListener();
        client = new NetManager(clientListener) { UnconnectedMessagesEnabled = true };

        if (!client.Start())
        {
            Console.WriteLine("[Client] Fehler beim Starten des Discovery-Clients");
            return;
        }

        clientListener.NetworkReceiveUnconnectedEvent += HandleLobbyInfoResponse;
    }

    public void CleanupIfRunning()
    {
        if (client != null && client.IsRunning)
        {
            client.Stop();
            client = null;
        }
    }

    public bool IsServerConnected() =>
        client != null
        && client.FirstPeer != null
        && client.FirstPeer.ConnectionState == ConnectionState.Connected;

    public void PollEvents()
    {
        client?.PollEvents();
    }

    public void Shutdown()
    {
        client?.Stop();
    }

    public bool IsClientReady()
    {
        return clientReady;
    }

    private void HandleLobbyInfoResponse(
        IPEndPoint remoteEndPoint,
        NetPacketReader reader,
        UnconnectedMessageType messageType
    )
    {
        try
        {
            string msgType = reader.GetString();
            if (msgType == "LobbyInfo")
            {
                string lobbyName = reader.GetString();
                int playerCount = reader.GetInt();

                Console.WriteLine(
                    $"[Client] Lobby found: '{lobbyName}' with {playerCount} players at {remoteEndPoint}"
                );

                var existingLobby = manager.discoveredLobbies.FirstOrDefault(l =>
                    l.IP == remoteEndPoint.ToString()
                );
                if (existingLobby != null)
                {
                    existingLobby.Name = lobbyName;
                    existingLobby.PlayerCount = playerCount;
                }
                else
                {
                    manager.discoveredLobbies.Add(
                        new LobbyInfo(remoteEndPoint.ToString(), lobbyName, playerCount)
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error processing lobby info: {ex.Message}");
        }
    }

    public void SendDiscoveryRequests()
    {
        if (client == null || !client.IsRunning)
            return;

        var req = new NetDataWriter();
        req.Put("DiscoverLobbies");

        client.SendBroadcast(req, manager.hostPort);

        try
        {
            client.SendUnconnectedMessage(
                req,
                new IPEndPoint(IPAddress.Parse("208.77.246.27"), manager.hostPort)
            );
            Console.WriteLine("[Client] Discovery request sent to 208.77.246.27");

            client.SendUnconnectedMessage(
                req,
                new IPEndPoint(IPAddress.Loopback, manager.hostPort)
            );
            Console.WriteLine("[Client] Discovery request sent to localhost");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error sending discovery requests: {ex.Message}");
        }

        Console.WriteLine("[Client] Lobby discovery requests sent");
    }

    /// <summary>
    /// Refreshes the list of discovered lobbies.
    /// </summary>
    public void RefreshLobbies()
    {
        if (client != null && client.IsRunning)
        {
            SendDiscoveryRequests();
            Console.WriteLine("[Client] Lobby-Liste aktualisiert");

            if (manager.hostService.IsRunning())
            {
                manager.hostService.AddLocalLobbyToDiscoveredList();
            }
        }
        else
        {
            manager.DiscoverLobbies();
        }
    }

    public void ConnectToServer(string ip, int port = 7777)
    {
        if (
            client != null
            && client.FirstPeer != null
            && client.FirstPeer.ConnectionState == ConnectionState.Connected
        )
        {
            Console.WriteLine("[Client] Bereits mit dem Server verbunden.");
            return;
        }

        CleanupIfRunning();

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

                Console.WriteLine(
                    $"[Client] Extracted IP: {cleanIp}, Port: {connectionPort} from {ip}"
                );
            }

            Console.WriteLine(
                $"Versuche, eine Verbindung zum Server herzustellen: IP={cleanIp}, Port={connectionPort}"
            );

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
                EnableStatistics = true,
            };

            if (!client.Start())
            {
                Console.WriteLine("[Client] Fehler beim Starten des Network Clients");
                GameStateManager.Instance.ChatLog.AddMessage(
                    "[Error] Failed to start network client"
                );
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
                        Console.WriteLine(
                            $"[Client] Server at {cleanIp} is pingable. Latency: {reply.RoundtripTime}ms"
                        );
                        GameStateManager.Instance.ChatLog.AddMessage(
                            $"Server at {cleanIp} is reachable. Attempting connection..."
                        );
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
                GameStateManager.Instance.ChatLog.AddMessage(
                    $"Warning: Server at {cleanIp} did not respond to ping. Trying connection anyway..."
                );
            }

            client.Connect(cleanIp, connectionPort, "WaterWizardClient");
            Console.WriteLine($"Verbindungsanfrage gesendet an {cleanIp}:{connectionPort}...");

            int connectionAttempts = 0;
            const int maxAttempts = 20;

            System.Timers.Timer connectionTimer = new(500);
            connectionTimer.Elapsed += (s, e) =>
            {
                connectionAttempts++;

                client.PollEvents();

                bool isConnected =
                    client.FirstPeer != null
                    && client.FirstPeer.ConnectionState == ConnectionState.Connected;

                if (isConnected)
                {
                    connectionTimer.Stop();
                    Console.WriteLine(
                        $"[Client] Connected to server after {connectionAttempts} attempts"
                    );
                }
                else if (connectionAttempts >= maxAttempts)
                {
                    connectionTimer.Stop();
                    Console.WriteLine($"[Client] Failed to connect after {maxAttempts} attempts");

                    GameStateManager.Instance.ChatLog.AddMessage(
                        $"Connection to server at {cleanIp}:{connectionPort} failed. Please check:"
                    );
                    GameStateManager.Instance.ChatLog.AddMessage("1. Is the server running?");
                    GameStateManager.Instance.ChatLog.AddMessage("2. Is the IP address correct?");
                    GameStateManager.Instance.ChatLog.AddMessage(
                        "3. Is port 7777 open in the server's firewall?"
                    );

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
            GameStateManager.Instance.ChatLog.AddMessage(
                $"Error connecting to server: {ex.Message}"
            );
        }
    }

    private void SetupClientEventHandlers()
    {
        if (clientListener == null)
            return;

        clientListener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"[Client] Erfolgreich mit dem Server verbunden: {peer}");

            GameStateManager.Instance.ChatLog.AddMessage($"Connected to server at {peer}");
        };

        clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine(
                $"[Client] Verbindung zum Server verloren: {peer}, Grund: {disconnectInfo.Reason}"
            );

            string reasonExplanation = disconnectInfo.Reason switch
            {
                DisconnectReason.ConnectionFailed =>
                    "Could not establish connection (firewall or server offline?)",
                DisconnectReason.Timeout => "Connection timed out (network issues?)",
                DisconnectReason.RemoteConnectionClose => "Server closed the connection",
                _ => disconnectInfo.Reason.ToString(),
            };

            Console.WriteLine($"[Client] Disconnect explanation: {reasonExplanation}");

            GameStateManager.Instance.ChatLog.AddMessage(
                $"Disconnected from server: {reasonExplanation}"
            );
            manager.LobbyCountdownSeconds = null;
        };

        clientListener.NetworkErrorEvent += (endPoint, error) =>
        {
            Console.WriteLine($"[Client] Netzwerkfehler bei Verbindung zu {endPoint}: {error}");
            string errorExplanation = error switch
            {
                SocketError.HostUnreachable =>
                    "Host unreachable (check firewall settings or if server is online)",
                SocketError.ConnectionRefused =>
                    "Connection refused (server may be running but rejecting connections)",
                SocketError.TimedOut => "Connection timed out (network issues)",
                _ => error.ToString(),
            };

            Console.WriteLine($"[Client] Error explanation: {errorExplanation}");

            GameStateManager.Instance.ChatLog.AddMessage($"Network error: {errorExplanation}");
        };

        clientListener.NetworkReceiveEvent += HandleClientReceiveEvent;
    }

    private void TryDirectConnection(string ip, int port)
    {
        GameStateManager.Instance.ChatLog.AddMessage(
            "Attempting direct connection as last resort..."
        );
        ConnectToServerDirect(ip, port);
    }

    public void ConnectToServerDirect(string ip, int port)
    {
        try
        {
            Console.WriteLine($"[Client] Attempting direct connection to {ip}:{port}...");

            CleanupIfRunning();

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
                EnableStatistics = true,
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

                    if (
                        client.FirstPeer != null
                        && client.FirstPeer.ConnectionState == ConnectionState.Connected
                    )
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

    private void HandleClientReceiveEvent(
        NetPeer peer,
        NetPacketReader reader,
        byte channelNumber,
        DeliveryMethod deliveryMethod
    )
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
                    manager.HandleLobbyCountdown(reader);
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
                    try
                    {
                        reader.GetFloat();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Lesen von TimerUpdate: {ex.Message}"
                        );
                    }
                    break;
                case "ChatMessage":
                    try
                    {
                        string senderName = reader.GetString();
                        string chatMsg = reader.GetString();
                        GameStateManager.Instance.ChatLog.AddMessage($"{senderName}: {chatMsg}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von ChatMessage: {ex.Message}"
                        );
                    }
                    break;
                case "SystemMessage":
                    try
                    {
                        string systemMsg = reader.GetString();
                        GameStateManager.Instance.ChatLog.AddMessage($"[System] {systemMsg}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von SystemMessage: {ex.Message}"
                        );
                    }
                    break;
                case "StartPlacementPhase":
                    GameStateManager.Instance.SetStateToPlacementPhase();
                    break;
                case "GameOver":
                    string result = reader.GetString();
                    Console.WriteLine($"[Client] Game Over: {result}");
                    
                    GameStateManager.Instance.ChatLog.AddMessage($"Game Over: {result}");
                    GameStateManager.Instance.ChatLog.AddMessage("Returning to lobby in 5 seconds...");
                    
                    var clientTimer = new System.Timers.Timer(5000);
                    clientTimer.Elapsed += (s, e) =>
                    {
                        clientTimer.Stop();
                        clientTimer.Dispose();
                        GameStateManager.Instance.SetStateToLobby();
                        Console.WriteLine("[Client] Transitioned back to lobby after game over");
                    };
                    clientTimer.Start();
                    break;
                case "ShipPosition":
                    try
                    {
                        var playerBoard = GameStateManager.Instance.GameScreen!.playerBoard;
                        if (playerBoard == null)
                        {
                            Console.WriteLine(
                                "[Client] Fehler: playerBoard ist null bei ShipPosition."
                            );
                            break;
                        }
                        int x = reader.GetInt();
                        int y = reader.GetInt();
                        int width = reader.GetInt();
                        int height = reader.GetInt();
                        Console.WriteLine($"[Client] Schiff Platziert auf: {messageType} {x} {y} {width} {height}");
                        int pixelX = (int)playerBoard.Position.X + x * playerBoard.CellSize;
                        int pixelY = (int)playerBoard.Position.Y + y * playerBoard.CellSize;
                        int pixelWidth = width * playerBoard.CellSize;
                        int pixelHeight = height * playerBoard.CellSize;
                        playerBoard.putShip(
                            new GameShip(
                                GameStateManager.Instance.GameScreen,
                                pixelX,
                                pixelY,
                                ShipType.DEFAULT,
                                pixelWidth,
                                pixelHeight
                            )
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von ShipPosition: {ex.Message}"
                        );
                    }
                    break;
                case "BoughtCard":
                    try
                    {
                        string cardVariant = reader.GetString();
                        GameStateManager.Instance.GameScreen?.HandleBoughtCard(cardVariant);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von BoughtCard: {ex.Message}"
                        );
                    }
                    break;
                case "OpponentBoughtCard":
                    try
                    {
                        string cardType = reader.GetString();
                        GameStateManager.Instance.GameScreen?.HandleOpponentBoughtCard(cardType);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von OpponentBoughtCard: {ex.Message}"
                        );
                    }
                    break;
                case "ShipSync":
                    try
                    {
                        int count = reader.GetInt();
                        var playerBoard = GameStateManager.Instance.GameScreen!.playerBoard;
                        if (playerBoard == null)
                        {
                            Console.WriteLine(
                                "[Client] Fehler: playerBoard ist null bei ShipSync."
                            );
                            break;
                        }
                        playerBoard.Ships.Clear();
                        for (int i = 0; i < count; i++)
                        {
                            int x = reader.GetInt();
                            int y = reader.GetInt();
                            int width = reader.GetInt();
                            int height = reader.GetInt();
                            int pixelX = (int)playerBoard.Position.X + x * playerBoard.CellSize;
                            int pixelY = (int)playerBoard.Position.Y + y * playerBoard.CellSize;
                            int pixelWidth = width * playerBoard.CellSize;
                            int pixelHeight = height * playerBoard.CellSize;
                            playerBoard.putShip(
                                new GameShip(
                                    GameStateManager.Instance.GameScreen,
                                    pixelX,
                                    pixelY,
                                    ShipType.DEFAULT,
                                    pixelWidth,
                                    pixelHeight
                                )
                            );
                        }
                        Console.WriteLine(
                            $"[Client] Nach ShipSync sind {playerBoard.Ships.Count} Schiffe auf dem Board."
                        );
                        GameStateManager.Instance.SetStateToInGame();
                        Console.WriteLine(
                                $"[Client] Nach SetStateToInGame sind {playerBoard.Ships.Count} Schiffe auf dem Board."
                            );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von ShipSync: {ex.Message}"
                        );
                    }
                    break;

                // case "OpponentShipSync":
                //     try
                //     {
                //         int oppCount = reader.GetInt();
                //         var opponentBoard = GameStateManager.Instance.GameScreen!.opponentBoard;
                //         if (opponentBoard == null)
                //         {
                //             Console.WriteLine(
                //                 "[Client] Fehler: opponentBoard ist null bei OpponentShipSync."
                //             );
                //             break;
                //         }
                //         opponentBoard.Ships.Clear();
                //         for (int i = 0; i < oppCount; i++)
                //         {
                //             int x = reader.GetInt();
                //             int y = reader.GetInt();
                //             int width = reader.GetInt();
                //             int height = reader.GetInt();
                //             int pixelX = (int)opponentBoard.Position.X + x * opponentBoard.CellSize;
                //             int pixelY = (int)opponentBoard.Position.Y + y * opponentBoard.CellSize;
                //             int pixelWidth = width * opponentBoard.CellSize;
                //             int pixelHeight = height * opponentBoard.CellSize;
                //             opponentBoard.putShip(
                //                 new GameShip(
                //                     GameStateManager.Instance.GameScreen,
                //                     pixelX,
                //                     pixelY,
                //                     ShipType.DEFAULT,
                //                     pixelWidth,
                //                     pixelHeight
                //                 )
                //             );
                //         }
                //     }
                //     catch (Exception ex)
                //     {
                //         Console.WriteLine(
                //             $"[Client] Fehler beim Verarbeiten von OpponentShipSync: {ex.Message}"
                //         );
                //     }
                //
                //    break;
                case "CellReveal":
                    try
                    {
                        int x = reader.GetInt();
                        int y = reader.GetInt();
                        bool isHit = reader.GetBool();

                        var opponentBoard = GameStateManager.Instance.GameScreen!.opponentBoard;
                        if (opponentBoard != null)
                        {
                            opponentBoard.SetCellState(x, y, isHit ? gamescreen.CellState.Hit : gamescreen.CellState.Miss);
                            Console.WriteLine($"[Client] Cell revealed: ({x},{y}) = {(isHit ? "hit" : "miss")}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Client] Error processing CellReveal: {ex.Message}");
                    }
                    break;

                case "ShipReveal":
                    try
                    {
                        int x = reader.GetInt();
                        int y = reader.GetInt();
                        int width = reader.GetInt();
                        int height = reader.GetInt();
                        
                        var opponentBoard = GameStateManager.Instance.GameScreen!.opponentBoard;
                        if (opponentBoard != null)
                        {
                            int pixelX = (int)opponentBoard.Position.X + x * opponentBoard.CellSize;
                            int pixelY = (int)opponentBoard.Position.Y + y * opponentBoard.CellSize;
                            int pixelWidth = width * opponentBoard.CellSize;
                            int pixelHeight = height * opponentBoard.CellSize;
                            
                            opponentBoard.putShip(
                                new GameShip(
                                    GameStateManager.Instance.GameScreen,
                                    pixelX,
                                    pixelY,
                                    ShipType.DEFAULT,
                                    pixelWidth,
                                    pixelHeight
                                )
                            );
                            
                            Console.WriteLine($"[Client] Ship revealed: ({x},{y}) size {width}x{height}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Client] Error processing ShipReveal: {ex.Message}");
                    }
                    break;
                case "ShipPlacementError":
                    try
                    {
                        string errorMsg = reader.GetString();
                        Console.WriteLine(
                            $"[Client] Fehler beim Platzieren des Schiffs: {errorMsg}"
                        );
                        //GameStateManager.Instance.GameScreen?.ShowPlacementError(errorMsg);

                        // Sperre das Draggen für diese Größe, wenn das Limit erreicht ist
                        var match = System.Text.RegularExpressions.Regex.Match(
                            errorMsg,
                            @"nur (\d+) Schiffe der Länge (\d+)"
                        );
                        if (match.Success)
                        {
                            int size = int.Parse(match.Groups[2].Value);
                            GameStateManager.Instance.GameScreen?.MarkShipSizeLimitReached(size);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von ShipPlacementError: {ex.Message}"
                        );
                    }
                    break;
                case "ActiveCards":
                    try
                    {
                        var activeCardsNum = reader.GetInt();
                        List<Cards> activeCards = [];
                        for (int i = 0; i < activeCardsNum; i++)
                        {
                            var variant = reader.GetString();
                            var remainingDuration = reader.GetFloat();
                            Cards card = new(Enum.Parse<CardVariant>(variant))
                            {
                                remainingDuration = remainingDuration
                            };
                            activeCards.Add(card);
                            Console.WriteLine($"[Client] ActivateCard received: {variant}");
                        }

                        GameStateManager.Instance.GameScreen.activeCards!.UpdateActiveCards(activeCards);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von ActiveCardsError: {ex.Message}"
                        );
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Fehler beim Verarbeiten der Nachricht: {ex.Message}");
        }
        finally
        {
            try
            {
                reader.Recycle();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Fehler beim Recyceln des Readers: {ex.Message}");
            }
        }
    }

    private void HandlePlayerListUpdate(NetDataReader reader)
    {
        try
        {
            int count = reader.GetInt();
            Console.WriteLine($"[Client] Empfange Spielerliste mit {count} Spielern.");
            ConnectedPlayers.Clear();

            for (int i = 0; i < count; i++)
            {
                string address = reader.GetString();
                string name = reader.GetString();
                bool isReady = reader.GetBool();
                ConnectedPlayers.Add(new Player(address) { Name = name, IsReady = isReady });
                Console.WriteLine(
                    $"[Client] Spieler empfangen: {name} ({address}), Bereit: {isReady}"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Fehler beim Verarbeiten der Spielerliste: {ex.Message}");
        }
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
            Console.WriteLine(
                "[Client] Kein Server verbunden, Nachricht konnte nicht gesendet werden."
            );
        }
    }

    /// <summary>
    /// Sends a chat message from the client to the server/host.
    /// </summary>
    /// <param name="message">The chat message text.</param>
    public void SendChatMessage(string message)
    {
        if (
            client == null
            || client.FirstPeer == null
            || client.FirstPeer.ConnectionState != ConnectionState.Connected
        )
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
            Console.WriteLine(
                "[Client] Kein Server verbunden, PlacementReady konnte nicht gesendet werden."
            );
        }
    }

    public void SendShipPlacement(int x, int y, int width, int height)
    {
        if (client != null && client.FirstPeer != null)
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
            Console.WriteLine(
                "[Client] Kein Server verbunden, PlaceShip konnte nicht gesendet werden."
            );
        }
    }

    public void RequestCardBuy(string cardType)
    {
        if (client != null && client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("BuyCard");
            writer.Put(cardType);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine("[Client] Kaufe Karte");
        }
        else
        {
            Console.WriteLine(
                "[Client] Kein Server verbunden, PlaceShip konnte nicht gesendet werden."
            );
        }
    }

    public void HandleCast(Cards card, GameBoard.Point hoveredCoords)
    {
        if (client != null && client.FirstPeer != null)
        {
            NetDataWriter writer = new();
            writer.Put("CastCard");
            writer.Put(card.Variant.ToString());
            writer.Put(hoveredCoords.X);
            writer.Put(hoveredCoords.Y);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.Write("[Client] Karte wirken");
        }
        else
        {
            Console.WriteLine(
                "[Client] Kein Server verbunden, PlaceShip konnte nicht gesendet werden."
            );
        }
    }

    public void SendAttack(int x, int y)
    {
        if (client != null && client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("Attack");
            writer.Put(x);
            writer.Put(y);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Attack initiated at ({x}, {y})");
        }
    }
}
