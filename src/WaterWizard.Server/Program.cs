using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using WaterWizard.Shared;

namespace WaterWizard.Server
{
    static class Program
    {
        private static List<string> connectedPlayers = new List<string>();

        static void Main()
        {
            Console.WriteLine("WaterWizards Server wird gestartet...");

            var listener = new EventBasedNetListener();
            var server = new NetManager(listener) { AutoRecycle = true };

            if (!server.Start(7777))
            {
                Console.WriteLine("Server konnte nicht auf Port 7777 gestartet werden!");
                return;
            }

            string localIp = NetworkUtils.GetLocalIPAddress();
            string publicIp = NetworkUtils.GetPublicIPAddress();
            Console.WriteLine($"Server erfolgreich auf Port 7777 gestartet");
            Console.WriteLine($"Verbinde dich mit der IP-Adresse: {publicIp}:7777");
            Console.WriteLine($"localIp: {localIp}:7777");
            Console.WriteLine("Drücke ESC zum Beenden");

            listener.ConnectionRequestEvent += request =>
            {
                request.Accept();
                Console.WriteLine($"Client verbunden (ohne Schlüssel): {request.RemoteEndPoint}");
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine($"Client {peer} verbunden");

                string playerAddress = peer.ToString();
                if (!connectedPlayers.Contains(playerAddress))
                {
                    connectedPlayers.Add(playerAddress);
                }

                var writer = new NetDataWriter();
                writer.Put("EnterLobby");
                peer.Send(writer, DeliveryMethod.ReliableOrdered);

                SendPlayerList(server);
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Console.WriteLine($"Client {peer} getrennt: {disconnectInfo.Reason}");

                string playerAddress = peer.ToString();
                if (connectedPlayers.Contains(playerAddress))
                {
                    connectedPlayers.Remove(playerAddress);
                }

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

        private static void SendPlayerList(NetManager server)
        {
            var writer = new NetDataWriter();
            writer.Put("PlayerList");
            writer.Put(connectedPlayers.Count);

            foreach (var playerAddress in connectedPlayers)
            {
                writer.Put(playerAddress);     
                writer.Put("Player");          
                writer.Put(false);             
            }

            foreach (var peer in server.ConnectedPeerList)
            {
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            }

            Console.WriteLine($"Spielerliste mit {connectedPlayers.Count} Spielern gesendet");
        }
    }
}
