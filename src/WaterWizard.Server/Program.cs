using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;
using System.Threading;

namespace WaterWizard.Server
{
    /// <summary>
    /// Stellt den WaterWizards Server dar.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Serveranwendung.
        /// Initialisiert das Netzwerk, verarbeitet Verbindungen und Nachrichten.
        /// </summary>
        static void Main()
        {
            Console.WriteLine("WaterWizards Server wird gestartet...");

            var listener = new EventBasedNetListener();
            var server = new NetManager(listener) { AutoRecycle = true };

            if (!server.Start(9050))
            {
                Console.WriteLine("Server konnte nicht auf Port 9050 gestartet werden!");
                return;
            }

            Console.WriteLine("Server erfolgreich auf Port 9050 gestartet");
            Console.WriteLine("Drücke ESC zum Beenden");

            listener.ConnectionRequestEvent += request =>
            {
                request.Accept();
                Console.WriteLine($"Client verbunden (ohne Schlüssel): {request.RemoteEndPoint}");
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine($"Client {peer} verbunden");
                var writer = new NetDataWriter();
                writer.Put("Willkommen beim WaterWizards Server!");
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Console.WriteLine($"Client {peer} getrennt: {disconnectInfo.Reason}");
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
                    var writer = new NetDataWriter();
                    writer.Put($"Echo: {message}");
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
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

            server.Stop();
            Console.WriteLine("Server beendet");
        }
    }
}