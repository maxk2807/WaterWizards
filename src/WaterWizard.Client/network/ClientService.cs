using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.gamescreen;
using WaterWizard.Client.gamescreen.handler;
using WaterWizard.Shared;

namespace WaterWizard.Client.network;

public class ClientService(NetworkManager manager)
{
    public NetManager? client;
    public EventBasedNetListener? clientListener;
    public bool clientReady = false;
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

        clientListener.NetworkReceiveUnconnectedEvent += (remoteEndPoint, reader, messageType) =>
            LobbyHandler.HandleLobbyInfoResponse(manager, remoteEndPoint, reader);
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

                    LobbyHandler.TryDirectConnection(cleanIp, connectionPort);
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

    public void SetupClientEventHandlers()
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

                    GameStateManager.Instance.ResetGame();

                    if (client != null && client.FirstPeer != null)
                    {
                        var joinWriter = new NetDataWriter();
                        joinWriter.Put("PlayerJoin");
                        joinWriter.Put(Environment.UserName);
                        client.FirstPeer.Send(joinWriter, DeliveryMethod.ReliableOrdered);
                    }
                    GameStateManager.Instance.SetStateToLobby();
                    break;
                case "PlayerList":
                    HandlePlayer.HandlePlayerListUpdate(reader);
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
                    try
                    {
                        NetworkManager.HandleGameOverMessage(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Client] Error handling game over: {ex.Message}");
                    }
                    break;
                case "ResetGame":
                    Console.WriteLine("[Client] Received game reset command from server");
                    GameStateManager.Instance.ResetGame();
                    break;
                case "ShipPosition":
                    try
                    {
                        HandleShips.HandleShipPosition(messageType, reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Client] Fehler beim Verarbeiten von ShipPosition: {ex.Message}");
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
                        HandleShips.HandleShipSync(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von ShipSync: {ex.Message}"
                        );
                    }
                    break;

                case "CellReveal":
                    try
                    {
                        HandleCell.HandleCellReveal(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Client] Error processing CellReveal: {ex.Message}");
                    }
                    break;

                case "ShipReveal":
                    try
                    {
                        HandleShips.HandleShipReveal(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Client] Error processing ShipReveal: {ex.Message}");
                    }
                    break;
                case "ShipPlacementError":
                    try
                    {
                        HandleShips.HandleShipPlacementError(reader);
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
                        HandleCards.HandleActiveCards(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von ActiveCardsError: {ex.Message}"
                        );
                    }
                    break;
                case "UpdateMana":
                    {
                        HandleRessources.HandleUpdateMana(reader);
                        break;
                    }
                case "UpdateGold":
                    {
                        HandleRessources.HandleUpdateGold(reader);
                        break;
                    }
                case "AttackResult":
                    try
                    {
                        HandleAttacks.HandleAttackResult(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Client] Error handling AttackResult: {ex.Message}");
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
