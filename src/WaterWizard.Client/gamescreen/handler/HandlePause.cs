// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 25 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamescreen.handler
{
    public static class HandlePause
    {
        public static void SendPauseToggleRequest()
        {
            var client = NetworkManager.Instance.clientService?.client;
            if (client != null && client.FirstPeer != null)
            {
                var writer = new NetDataWriter();
                writer.Put("PauseToggle");
                client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
                Console.WriteLine("[Client] PauseToggle request sent to server.");
            }
            else
            {
                Console.WriteLine("[Client] Cannot send PauseToggle request, not connected to a server.");
            }
        }
    }
} 