// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 38 Zeilen
// - Erickk0: 30 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles resource-related messages received from the server.
/// </summary>
public class HandleRessources
{
    /// <summary>
    /// Handles the update mana message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleUpdateMana(NetPacketReader reader)
    {
        int playerIndex = reader.GetInt();
        int mana = reader.GetInt();
        Console.WriteLine($"[Client] Spieler {playerIndex} hat nun {mana} Mana.");

        var ressourceField = GameStateManager.Instance.GameScreen.ressourceField!;
        ressourceField.SetMana(mana);
        ressourceField.ManaFieldUpdate();
    }

    /// <summary>
    /// Handles the update gold message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleUpdateGold(NetPacketReader reader)
    {
        int playerIndex = reader.GetInt();
        int gold = reader.GetInt();
        Console.WriteLine($"[Client] Spieler {playerIndex} hat nun {gold} Gold.");

        var ressourceField = GameStateManager.Instance.GameScreen.ressourceField!;
        ressourceField.SetGold(gold);
        ressourceField.GoldFieldUpdate();
    }

    /// <summary>
    /// Hnadles the Message effect of the Gold Freeze and shows it on the client that the gold is frozen
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleGoldFreeze(NetPacketReader reader)
    {
        int playerIndex = reader.GetInt();
        bool isFrozen = reader.GetBool();
        int myPlayerIndex = GameStateManager.Instance.MyPlayerIndex;

        Console.WriteLine($"[Client] GoldFreezeStatus empfangen - PlayerIndex: {playerIndex}, IsFrozen: {isFrozen}");
        Console.WriteLine($"[Client] Mein PlayerIndex: {myPlayerIndex}");
        Console.WriteLine($"[Client] Betrifft mich: {playerIndex == myPlayerIndex}");

        if (playerIndex == myPlayerIndex)
        {
            var ressourceField = GameStateManager.Instance.GameScreen?.ressourceField;
            if (ressourceField != null)
            {
                ressourceField.SetGoldFrozen(isFrozen);
                Console.WriteLine($"[Client] Gold-Freeze-Status in RessourceField gesetzt: {isFrozen}");
            }
        }
        else
        {
            Console.WriteLine($"[Client] Gold-Freeze-Status betrifft mich nicht, ignoriere");
        }
    }
}
