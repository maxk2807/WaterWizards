using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server;

namespace WaterWizard.Server.handler;

/// <summary>
/// Verwaltet die Mana-Logik und berücksichtigt Paralize-Effekte.
/// </summary>
public class ManaHandler
{
    private readonly GameState gameState;
    private readonly ParalizeHandler paralizeHandler;

    public ManaHandler(GameState gameState, ParalizeHandler paralizeHandler)
    {
        this.gameState = gameState;
        this.paralizeHandler = paralizeHandler;
    }

    /// <summary>
    /// Aktualisiert das Mana für beide Spieler und berücksichtigt Paralize-Effekte
    /// </summary>
    public void UpdateMana()
    {
        // Aktualisiere Paralize-Timer
        paralizeHandler.UpdateParalizeTimers(4000f); // 4 Sekunden = 4000ms

        // Gebe Mana nur hinzu, wenn der Spieler nicht paralysiert ist
        if (!paralizeHandler.IsPlayerParalized(0))
        {
            gameState.Player1Mana.Add(1);
            Console.WriteLine($"[ManaHandler] Player 1 Mana +1 (Neuer Stand: {gameState.Player1Mana.CurrentMana})");
        }
        else
        {
            Console.WriteLine("[ManaHandler] Player 1 paralyzed - no mana gained");
            Console.WriteLine($"[ManaHandler] Player 1 Mana bleibt bei {gameState.Player1Mana.CurrentMana} (Paralize-Effekt aktiv)");
        }

        if (!paralizeHandler.IsPlayerParalized(1))
        {
            gameState.Player2Mana.Add(1);
            Console.WriteLine($"[ManaHandler] Player 2 Mana +1 (Neuer Stand: {gameState.Player2Mana.CurrentMana})");
        }
        else
        {
            Console.WriteLine("[ManaHandler] Player 2 paralyzed - no mana gained");
            Console.WriteLine($"[ManaHandler] Player 2 Mana bleibt bei {gameState.Player2Mana.CurrentMana} (Paralize-Effekt aktiv)");
        }

        // Sende Mana-Updates an alle Clients
        SendManaUpdates();
    }

    /// <summary>
    /// Sendet Mana-Updates an alle verbundenen Clients
    /// </summary>
    private void SendManaUpdates()
    {
        for (int i = 0; i < gameState.Server.ConnectedPeersCount; i++)
        {
            var peer = gameState.Server.ConnectedPeerList[i];
            var writer = new NetDataWriter();
            writer.Put("UpdateMana");
            writer.Put(i); // Spielerindex
            writer.Put(
                i == 0 ? gameState.Player1Mana.CurrentMana : gameState.Player2Mana.CurrentMana
            );
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    /// <summary>
    /// Prüft, ob ein Spieler genug Mana hat, um eine Karte zu spielen
    /// </summary>
    /// <param name="playerIndex">Index des Spielers (0 oder 1)</param>
    /// <param name="manaCost">Mana-Kosten der Karte</param>
    /// <returns>True wenn der Spieler genug Mana hat</returns>
    public bool CanSpendMana(int playerIndex, int manaCost)
    {
        var mana = playerIndex == 0 ? gameState.Player1Mana : gameState.Player2Mana;
        return mana.CurrentMana >= manaCost;
    }

    /// <summary>
    /// Gibt Mana für eine Karte aus
    /// </summary>
    /// <param name="playerIndex">Index des Spielers (0 oder 1)</param>
    /// <param name="manaCost">Mana-Kosten der Karte</param>
    /// <returns>True wenn das Mana erfolgreich ausgegeben wurde</returns>
    public bool SpendMana(int playerIndex, int manaCost)
    {
        var mana = playerIndex == 0 ? gameState.Player1Mana : gameState.Player2Mana;
        return mana.Spend(manaCost);
    }
}