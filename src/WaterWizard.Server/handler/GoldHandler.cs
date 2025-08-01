// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Erickk0: 114 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Shared.ShipType;

namespace WaterWizard.Server.handler;

/// <summary>
/// Verwaltet die Gold-Logik und berücksichtigt Gold-Freeze-Effekte.
/// </summary>
public class GoldHandler
{
    private readonly GameState gameState;

    public GoldHandler(GameState gameState)
    {
        this.gameState = gameState;
    }

    /// <summary>
    /// Aktualisiert das Gold für beide Spieler und berücksichtigt Gold-Freeze-Effekte
    /// </summary>
    public void UpdateGold()
    {
        gameState.UpdateGoldFreezeTimers(4000f);

        if (!gameState.IsPlayerGoldFrozen(0))
        {
            int merchantShipCount = GetMerchantShipCount(0);
            int goldToAdd = 1 + merchantShipCount;

            gameState.SetGold(0, gameState.Player1Gold + goldToAdd);
            Console.WriteLine(
                $"[GoldHandler] Player 1 Gold +{goldToAdd} (Neuer Stand: {gameState.Player1Gold})"
            );
        }
        else
        {
            Console.WriteLine("[GoldHandler] Player 1 gold generation frozen - no gold gained");
            Console.WriteLine(
                $"[GoldHandler] Player 1 Gold bleibt bei {gameState.Player1Gold} (Gold-Freeze-Effekt aktiv)"
            );
        }

        if (!gameState.IsPlayerGoldFrozen(1))
        {
            int merchantShipCount = GetMerchantShipCount(1);
            int goldToAdd = 1 + merchantShipCount;

            gameState.SetGold(1, gameState.Player2Gold + goldToAdd);
            Console.WriteLine(
                $"[GoldHandler] Player 2 Gold +{goldToAdd} (Neuer Stand: {gameState.Player2Gold})"
            );
        }
        else
        {
            Console.WriteLine("[GoldHandler] Player 2 gold generation frozen - no gold gained");
            Console.WriteLine(
                $"[GoldHandler] Player 2 Gold bleibt bei {gameState.Player2Gold} (Gold-Freeze-Effekt aktiv)"
            );
        }

        SendGoldUpdates();
    }

    /// <summary>
    /// Sendet Gold-Updates an alle verbundenen Clients
    /// </summary>
    private void SendGoldUpdates()
    {
        for (int i = 0; i < gameState.Server.ConnectedPeersCount; i++)
        {
            var peer = gameState.Server.ConnectedPeerList[i];
            var writer = new NetDataWriter();
            writer.Put("UpdateGold");
            writer.Put(i); // Spielerindex
            writer.Put(i == 0 ? gameState.Player1Gold : gameState.Player2Gold);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    /// <summary>
    /// Prüft, ob ein Spieler genug Gold hat, um eine Karte zu kaufen
    /// </summary>
    /// <param name="playerIndex">Index des Spielers (0 oder 1)</param>
    /// <param name="goldCost">Gold-Kosten der Karte</param>
    /// <returns>True wenn der Spieler genug Gold hat</returns>
    public bool CanSpendGold(int playerIndex, int goldCost)
    {
        var gold = playerIndex == 0 ? gameState.Player1Gold : gameState.Player2Gold;
        return gold >= goldCost;
    }

    /// <summary>
    /// Gibt Gold für eine Karte aus
    /// </summary>
    /// <param name="playerIndex">Index des Spielers (0 oder 1)</param>
    /// <param name="goldCost">Gold-Kosten der Karte</param>
    /// <returns>True wenn das Gold erfolgreich ausgegeben wurde</returns>
    public bool SpendGold(int playerIndex, int goldCost)
    {
        var currentGold = playerIndex == 0 ? gameState.Player1Gold : gameState.Player2Gold;
        if (currentGold >= goldCost)
        {
            gameState.SetGold(playerIndex, currentGold - goldCost);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ermittelt die Anzahl der Handelsschiffe für einen Spieler
    /// </summary>
    /// <param name="playerIndex">Index des Spielers</param>
    /// <returns>Anzahl der Handelsschiffe</returns>
    private int GetMerchantShipCount(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= gameState.players.Length)
            return 0;
            
        var player = gameState.players[playerIndex];
        if (player == null)
            return 0;
            
        var ships = ShipHandler.GetShips(player);
        return ships.Count(ship => ship.ShipType == ShipType.Merchant && !ship.IsDestroyed);
    }
}
