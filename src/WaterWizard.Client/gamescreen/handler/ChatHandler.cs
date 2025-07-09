// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 39 Zeilen
// - Erickk0: 2 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamescreen.handler;

public class ChatHandler
{
    /// <summary>
    /// Sends a chat message from the client to the server/host.
    /// </summary>
    /// <param name="message">The chat message text.</param>
    public static void SendChatMessage(string message, NetworkManager manager)
    {
        ClientService clientService = NetworkManager.Instance.clientService;

        if (
            clientService.client == null
            || clientService.client.FirstPeer == null
            || clientService.client.FirstPeer.ConnectionState != ConnectionState.Connected
        )
        {
            Console.WriteLine("[Client] Cannot send chat message: Not connected to a server.");
            return;
        }

        try
        {
            var writer = new NetDataWriter();
            writer.Put("ChatMessage");
            writer.Put(message);
            clientService.client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            // Zeige die eigene Nachricht sofort im Chatfenster an
            GameStateManager.Instance.ChatLog.AddMessage($"{Environment.UserName}: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error sending chat message: {ex.Message}");
        }
    }
}
