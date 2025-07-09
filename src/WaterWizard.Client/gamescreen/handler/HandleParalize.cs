// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 36 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles paralysis-related messages received from the server.
/// </summary>
public class HandleParalize
{
    /// <summary>
    /// Handles the paralysis status update message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the paralysis data sent from the server</param>
    public static void HandleParalizeStatus(NetPacketReader reader)
    {
        int playerIndex = reader.GetInt();
        bool isParalized = reader.GetBool();
        int myPlayerIndex = GameStateManager.Instance.MyPlayerIndex;

        Console.WriteLine($"[Client] ParalizeStatus empfangen - PlayerIndex: {playerIndex}, IsParalized: {isParalized}");
        Console.WriteLine($"[Client] Mein PlayerIndex: {myPlayerIndex}");
        Console.WriteLine($"[Client] Betrifft mich: {playerIndex == myPlayerIndex}");

        // Nur setzen, wenn es mich betrifft
        if (playerIndex == myPlayerIndex)
        {
            var ressourceField = GameStateManager.Instance.GameScreen.ressourceField!;
            ressourceField.SetParalized(isParalized);
            Console.WriteLine($"[Client] Paralize-Status in RessourceField gesetzt: {isParalized}");
        }
        else
        {
            Console.WriteLine($"[Client] Paralize-Status betrifft mich nicht, ignoriere");
        }
    }
}