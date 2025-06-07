using LiteNetLib;
using WaterWizard.Client.gamescreen.handler;

namespace WaterWizard.Client.network;

/// <summary>
/// Handles the connection to the game server.
/// </summary>
public class ServerConnection
{
    /// <summary>
    /// Connects to the game server using the specified IP address and port.
    /// </summary>
    /// <param name="ip">The IP address of the server to connect to. Can optionally include a port (e.g., "127.0.0.1:7777").</param>
    /// <param name="port">The port number to connect to. Defaults to 7777 if not specified or not included in the <paramref name="ip"/>.</param>
    public static void ConnectToServer(string ip, int port = 7777)
    {
        ClientService clientService = NetworkManager.Instance.clientService;

        if (
            clientService.client != null
            && clientService.client.FirstPeer != null
            && clientService.client.FirstPeer.ConnectionState == ConnectionState.Connected
        )
        {
            Console.WriteLine("[Client] Bereits mit dem Server verbunden.");
            return;
        }

        clientService.CleanupIfRunning();


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

            clientService.clientListener = new EventBasedNetListener();
            clientService.client = new NetManager(clientService.clientListener)
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

            if (!clientService.client.Start())
            {
                Console.WriteLine("[Client] Fehler beim Starten des Network Clients");
                GameStateManager.Instance.ChatLog.AddMessage(
                    "[Error] Failed to start network client"
                );
                return;
            }

            clientService.SetupClientEventHandlers();

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

            clientService.client.Connect(cleanIp, connectionPort, "WaterWizardClient");
            Console.WriteLine($"Verbindungsanfrage gesendet an {cleanIp}:{connectionPort}...");

            int connectionAttempts = 0;
            const int maxAttempts = 20;

            System.Timers.Timer connectionTimer = new(500);
            connectionTimer.Elapsed += (s, e) =>
            {
                connectionAttempts++;

                clientService.client.PollEvents();

                bool isConnected =
                    clientService.client.FirstPeer != null
                    && clientService.client.FirstPeer.ConnectionState == ConnectionState.Connected;

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
}