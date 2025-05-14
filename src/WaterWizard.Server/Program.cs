using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Shared;
using WaterWizard.Server.ServerGameStates;

namespace WaterWizard.Server;

static class Program
{
    private static readonly Dictionary<string, bool> ConnectedPlayers = new();
    private static readonly Dictionary<string, bool> PlacementReadyPlayers = new();
    private static readonly Dictionary<string, string> PlayerNames = new();
    private static GameSessionTimer? _gameSessionTimer;

    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }
    static void Main()
    {
        Log("WaterWizards Server wird gestartet...");

        var listener = new EventBasedNetListener();
        var server = new NetManager(listener)
        {
            AutoRecycle = true,
            UnconnectedMessagesEnabled = true
        };

        _gameSessionTimer = new GameSessionTimer(server);

        var gameStateManager = new ServerGameStateManager(server);
        gameStateManager.ChangeState(new LobbyState(server));

        string localIp = NetworkUtils.GetLocalIPAddress();
        string publicIp = Environment.GetEnvironmentVariable("PUBLIC_ADDRESS") ?? NetworkUtils.GetPublicIPAddress();
        Console.WriteLine($"Server erfolgreich auf Port 7777 gestartet");
        Console.WriteLine($"Verbinde dich mit der IP-Adresse: {publicIp}:7777");
        Console.WriteLine($"localIp: {localIp}:7777");
        Console.WriteLine("Drücke ESC zum Beenden");

        using (_gameSessionTimer = new GameSessionTimer(server))
        {
            try
            {
                if (!server.Start(7777))
                {
                    Log("Server konnte nicht auf Port 7777 gestartet werden!");
                    return;
                }
                else
                {
                    Console.WriteLine($"Server erfolgreich auf Port 7777 gestartet");
                    Console.WriteLine($"Verbinde dich mit der IP-Adresse: {publicIp}:7777");
                    Console.WriteLine($"localIp: {localIp}:7777");
                    Console.WriteLine("Drücke ESC zum Beenden");

                    listener.NetworkReceiveUnconnectedEvent += (remoteEndPoint, reader, msgType) =>
                    {
                        try
                        {
                            string message = reader.GetString();
                            Log($"[Server] Unconnected message: {message} from {remoteEndPoint}");

                            if (message == "DiscoverLobbies")
                            {
                                var response = new NetDataWriter();
                                response.Put("LobbyInfo");
                                response.Put("WaterWizards Server");
                                response.Put(ConnectedPlayers.Count);
                                server?.SendUnconnectedMessage(response, remoteEndPoint);
                                Log($"[Server] Sent lobby info to {remoteEndPoint}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"[Server] Error processing discovery: {ex.Message}");
                        }
                    };

                    listener.ConnectionRequestEvent += request =>
                    {
                        request.Accept();
                        Log($"[Server] Client verbunden (ohne Schlüssel): {request.RemoteEndPoint}");
                    };

                    listener.PeerConnectedEvent += peer =>
                    {
                        Log($"[Server] Client {peer} verbunden");

                        string playerAddress = peer.ToString();
                        ConnectedPlayers.TryAdd(playerAddress, false);

                        var writer = new NetDataWriter();
                        writer.Put("EnterLobby");
                        writer.Put(""); // Leere SessionId, damit Client korrekt liest
                        peer.Send(writer, DeliveryMethod.ReliableOrdered);

                        if (_gameSessionTimer != null && _gameSessionTimer.IsRunning)
                        {
                            _gameSessionTimer.SendCurrentTimeToPeer(peer);
                        }

                        SendPlayerList(server);

                        PlacementReadyPlayers.Clear();
                    };

                    listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
                    {
                        Log($"[Server] Client {peer} getrennt: {disconnectInfo.Reason}");

                        var playerAddress = peer.ToString();

                        ConnectedPlayers.Remove(playerAddress);
                        PlacementReadyPlayers.Remove(playerAddress);
                        PlayerNames.Remove(playerAddress);

                        if (ConnectedPlayers.Count == 0)
                        {
                            Log("[Server] Letzter Spieler hat die Verbindung getrennt.");
                            if (_gameSessionTimer != null && _gameSessionTimer.IsRunning)
                            {
                                Log("[Server] Stoppe den Spiel-Timer.");
                                _gameSessionTimer.Stop();
                            }
                        }

                        SendPlayerList(server);
                    };

                    listener.NetworkErrorEvent += (endPoint, error) =>
                    {
                        Log($"[Server] Netzwerkfehler von {endPoint}: {error}");
                    };

                    listener.NetworkReceiveEvent += (peer, reader, channelNumber, deliveryMethod) =>
                    {
                        try
                        {
                            string messageType = reader.GetString();
                            switch (messageType)
                            {
                                case "PlayerJoin":
                                    string playerName = reader.GetString();
                                    PlayerNames[peer.ToString()] = playerName;
                                    Log($"[Server] PlayerJoin: {playerName} ({peer})");
                                    SendPlayerList(server);
                                    break;
                                case "ChatMessage":
                                    string chatMsg = reader.GetString();
                                    string senderName = PlayerNames.TryGetValue(peer.ToString(), out var name) ? name : $"Player_{peer.Port}";
                                    BroadcastChatMessage(server, peer, senderName, chatMsg);
                                    break;
                                default:
                                    gameStateManager.HandleNetworkEvent(peer, reader);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"[Server] Fehler beim Verarbeiten der Nachricht: {ex.Message}");
                        }
                        finally
                        {
                            reader.Recycle();
                        }
                    };

                    while (true)
                    {
                        server?.PollEvents();
                        Thread.Sleep(15);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[Server] FATAL ERROR: {ex.Message}");
                Log(ex.StackTrace ?? "No stack trace available.");
            }
            finally
            {
                Log("Server wird beendet...");
                _gameSessionTimer?.Dispose();
                server?.Stop();
                Log("Server beendet");
            }
        }
    }

    private static void BroadcastMessage(NetManager? server, NetDataWriter writer, DeliveryMethod deliveryMethod)
    {
        if (server == null)
            return;
        foreach (var connectedPeer in server.ConnectedPeerList)
        {
            connectedPeer.Send(writer, deliveryMethod);
        }
    }

    private static void SendPlayerList(NetManager? server)
    {
        if (server == null)
            return;
        var writer = new NetDataWriter();
        writer.Put("PlayerList");
        writer.Put(ConnectedPlayers.Count);

        foreach (var kvp in ConnectedPlayers)
        {
            writer.Put(kvp.Key);
            writer.Put(PlayerNames.TryGetValue(kvp.Key, out var name) ? name : $"Player_{kvp.Key.Split(':').LastOrDefault()}");
            writer.Put(kvp.Value);
        }

        BroadcastMessage(server, writer, DeliveryMethod.ReliableOrdered);
        Log($"[Server] Spielerliste mit {ConnectedPlayers.Count} Spielern gesendet");
    }

    private static void BroadcastChatMessage(NetManager? server, NetPeer senderPeer, string senderDisplayName, string message)
    {
        if (server == null)
            return;

        var writer = new NetDataWriter();
        writer.Put("ChatMessage");
        writer.Put(senderDisplayName);
        writer.Put(message);

        Log($"[Server] Sende Chat-Nachricht von [{senderDisplayName}] an andere Spieler: {message}");

        foreach (var recipientPeer in server.ConnectedPeerList)
        {
            if (recipientPeer != senderPeer)
            {
                recipientPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}