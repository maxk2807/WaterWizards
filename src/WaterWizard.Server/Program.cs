using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Shared;

namespace WaterWizard.Server;

static class Program
{
    private static readonly Dictionary<string, bool> ConnectedPlayers = [];
    private static GameSessionTimer? _gameSessionTimer;
    static void Main()
    {
        Console.WriteLine("WaterWizards Server wird gestartet...");

        var listener = new EventBasedNetListener();
        var server = new NetManager(listener)
        {
            AutoRecycle = true,
            UnconnectedMessagesEnabled = true
        };

        _gameSessionTimer = new GameSessionTimer(server);

        if (!server.Start(7777))
        {
            Console.WriteLine("Server konnte nicht auf Port 7777 gestartet werden!");
            _gameSessionTimer?.Dispose();
            return;
        }

        string localIp = NetworkUtils.GetLocalIPAddress();
        string publicIp = NetworkUtils.GetPublicIPAddress();
        Console.WriteLine($"Server erfolgreich auf Port 7777 gestartet");
        Console.WriteLine($"Verbinde dich mit der IP-Adresse: {publicIp}:7777");
        Console.WriteLine($"localIp: {localIp}:7777");
        Console.WriteLine("Drücke ESC zum Beenden");

        listener.NetworkReceiveUnconnectedEvent += (remoteEndPoint, reader, msgType) =>
        {
            try
            {
                string message = reader.GetString();
                Console.WriteLine($"Unconnected message: {message} from {remoteEndPoint}");

                if (message == "DiscoverLobbies")
                {
                    var response = new NetDataWriter();
                    response.Put("LobbyInfo");
                    response.Put("WaterWizards Server");
                    response.Put(ConnectedPlayers.Count);
                    server.SendUnconnectedMessage(response, remoteEndPoint);
                    Console.WriteLine($"Sent lobby info to {remoteEndPoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing discovery: {ex.Message}");
            }
        };

        listener.ConnectionRequestEvent += request =>
        {
            request.Accept();
            Console.WriteLine($"Client verbunden (ohne Schlüssel): {request.RemoteEndPoint}");
        };

        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"Client {peer} verbunden");

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
            Console.WriteLine($"Client {peer} getrennt: {disconnectInfo.Reason}");

            var playerAddress = peer.ToString();

            ConnectedPlayers.Remove(playerAddress);

            SendPlayerList(server);
        };

        listener.NetworkErrorEvent += (endPoint, error) =>
        {
            Console.WriteLine($"Netzwerkfehler von {endPoint}: {error}");
        };

        listener.NetworkReceiveEvent += (peer, reader, channelNumber, deliveryMethod) =>
        {
            try
            {
                string message = reader.GetString();
                Console.WriteLine($"Nachricht von Client {peer} (Kanal: {channelNumber}, Methode: {deliveryMethod}): {message}");

                if (message == "PlayerReady" || message == "PlayerNotReady")
                {
                    string playerAddress = peer.ToString();

                    if (ConnectedPlayers.ContainsKey(playerAddress))
                    {
                        ConnectedPlayers[playerAddress] = (message == "PlayerReady");

                        SendPlayerList(server);

                        Console.WriteLine($"Spielerliste nach Ready/NotReady-Status Änderung gesendet.");
                    }
                    bool allReady = true;
                    foreach (var ready in ConnectedPlayers.Values)
                    {
                        if (!ready)
                        {
                            allReady = false;
                            break;
                        }
                    }

                    if (allReady && ConnectedPlayers.Count > 0 && _gameSessionTimer != null && !_gameSessionTimer.IsRunning) // Check if timer not already running
                    {
                        var writer = new NetDataWriter();
                        writer.Put("StartGame");
                        BroadcastMessage(server, writer, DeliveryMethod.ReliableOrdered);


                        Console.WriteLine("Alle Spieler bereit. Spiel wird gestartet!");
                        _gameSessionTimer.Start();

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Verarbeiten der Nachricht: {ex.Message}");
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

            server.PollEvents();
            Thread.Sleep(15);
        }
        _gameSessionTimer?.Dispose();
        Console.WriteLine("Server wird beendet...");

        server.Stop();
        Console.WriteLine("Server beendet");
    }

    private static void BroadcastMessage(NetManager server, NetDataWriter writer, DeliveryMethod deliveryMethod)
    {
        foreach (var connectedPeer in server.ConnectedPeerList)
        {
            connectedPeer.Send(writer, deliveryMethod);
        }
    }

    private static void SendPlayerList(NetManager server)
    {
        var writer = new NetDataWriter();
        writer.Put("PlayerList");
        writer.Put(ConnectedPlayers.Count);

        foreach (var kvp in ConnectedPlayers)
        {
            writer.Put(kvp.Key);
            writer.Put("Player");
            writer.Put(kvp.Value);
        }

        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        Console.WriteLine($"Spielerliste mit {ConnectedPlayers.Count} Spielern gesendet");
    }
}

