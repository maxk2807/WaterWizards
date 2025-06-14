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
}
