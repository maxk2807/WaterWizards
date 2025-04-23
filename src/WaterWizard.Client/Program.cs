using Raylib_cs;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Threading;

namespace WaterWizard.Client
{
    /// <summary>
    /// Stellt den WaterWizards Client dar.
    /// Ermöglicht die Verbindung zum Server und die Anzeige des Spielstatus.
    /// </summary>
    static class Program
    {
        private static NetManager? client;
        private static EventBasedNetListener? listener;
        private static bool isConnected = false;
        private static string connectionStatus = "Verbindung wird aufgebaut...";
        private static Color statusColor = Color.Orange;

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
                SetupNetwork();
                Raylib.InitWindow(screenWidth, screenHeight, "WaterWizards - Battleship Game");
                Raylib.SetTargetFPS(60);

                while (!Raylib.WindowShouldClose())
                {
                    client?.PollEvents();

                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Color.Beige);
                    Raylib.DrawText(connectionStatus, 10, 10, 20, statusColor);
                    Raylib.DrawText("Welcome to WaterWizards!", screenWidth / 3, screenHeight / 2, 20, Color.DarkBlue);
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
                if (Raylib.IsWindowReady())
                {
                    Raylib.CloseWindow();
                }
            }
        }

        /// <summary>
        /// Initialisiert die Netzwerkverbindung zum Server.
        /// Erstellt Listener und NetManager und verbindet sich mit dem Server.
        /// </summary>
        static void SetupNetwork()
        {
            try
            {
                listener = new EventBasedNetListener();
                client = new NetManager(listener);
                client.Start();

                Console.WriteLine("Verbinde mit Server auf localhost:9050...");
                client.Connect("localhost", 9050, "");

                listener.PeerConnectedEvent += peer =>
                {
                    Console.WriteLine($"Verbunden mit Server: {peer}");
                    isConnected = true;
                    connectionStatus = "Verbunden mit Server!";
                    statusColor = Color.Green;
                    var writer = new NetDataWriter();
                    writer.Put("Hallo vom Client!");
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                };

                listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
                {
                    Console.WriteLine($"Verbindung zum Server getrennt: {disconnectInfo.Reason}");
                    isConnected = false;
                    connectionStatus = "Verbindung getrennt: " + disconnectInfo.Reason;
                    statusColor = Color.Red;
                };

                listener.NetworkErrorEvent += (endPoint, socketError) =>
                {
                    Console.WriteLine($"Netzwerkfehler: {socketError} von {endPoint}");
                    connectionStatus = "Netzwerkfehler: " + socketError;
                    statusColor = Color.Red;
                };

                listener.NetworkReceiveEvent += (fromPeer, dataReader, channelNumber, deliveryMethod) =>
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
                connectionStatus = "Fehler beim Verbinden";
                statusColor = Color.Red;
            }
        }
    }
}