using Raylib_cs;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;

namespace WaterWizard.Client
{
    /// <summary>
    /// Stellt den WaterWizards Client dar.
    /// Ermöglicht die Verbindung zum Server und die Anzeige des Spielstatus.
    /// </summary>
    static class Program
    {
        private static NetManager? client;
        private static NetManager? server;
        private static EventBasedNetListener? clientListener;
        private static EventBasedNetListener? serverListener;
        private static bool isConnected = false;
        private static bool isHosting = false;
        private static string connectionStatus = "Bereit zum Verbinden";
        private static Color statusColor = Color.Orange;
        private const int DefaultPort = 9050;
        private static int hostPort = DefaultPort;

        private enum GameState
        {
            MainMenu,
            ConnectingMenu,
            HostingMenu,
            InGame
        }

        private static GameState currentState = GameState.MainMenu;
        private static string ipAddress = "localhost";
        private static string inputText = "";
        private static bool isEditingIp = false;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Clientanwendung.
        /// Initialisiert Raylib, das Netzwerk und die Spielschleife.
        /// </summary>
        static void Main()
        {
            const int screenWidth = 800;
            const int screenHeight = 600;

            try
            {
                Raylib.InitWindow(screenWidth, screenHeight, "WaterWizards - Battleship Game");
                Raylib.SetTargetFPS(60);

                while (!Raylib.WindowShouldClose())
                {
                    client?.PollEvents();
                    server?.PollEvents();

                    // Input handling
                    if (currentState == GameState.ConnectingMenu && isEditingIp)
                    {
                        HandleTextInput();
                    }

                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Color.Beige);

                    // Display status
                    Raylib.DrawText(connectionStatus, 10, 10, 20, statusColor);

                    // Draw UI based on current state
                    switch (currentState)
                    {
                        case GameState.MainMenu:
                            DrawMainMenu(screenWidth, screenHeight);
                            break;
                        case GameState.ConnectingMenu:
                            DrawConnectMenu(screenWidth, screenHeight);
                            break;
                        case GameState.HostingMenu:
                            DrawHostMenu(screenWidth, screenHeight);
                            break;
                        case GameState.InGame:
                            DrawGameScreen(screenWidth, screenHeight);
                            break;
                    }

                    Raylib.EndDrawing();

                    Thread.Sleep(15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                client?.Stop();
                server?.Stop();
                if (Raylib.IsWindowReady())
                {
                    Raylib.CloseWindow();
                }
            }
        }

        /// <summary>
        /// Verarbeitet die Texteingabe für die IP-Adresse
        /// </summary>
        static void HandleTextInput()
        {
            int key = Raylib.GetCharPressed();
            while (key > 0)
            {
                if ((key >= 48 && key <= 57) || // 0-9
                    (key >= 97 && key <= 122) || // a-z
                    (key >= 65 && key <= 90) || // A-Z
                    key == 46 || key == 58) // . and :
                {
                    inputText += (char)key;
                }
                key = Raylib.GetCharPressed();
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && inputText.Length > 0)
            {
                inputText = inputText.Substring(0, inputText.Length - 1);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                ipAddress = inputText;
                isEditingIp = false;
                ConnectToServer(ipAddress, DefaultPort);
            }
        }

        /// <summary>
        /// Zeichnet das Hauptmenü
        /// </summary>
        static void DrawMainMenu(int screenWidth, int screenHeight)
        {
            Raylib.DrawText("Welcome to WaterWizards!", screenWidth / 3, screenHeight / 3, 30, Color.DarkBlue);

            Rectangle joinButton = new Rectangle(screenWidth / 2 - 100, screenHeight / 2, 200, 40);
            bool joinHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), joinButton);

            Raylib.DrawRectangleRec(joinButton, joinHovered ? Color.SkyBlue : Color.Blue);
            Raylib.DrawText("Join Lobby", (int)joinButton.X + 50, (int)joinButton.Y + 10, 20, Color.White);

            Rectangle hostButton = new Rectangle(screenWidth / 2 - 100, screenHeight / 2 + 60, 200, 40);
            bool hostHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), hostButton);

            Raylib.DrawRectangleRec(hostButton, hostHovered ? Color.SkyBlue : Color.Blue);
            Raylib.DrawText("Host Lobby", (int)hostButton.X + 50, (int)hostButton.Y + 10, 20, Color.White);

            if (joinHovered && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.ConnectingMenu;
                inputText = "localhost";
                isEditingIp = true;
            }

            if (hostHovered && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.HostingMenu;
                StartHosting();
            }
        }

        /// <summary>
        /// Zeichnet das Verbindungsmenü
        /// </summary>
        static void DrawConnectMenu(int screenWidth, int screenHeight)
        {
            Raylib.DrawText("Enter IP Address to Connect:", screenWidth / 3, screenHeight / 3, 20, Color.DarkBlue);

            Rectangle inputBox = new Rectangle(screenWidth / 3, screenHeight / 2, 300, 40);
            Raylib.DrawRectangleRec(inputBox, isEditingIp ? Color.White : Color.LightGray);
            Raylib.DrawRectangleLines((int)inputBox.X, (int)inputBox.Y, (int)inputBox.Width, (int)inputBox.Height, Color.DarkBlue);
            Raylib.DrawText(inputText, (int)inputBox.X + 5, (int)inputBox.Y + 10, 20, Color.Black);

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), inputBox) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                isEditingIp = true;
            }

            Rectangle connectButton = new Rectangle(screenWidth / 2 - 80, screenHeight / 2 + 60, 160, 40);
            bool connectHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), connectButton);

            Raylib.DrawRectangleRec(connectButton, connectHovered ? Color.SkyBlue : Color.Blue);
            Raylib.DrawText("Connect", (int)connectButton.X + 40, (int)connectButton.Y + 10, 20, Color.White);

            Rectangle backButton = new Rectangle(screenWidth / 3, screenHeight / 2 + 120, 100, 40);
            bool backHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);

            Raylib.DrawRectangleRec(backButton, backHovered ? Color.LightGray : Color.Gray);
            Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 10, 20, Color.White);

            if (connectHovered && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                ConnectToServer(inputText, DefaultPort);
            }

            if (backHovered && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.MainMenu;
                isEditingIp = false;
            }
        }

        /// <summary>
        /// Zeichnet das Hosting-Menü
        /// </summary>
        static void DrawHostMenu(int screenWidth, int screenHeight)
        {
            if (isHosting)
            {
                Raylib.DrawText("Hosting a game on:", screenWidth / 3, screenHeight / 3, 25, Color.DarkGreen);
                string localIp = GetLocalIPAddress();
                Raylib.DrawText($"{localIp}:{hostPort}", screenWidth / 3, screenHeight / 3 + 40, 20, Color.Black);
                Raylib.DrawText("Waiting for players to join...", screenWidth / 3, screenHeight / 2, 20, Color.DarkBlue);
            }
            else
            {
                Raylib.DrawText("Failed to start hosting", screenWidth / 3, screenHeight / 3, 25, Color.Red);
                if (connectionStatus.Contains("AddressAlreadyInUse"))
                {
                    Raylib.DrawText("Port bereits in Benutzung!", screenWidth / 3, screenHeight / 3 + 40, 20, Color.Red);
                    Raylib.DrawText("Schließen Sie andere Server-Instanzen und versuchen Sie es erneut.",
                        screenWidth / 4, screenHeight / 3 + 70, 16, Color.Red);
                }
            }

            Rectangle backButton = new Rectangle(screenWidth / 3, screenHeight / 2 + 120, 100, 40);
            bool backHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);

            Raylib.DrawRectangleRec(backButton, backHovered ? Color.LightGray : Color.Gray);
            Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 10, 20, Color.White);

            if (backHovered && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                StopHosting();
                currentState = GameState.MainMenu;
            }
        }

        /// <summary>
        /// Zeichnet den Spielbildschirm
        /// </summary>
        static void DrawGameScreen(int screenWidth, int screenHeight)
        {
            Raylib.DrawText("Game In Progress", screenWidth / 3, screenHeight / 2, 30, Color.DarkBlue);
        }

        /// <summary>
        /// Überprüft, ob ein Port bereits verwendet wird
        /// </summary>
        static bool IsPortInUse(int port)
        {
            bool inUse = false;
            try
            {
                // Prüfe aktive TCP-Verbindungen
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpEndPoints = ipProperties.GetActiveTcpListeners();

                foreach (IPEndPoint endpoint in tcpEndPoints)
                {
                    if (endpoint.Port == port)
                    {
                        inUse = true;
                        break;
                    }
                }
            }
            catch
            {
                // Im Fehlerfall gehen wir davon aus, dass der Port verfügbar ist
                inUse = false;
            }
            return inUse;
        }

        /// <summary>
        /// Findet einen freien Port im Bereich startPort bis startPort+1000
        /// </summary>
        static int FindFreePort(int startPort)
        {
            int port = startPort;
            bool found = false;

            // Suche nach einem freien Port (max. 1000 Versuche)
            while (!found && port < startPort + 1000)
            {
                if (!IsPortInUse(port))
                {
                    found = true;
                }
                else
                {
                    port++;
                }
            }

            return found ? port : -1; // -1 wenn kein freier Port gefunden wurde
        }

        /// <summary>
        /// Startet einen Server zum Hosten eines Spiels
        /// </summary>
        static void StartHosting()
        {
            try
            {
                serverListener = new EventBasedNetListener();
                server = new NetManager(serverListener) { AutoRecycle = true };

                // Zuerst prüfen, ob der Standardport verfügbar ist
                if (IsPortInUse(DefaultPort))
                {
                    // Wenn nicht, nach einem freien Port suchen
                    hostPort = FindFreePort(DefaultPort + 1);
                    if (hostPort == -1)
                    {
                        connectionStatus = "Kein freier Port verfügbar!";
                        statusColor = Color.Red;
                        isHosting = false;
                        return;
                    }
                }
                else
                {
                    hostPort = DefaultPort;
                }

                if (!server.Start(hostPort))
                {
                    connectionStatus = $"Server konnte nicht auf Port {hostPort} gestartet werden!";
                    statusColor = Color.Red;
                    isHosting = false;
                    return;
                }

                isHosting = true;
                connectionStatus = $"Server gestartet auf Port {hostPort}";
                statusColor = Color.Green;

                serverListener.ConnectionRequestEvent += request =>
                {
                    request.Accept();
                    Console.WriteLine($"Client verbunden: {request.RemoteEndPoint}");
                };

                serverListener.PeerConnectedEvent += peer =>
                {
                    Console.WriteLine($"Client {peer} verbunden");
                    var writer = new NetDataWriter();
                    writer.Put("Willkommen beim WaterWizards Spiel!");
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);

                    // Wechsel zum Spielmodus, wenn ein Client verbunden ist
                    currentState = GameState.InGame;
                };

                serverListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
                {
                    Console.WriteLine($"Client {peer} getrennt: {disconnectInfo.Reason}");
                };

                serverListener.NetworkReceiveEvent += (peer, reader, channelNumber, deliveryMethod) =>
                {
                    try
                    {
                        string message = reader.GetString();
                        Console.WriteLine($"Nachricht von Client: {message}");
                    }
                    finally
                    {
                        reader.Recycle();
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Hosten: {ex.Message}");
                connectionStatus = "Fehler beim Hosten: " + ex.Message;
                statusColor = Color.Red;
                isHosting = false;
            }
        }

        /// <summary>
        /// Stoppt den Server
        /// </summary>
        static void StopHosting()
        {
            if (isHosting && server != null)
            {
                server.Stop();
                isHosting = false;
                connectionStatus = "Hosting beendet";
                statusColor = Color.Orange;
            }
        }

        /// <summary>
        /// Erstellt eine Verbindung zum Server
        /// </summary>
        static void ConnectToServer(string ip, int port)
        {
            try
            {
                // Bestehende Verbindung trennen, falls vorhanden
                client?.Stop();

                clientListener = new EventBasedNetListener();
                client = new NetManager(clientListener);
                client.Start();

                Console.WriteLine($"Verbinde mit Server auf {ip}:{port}...");
                connectionStatus = $"Verbinde mit {ip}:{port}...";
                statusColor = Color.Orange;
                client.Connect(ip, port, "");

                clientListener.PeerConnectedEvent += peer =>
                {
                    Console.WriteLine($"Verbunden mit Server: {peer}");
                    isConnected = true;
                    connectionStatus = "Verbunden mit Server!";
                    statusColor = Color.Green;

                    // Wechsel zum Spielmodus
                    currentState = GameState.InGame;

                    var writer = new NetDataWriter();
                    writer.Put("Hallo vom Client!");
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                };

                clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
                {
                    Console.WriteLine($"Verbindung zum Server getrennt: {disconnectInfo.Reason}");
                    isConnected = false;
                    connectionStatus = "Verbindung getrennt: " + disconnectInfo.Reason;
                    statusColor = Color.Red;
                };

                clientListener.NetworkErrorEvent += (endPoint, socketError) =>
                {
                    Console.WriteLine($"Netzwerkfehler: {socketError} von {endPoint}");
                    connectionStatus = "Netzwerkfehler: " + socketError;
                    statusColor = Color.Red;
                };

                clientListener.NetworkReceiveEvent += (fromPeer, dataReader, channelNumber, deliveryMethod) =>
                {
                    try
                    {
                        string message = dataReader.GetString();
                        Console.WriteLine($"Nachricht vom Server (Kanal: {channelNumber}, Methode: {deliveryMethod}): {message}");
                        connectionStatus = "Server: " + message;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Fehler beim Lesen der Nachricht: {ex.Message}");
                    }
                    finally
                    {
                        dataReader.Recycle();
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Netzwerk-Setup: {ex.Message}");
                connectionStatus = "Fehler beim Verbinden: " + ex.Message;
                statusColor = Color.Red;
            }
        }

        /// <summary>
        /// Ermittelt die lokale IP-Adresse des Hosts
        /// </summary>
        static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Ermitteln der IP-Adresse: {ex.Message}");
            }

            return "127.0.0.1"; // Fallback to localhost
        }
    }
}
