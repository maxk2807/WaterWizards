using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WaterWizard.Client
{
    public class NetworkManager
    {
        private static NetworkManager? instance;
        public static NetworkManager Instance => instance ??= new NetworkManager();
        private List<Player> connectedPlayers = new List<Player>();

        private NetManager? server;
        private NetManager? client;
        private EventBasedNetListener? serverListener;
        private EventBasedNetListener? clientListener;
        private bool isPlayerConnected = false;
        private int hostPort = 7777;
        private bool clientReady = false;

        private NetworkManager() { }

        public void StartHosting()
        {
            try
            {
                serverListener = new EventBasedNetListener();
                server = new NetManager(serverListener) { AutoRecycle = true };

                if (!server.Start(hostPort))
                {
                    Console.WriteLine("Server konnte nicht gestartet werden!");
                    return;
                }
                connectedPlayers.Add(new Player("Host") { Name = "Host (You)", IsReady = true });

                Console.WriteLine($"Server gestartet auf Port {hostPort}");
                serverListener.ConnectionRequestEvent += request => request.Accept();
                serverListener.PeerConnectedEvent += peer =>
                {
                    Console.WriteLine($"Client {peer} verbunden");
                    isPlayerConnected = true;

                    string playerAddress = peer.ToString();
                    // Überprüfe, ob die Adresse bereits in der Spielerliste vorhanden ist
                    if (!PlayerExists(playerAddress))
                    {
                        connectedPlayers.Add(new Player(playerAddress));
                    }

                    var writer = new NetDataWriter();
                    writer.Put("EnterLobby");
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                    UpdatePlayerList();
                };
                serverListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
                {
                    Console.WriteLine($"Client {peer} getrennt: {disconnectInfo.Reason}");
                    isPlayerConnected = false;

                    string playerAddress = peer.ToString();
                    RemovePlayerByAddress(playerAddress);
                    UpdatePlayerList();
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Hosten: {ex.Message}");
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
            return server != null;
        }

        private void UpdatePlayerList()
        {
            if (server == null) return;

            // Erstelle eine Nachricht mit allen verbundenen Spielern
            var writer = new NetDataWriter();
            writer.Put("PlayerList");
            writer.Put(connectedPlayers.Count);

            foreach (var player in connectedPlayers)
            {
                // Sende die Player-Informationen als Strings
                writer.Put(player.Address);
                writer.Put(player.Name);
                writer.Put(player.IsReady);
            }

            // Sende die Liste an alle verbundenen Clients
            foreach (var peer in server.ConnectedPeerList)
            {
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }

        public void ConnectToServer(string ip, int port)
        {
            try
            {
                Console.WriteLine($"Versuche, eine Verbindung zum Server herzustellen: IP={ip}, Port={port}");

                clientListener = new EventBasedNetListener();
                client = new NetManager(clientListener);
                client.Start();

                client.Connect(ip, port, "");
                Console.WriteLine("Verbindungsanfrage gesendet...");

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

                clientListener.NetworkReceiveEvent += (peer, reader, channelNumber, deliveryMethod) =>
                {
                    string message = reader.GetString();
                    Console.WriteLine($"Nachricht vom Server empfangen: {message}");

                    if (message == "EnterLobby" || message == "Player Connected")
                    {
                        Console.WriteLine("Betrete die Pre-Start-Lobby...");
                        GameStateManager.Instance.SetStateToLobby();
                    }
                    else if (message == "StartGame")
                    {
                        Console.WriteLine("Spiel wird gestartet...");
                        GameStateManager.Instance.SetStateToInGame();
                    }
                    else if (message == "PlayerList")
                    {
                        // Spielerliste empfangen
                        connectedPlayers.Clear();
                        int count = reader.GetInt();

                        for (int i = 0; i < count; i++)
                        {
                            string address = reader.GetString();
                            string name = reader.GetString();
                            bool isReady = reader.GetBool();

                            Player player = new Player(address) { Name = name, IsReady = isReady };
                            connectedPlayers.Add(player);
                        }

                        Console.WriteLine($"Spielerliste aktualisiert: {count} Spieler verbunden");
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Verbinden mit dem Server: {ex.Message}");
            }
        }


        public int GetHostPort() => hostPort;

        public bool IsPlayerConnected() => isPlayerConnected;

        public void PollEvents()
        {
            server?.PollEvents();
            client?.PollEvents();
        }

        public void Shutdown()
        {
            server?.Stop();
            client?.Stop();
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
                writer.Put(clientReady ? "PlayerReady" : "PlayerNotReady");
                client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }



    public class Player
    {
        public string Address { get; set; }
        public string Name { get; set; } = "Player";
        public bool IsReady { get; set; } = false;

        public Player(string address)
        {
            Address = address;
        }

        public override string ToString()
        {
            return $"{Name} ({(IsReady ? "Ready" : "Not Ready")})";
        }
    }
}
