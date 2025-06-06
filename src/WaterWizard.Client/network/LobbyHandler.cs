using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;

namespace WaterWizard.Client.network;

public static class LobbyHandler
{
    internal static void RefreshLobbies()
    {
        RefreshLobbies(NetworkManager.Instance);
    }

    /// <summary>
    /// Handles lobby info responses from server discovery requests.
    /// </summary>
    /// <param name="manager">The NetworkManager instance</param>
    /// <param name="remoteEndPoint">The endpoint that sent the response</param>
    /// <param name="reader">The packet reader containing the lobby info</param>
    /// <param name="messageType">The type of unconnected message</param>
    public static void HandleLobbyInfoResponse(
        NetworkManager manager,
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

    /// <summary>
    /// Sends discovery requests to find available lobbies on the network.
    /// </summary>
    /// <param name="manager">The NetworkManager instance</param>
    public static void SendDiscoveryRequests(NetworkManager manager)
    {
        var client = manager.clientService.client;
        if (client == null || !client.IsRunning)
            return;

        var req = new NetDataWriter();
        req.Put("DiscoverLobbies");

        client.SendBroadcast(req, manager.hostPort);

        try
        {
            client.SendUnconnectedMessage(
                req,
                new IPEndPoint(IPAddress.Parse("91.99.94.11"), manager.hostPort)
            );
            Console.WriteLine("[Client] Discovery request sent to 91.99.94.11");

            client.SendUnconnectedMessage(
                req,
                new IPEndPoint(IPAddress.Loopback, manager.hostPort)
            );
            Console.WriteLine("[Client] Discovery request sent to 127.0.0.1");
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
    public static void RefreshLobbies(NetworkManager manager)
    {
        if (manager.clientService.client != null && manager.clientService.client.IsRunning)
        {
            SendDiscoveryRequests(NetworkManager.Instance);
            manager.discoveredLobbies.Clear();
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

    public static void TryDirectConnection(string ip, int port)
    {
        GameStateManager.Instance.ChatLog.AddMessage(
            "Attempting direct connection as last resort..."
        );
        ConnectToServerDirect(ip, port, NetworkManager.Instance);
    }

    public static void ConnectToServerDirect(string ip, int port, NetworkManager manager)
    {
        try
        {
            ClientService clientService = manager.clientService;
            Console.WriteLine($"[Client] Attempting direct connection to {ip}:{port}...");

            clientService.CleanupIfRunning();

            clientService.clientListener = new EventBasedNetListener();
            manager.clientService.client = new NetManager(clientService.clientListener)
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

            if (!manager.clientService.client.Start())
            {
                Console.WriteLine("[Client] Failed to start network client for direct connection");
                return;
            }

            clientService.SetupClientEventHandlers();

            // Try connection with a key (sometimes helps with certain NAT configurations)
            manager.clientService.client.Connect(ip, port, "WaterWizardClientDirect");
            Console.WriteLine($"[Client] Direct connection request sent to {ip}:{port}");

            // Actively poll events to process connection
            Task.Run(() =>
            {
                for (int i = 0; i < 100; i++) // Poll for 10 seconds
                {
                    manager.clientService.client.PollEvents();
                    Thread.Sleep(100);

                    if (
                        manager.clientService.client.FirstPeer != null
                        && manager.clientService.client.FirstPeer.ConnectionState == ConnectionState.Connected
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
}