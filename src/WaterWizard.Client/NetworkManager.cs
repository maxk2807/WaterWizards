// NetworkManager.cs
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WaterWizard.Shared; // Behält die using-Anweisungen bei

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
            if (client != null && client.IsRunning)
            {
                client.Stop();
            }

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

            serverListener.ConnectionRequestEvent += request => request.Accept();

            serverListener.NetworkReceiveUnconnectedEvent += (remoteEndPoint, reader, msgType) =>
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
                        server.SendUnconnectedMessage(response, remoteEndPoint);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Host] Fehler bei Verarbeitung unverbundener Nachricht: {ex.Message}");
                }
            };

            serverListener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine($"Client {peer} verbunden");
                isPlayerConnected = true;

                string playerAddress = peer.ToString();
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
                isPlayerConnected = server.ConnectedPeersCount > 0;

                string playerAddress = peer.ToString();
                RemovePlayerByAddress(playerAddress);
                UpdatePlayerList();
            };

            serverListener.NetworkReceiveEvent += (peer, reader, channelNumber, deliveryMethod) =>
            {
                try
                {
                    string message = reader.GetString();
                    Console.WriteLine($"[Host] Nachricht von Client erhalten: {message}");

                    if (message == "PlayerReady" || message == "PlayerNotReady")
                    {
                        bool isReady = message == "PlayerReady";
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Host] Fehler beim Verarbeiten der Nachricht: {ex.Message}");
                }
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
            Console.WriteLine("[Client] Suche nach verfgbaren Lobbies...");
        }

        if (client != null && client.IsRunning)
        {
            client.Stop();
        }

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

        clientListener.NetworkReceiveUnconnectedEvent += (remoteEndPoint, reader, messageType) =>
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
        };

        var req = new NetDataWriter();
        req.Put("DiscoverLobbies");

        client.SendBroadcast(req, hostPort);

        try
        {
            client.SendUnconnectedMessage(req, new IPEndPoint(IPAddress.Loopback, hostPort));
            Console.WriteLine("[Client] Discovery-Request an localhost gesendet");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Fehler beim Senden an localhost: {ex.Message}");
        }

        try
        {
            string localIp = NetworkUtils.GetLocalIPAddress();
            if (localIp != "127.0.0.1")
            {
                client.SendUnconnectedMessage(req, new IPEndPoint(IPAddress.Parse(localIp), hostPort));
                Console.WriteLine($"[Client] Discovery-Request an lokale IP {localIp} gesendet");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Fehler beim Senden an lokale IP: {ex.Message}");
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
            var req = new NetDataWriter();
            req.Put("DiscoverLobbies");
            client.SendBroadcast(req, hostPort);

            try
            {
                client.SendUnconnectedMessage(req, new IPEndPoint(IPAddress.Loopback, hostPort));
                client.SendUnconnectedMessage(req, new IPEndPoint(IPAddress.Parse("127.0.0.1"), hostPort));

                string localIp = NetworkUtils.GetLocalIPAddress();
                client.SendUnconnectedMessage(req, new IPEndPoint(IPAddress.Parse(localIp), hostPort));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Fehler bei gezielten Anfragen: {ex.Message}");
            }

            Console.WriteLine("[Client] Lobby-Liste aktualisiert");

            if (IsHost())
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
                    Console.WriteLine("[Client] Lokale Lobby manuell zur Liste hinzugefgt");
                }
            }
        }
        else
        {
            DiscoverLobbies();
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
                Console.WriteLine($"[Client] Nachricht vom Server empfangen: {message}");

                if (message == "StartGame")
                {
                    GameStateManager.Instance.SetStateToInGame();
                }

                if (message == "EnterLobby")
                {
                    Console.WriteLine("[Client] Betrete die Lobby...");
                    GameStateManager.Instance.SetStateToLobby();
                }
                else if (message == "PlayerList")
                {
                    connectedPlayers.Clear();
                    int count = reader.GetInt();

                    for (int i = 0; i < count; i++)
                    {
                        string address = reader.GetString();
                        string name = reader.GetString();
                        bool isReady = reader.GetBool();

                        Player player = new Player(address) { Name = name, IsReady = isReady };
                        connectedPlayers.Add(player);
                        Console.WriteLine($"[Client] Spieler aktualisiert: {player.Name}, Status: {(isReady ? "bereit" : "nicht bereit")}");
                    }
                    GameStateManager.Instance.UpdateAndDraw();
                }
            };

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Verbinden mit dem Server: {ex.Message}");
        }
    }

    public int GetHostPort() => hostPort;

    public bool IsPlayerConnected() => isPlayerConnected || (client != null && client.FirstPeer != null && client.FirstPeer.ConnectionState == ConnectionState.Connected);

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
}
