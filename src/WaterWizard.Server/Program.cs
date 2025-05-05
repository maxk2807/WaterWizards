using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Shared;

namespace WaterWizard.Server;

static class Program
{
    private static readonly Dictionary<string, bool> ConnectedPlayers = [];
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

        string localIp = NetworkUtils.GetLocalIPAddress();
        //string publicIp = NetworkUtils.GetPublicIPAddress();
        string publicIp = Environment.GetEnvironmentVariable("PUBLIC_ADDRESS") ?? NetworkUtils.GetPublicIPAddress();
        Console.WriteLine($"Server erfolgreich auf Port 7777 gestartet");
        Console.WriteLine($"Verbinde dich mit der IP-Adresse: {publicIp}:7777");
        Console.WriteLine($"localIp: {localIp}:7777");
        Console.WriteLine("Drücke ESC zum Beenden");

        using (_gameSessionTimer = new GameSessionTimer(server))

        {
            try
            {
                {
                    if (!server.Start(7777))
                    {
                        Log("Server konnte nicht auf Port 7777 gestartet werden!");
                        return;
                    }
                    else
                    {
                        string localIp = NetworkUtils.GetLocalIPAddress();
                        string publicIp = NetworkUtils.GetPublicIPAddress();
                        Console.WriteLine($"Server erfolgreich auf Port 7777 gestartet");
                        Console.WriteLine(
                            $"Verbinde dich mit der IP-Adresse: {publicIp}:7777");
                        Console.WriteLine($"localIp: {localIp}:7777");
                        Console.WriteLine("Drücke ESC zum Beenden");

                        listener.NetworkReceiveUnconnectedEvent += (remoteEndPoint, reader,
                                                                    msgType) =>
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
                            peer.Send(writer, DeliveryMethod.ReliableOrdered);

                            if (_gameSessionTimer != null && _gameSessionTimer.IsRunning)
                            {
                                _gameSessionTimer.SendCurrentTimeToPeer(peer);
                            }

                            SendPlayerList(server);
                        };

                        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
                        {
                            Log($"[Server] Client {peer} getrennt: {disconnectInfo.Reason}");

                            var playerAddress = peer.ToString();

                            ConnectedPlayers.Remove(playerAddress);

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

                        listener.NetworkReceiveEvent += (peer, reader, channelNumber,
                                                         deliveryMethod) =>
                        {
                            try
                            {
                                string messageType = reader.GetString();
                                Log($"[Server] Nachricht von Client {peer} (Kanal: {channelNumber}, Methode: {deliveryMethod}): {messageType}");  // Use EndPoint

                                switch (messageType)
                                {
                                    case "PlayerReady":
                                    case "PlayerNotReady":
                                        string playerAddress = peer.ToString();
                                        if (ConnectedPlayers.ContainsKey(playerAddress))
                                        {
                                            ConnectedPlayers[playerAddress] =
                                                (messageType == "PlayerReady");
                                            Log($"[Server] Player {playerAddress} status set to {(messageType == "PlayerReady")}");
                                            SendPlayerList(server);
                                            Log($"[Server] Spielerliste nach Ready/NotReady-Status Änderung gesendet.");
                                        }
                                        else
                                        {
                                            Log($"[Server] ERROR: Player {playerAddress} not found in dictionary!");
                                        }

                                        bool allReady = ConnectedPlayers.Values.All(ready => ready);
                                        if (allReady && ConnectedPlayers.Count > 0 &&
                                            _gameSessionTimer != null &&
                                            !_gameSessionTimer.IsRunning)
                                        {
                                            var startWriter = new NetDataWriter();
                                            startWriter.Put("StartGame");
                                            BroadcastMessage(server, startWriter,
                                                             DeliveryMethod.ReliableOrdered);
                                            Log("[Server] Alle Spieler bereit. Spiel wird gestartet!");
                                            _gameSessionTimer.Start();
                                            Log("[Server] GameSessionTimer gestartet.");
                                        }
                                        break;

                                    case "ChatMessage":
                                        string chatMsg = reader.GetString();
                                        string senderIdentifier = $"Player_{peer.Port}";
                                        Log($"[Server] Chat von {senderIdentifier}: {chatMsg}");
                                        BroadcastChatMessage(server, senderIdentifier, chatMsg);
                                        break;

                                    default:
                                        Log($"[Server] Unbekannter Nachrichtentyp empfangen: {messageType}");
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
                            if (Console.KeyAvailable)
                            {
                                var key = Console.ReadKey(true);
                                if (key.Key == ConsoleKey.Escape)
                                    break;
                            }

                            server?.PollEvents();
                            Thread.Sleep(15);
                        }
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


    private static void BroadcastMessage(NetManager? server, NetDataWriter writer,
                                         DeliveryMethod deliveryMethod)
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
            writer.Put($"Player_{kvp.Key.Split(':').LastOrDefault() ?? kvp.Key}");
            writer.Put(kvp.Value);
        }

        BroadcastMessage(server, writer, DeliveryMethod.ReliableOrdered);

        Log($"[Server] Spielerliste mit {ConnectedPlayers.Count} Spielern gesendet");
    }

    private static void BroadcastChatMessage(NetManager? server, string sender,
                                             string message)
    {
        if (server == null)
            return;

        var writer = new NetDataWriter();
        writer.Put("ChatMessage");
        writer.Put(sender);
        writer.Put(message);

        Log($"[Server] Sende Chat-Nachricht an alle: [{sender}] {message}");

        foreach (var connectedPeer in server.ConnectedPeerList)
        {
            connectedPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }
}