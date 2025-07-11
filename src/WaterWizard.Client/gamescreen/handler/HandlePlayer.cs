// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 62 Zeilen
// - Erickk0: 2 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamescreen.handler;

public static class HandlePlayer
{
    /// <summary>
    /// Handles the player list update message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandlePlayerListUpdate(NetPacketReader reader)
    {
        var connectedPlayers = NetworkManager.Instance.clientService.ConnectedPlayers;

        int count = reader.GetInt();
        Console.WriteLine($"[Client] Empfange Spielerliste mit {count} Spielern.");
        connectedPlayers.Clear();

        for (int i = 0; i < count; i++)
        {
            string address = reader.GetString();
            string name = reader.GetString();
            bool isReady = reader.GetBool();
            connectedPlayers.Add(new Player(address) { Name = name, IsReady = isReady });
            Console.WriteLine($"[Client] Spieler empfangen: {name} ({address}), Bereit: {isReady}");
        }

        if (connectedPlayers.Count == 0)
        {
            GameStateManager.Instance.ResetGame();
            GameStateManager.Instance.SetStateToLobby();
            Console.WriteLine($"[Client] 0 Spieler - GameScreen und Boards zurückgesetzt.");
        }
    }

    /// <summary>
    /// Toggles the ready status of the current client and sends update to server
    /// </summary>
    public static void ToggleReadyStatus()
    {
        var clientService = NetworkManager.Instance.clientService;
        var client = clientService.client;

        clientService.clientReady = !clientService.clientReady;

        if (client != null && client.FirstPeer != null)
        {
            var writer = new NetDataWriter();

            string message = clientService.clientReady ? "PlayerReady" : "PlayerNotReady";
            writer.Put(message);
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Nachricht gesendet: {message}");
        }
        else
        {
            Console.WriteLine(
                "[Client] Kein Server verbunden, Nachricht konnte nicht gesendet werden."
            );
        }
    }
}
