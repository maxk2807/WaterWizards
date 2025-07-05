using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.gamescreen;
using WaterWizard.Client.Gamescreen;
using WaterWizard.Client.gamescreen.handler;
using WaterWizard.Client.network;
using WaterWizard.Shared;

namespace WaterWizard.Client.network;

public class ClientService(NetworkManager manager)
{
    public NetManager? client;
    public EventBasedNetListener? clientListener;
    public bool clientReady = false;
    public string? myEndPoint;
    public List<Player> ConnectedPlayers { get; private set; } = [];

    public GameSessionId? sessionId;
    public GameSessionId? SessionId => sessionId;

    public void InitializeClientForDiscovery()
    {
        CleanupIfRunning();
        myEndPoint = null;

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

    public void SetupClientEventHandlers()
    {
        if (clientListener == null)
            return;

        clientListener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"[Client] Erfolgreich mit dem Server verbunden: {peer}");
            Console.WriteLine($"[Client] Debug Peer Properties:");
            Console.WriteLine($"- peer.Address: {peer.Address}");
            Console.WriteLine($"- peer.Port: {peer.Port}");
            Console.WriteLine($"- peer.Id: {peer.Id}");
            Console.WriteLine($"- peer.ConnectionState: {peer.ConnectionState}");
            Console.WriteLine($"- peer.ToString(): {peer.ToString()}");
            Console.WriteLine($"- client.LocalPort: {client?.LocalPort}");

            myEndPoint = $"127.0.0.1:{client?.LocalPort}";
            Console.WriteLine($"[Client] Meine Adresse ist: {myEndPoint}");

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
                case "UpdatePauseState":
                    bool isPaused = reader.GetBool();
                    if (isPaused)
                    {
                        GameStateManager.Instance.GetGamePauseManager().PauseGame();
                    }
                    else
                    {
                        GameStateManager.Instance.GetGamePauseManager().ResumeGame();
                    }
                    break;
                case "StartGame":
                    GameStateManager.Instance.SetStateToInGame();
                    break;
                case "LobbyCountdown":
                    manager.HandleLobbyCountdown(reader);
                    break;
                case "EnterLobby":
                    LobbyHandler.EnterLobby(reader);
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
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von ShipPosition: {ex.Message}"
                        );
                    }
                    break;
                case "UpdateShipPosition":
                    try
                    {
                        HandleShips.HandleUpdateShipPosition(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client Fehler beim Verarbeiten von UpdateShipPosition: {ex.Message}]"
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
                        HandleShips.HandleShipSync(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von ShipSync: {ex.Message}"
                        );
                    }
                    break;

                case "RockSync":
                    try
                    {
                        HandleRocks.HandleRockSync(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[Client] Fehler beim Verarbeiten von RockSync: {ex.Message}"
                        );
                    }
                    break;

                case "CellReveal":
                    try
                    {
                        HandleAttacks.HandleCellReveal(reader);
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
                case "ThunderStrike":
                    int targetBoardIndex = reader.GetInt();
                    int strikeX = reader.GetInt();
                    int strikeY = reader.GetInt();
                    bool thunderHit = reader.GetBool();

                    var gameScreen = GameStateManager.Instance.GameScreen;
                    if (gameScreen != null)
                    {
                        int myPlayerIndex = GameStateManager.Instance.MyPlayerIndex;

                        GameBoard? targetBoard = null;
                        Console.WriteLine($"[Client] Thunder strike - MyPlayerIndex: {myPlayerIndex}, TargetBoardIndex: {targetBoardIndex}, Hit: {thunderHit}");
                        if (targetBoardIndex == myPlayerIndex)
                        {
                            targetBoard = gameScreen.playerBoard;
                            Console.WriteLine($"Thunder visual effect on MY board (playerBoard) at ({strikeX}, {strikeY}) hit={thunderHit}");
                        }
                        else
                        {
                            targetBoard = gameScreen.opponentBoard;
                            Console.WriteLine($"Thunder visual effect on OPPONENT's board (opponentBoard) at ({strikeX}, {strikeY}) hit={thunderHit}");
                        }

                        targetBoard?.AddThunderStrike(strikeX, strikeY, thunderHit);
                    }
                    break;
                case "ThunderReset":
                    GameStateManager.Instance.GameScreen.playerBoard?.ResetThunderFields();
                    GameStateManager.Instance.GameScreen.opponentBoard?.ResetThunderFields();
                    break;
                case "ShipHeal":
                    bool success = reader.GetBool();
                    if (success)
                    {
                        int X = reader.GetInt();
                        int Y = reader.GetInt();
                        GameStateManager.Instance.GameScreen.playerBoard!.SetCellState(
                                X,
                                Y,
                                Gamescreen.CellState.Ship
                            );
                        Console.WriteLine($"[Client] Healed At ({X},{Y})");
                        break;
                    }
                    Console.WriteLine($"[Client] Could not Heal, Possible mismatch between Client and Server");
                    break;

                case "PlayerIndex":
                    int playerIndex = reader.GetInt();
                    GameStateManager.Instance.MyPlayerIndex = playerIndex;
                    Console.WriteLine($"[Client] Received player index: {playerIndex}");
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
                case "GoldFreezeStatus":
                    {
                        HandleRessources.HandleGoldFreeze(reader);
                        break;
                    }
                case "ParalizeStatus":
                    {
                        HandleParalize.HandleParalizeStatus(reader);
                        break;
                    }
                case "HoveringEyeReveal":
                    HandleUtility.HandleHoveringEyeReveal(reader);
                    break;
                case "ShipTeleported":
                    HandleUtility.HandleShipTeleported(reader);
                    break;
                case "ShieldCreated":
                    HandleShield.HandleShieldCreated(reader);
                    break;
                case "ShieldExpired":
                    HandleShield.HandleShieldExpired(reader);
                    break;
                case "SummonShip":
                    GameStateManager.Instance.GameScreen.EnableSingleShipPlacement();
                    break;
                case "OpponentUsedCard":
                    HandleCards.HandleOpponentUsedCard(reader);
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
}
