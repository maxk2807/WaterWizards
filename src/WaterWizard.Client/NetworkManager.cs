using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace WaterWizard.Client
{
    public class NetworkManager
    {
        private static NetworkManager? instance;
        public static NetworkManager Instance => instance ??= new NetworkManager();

        private NetManager? server;
        private NetManager? client;
        private EventBasedNetListener? serverListener;
        private EventBasedNetListener? clientListener;
        private bool isPlayerConnected = false;
        private int hostPort = 7777;

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

                Console.WriteLine($"Server gestartet auf Port {hostPort}");
                serverListener.ConnectionRequestEvent += request => request.Accept();
                serverListener.PeerConnectedEvent += peer =>
                {
                    Console.WriteLine($"Client {peer} verbunden");
                    isPlayerConnected = true;
                };
                serverListener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
                {
                    Console.WriteLine($"Client {peer} getrennt: {disconnectInfo.Reason}");
                    isPlayerConnected = false;
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Hosten: {ex.Message}");
            }
        }

        public void ConnectToServer(string ip, int port)
        {
            try
            {
                clientListener = new EventBasedNetListener();
                client = new NetManager(clientListener);
                client.Start();

                client.Connect(ip, port, "");
                clientListener.PeerConnectedEvent += peer => Console.WriteLine($"Verbunden mit Server: {peer}");
                clientListener.NetworkReceiveEvent += (peer, reader, channelNumber, deliveryMethod) =>
                {
                    string message = reader.GetString();
                    Console.WriteLine($"Nachricht vom Server: {message}");

                    // Update the host UI if a player connects
                    if (message == "Player Connected")
                    {
                        isPlayerConnected = true;
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Verbinden: {ex.Message}");
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
    }
}
