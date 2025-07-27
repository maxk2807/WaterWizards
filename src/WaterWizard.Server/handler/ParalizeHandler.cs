// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 126 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server;

namespace WaterWizard.Server.handler;

/// <summary>
/// Verwaltet den Paralize-Effekt, der den Mana-Timer eines Spielers für eine bestimmte Zeit stoppt.
/// </summary>
public class ParalizeHandler
{
    private readonly GameState gameState;

    // Paralize-Timer für jeden Spieler (in Sekunden für Konsistenz)
    private float player1ParalizeTimer = 0f;
    private float player2ParalizeTimer = 0f;

    public bool IsPlayer1Paralized => player1ParalizeTimer > 0f;
    public bool IsPlayer2Paralized => player2ParalizeTimer > 0f;

    public ParalizeHandler(GameState gameState)
    {
        this.gameState = gameState;
    }

    /// <summary>
    /// Aktiviert den Paralize-Effekt für den angegebenen Spieler
    /// </summary>
    /// <param name="playerIndex">Index des Spielers (0 oder 1)</param>
    /// <param name="durationSeconds">Dauer des Paralize-Effekts in Sekunden</param>
    public void ActivateParalize(int playerIndex, float durationSeconds)
    {
        Console.WriteLine($"[ParalizeHandler] Aktiviere Paralize-Effekt für Player {playerIndex + 1}");
        Console.WriteLine($"[ParalizeHandler] Dauer: {durationSeconds} Sekunden");

        if (playerIndex == 0)
        {
            player1ParalizeTimer = durationSeconds; // Direkt in Sekunden speichern
            Console.WriteLine($"[ParalizeHandler] Player 1 paralyzed for {durationSeconds} seconds");
            Console.WriteLine($"[ParalizeHandler] Player 1 Mana-Timer gestoppt - keine Mana-Generierung während Paralize");
        }
        else if (playerIndex == 1)
        {
            player2ParalizeTimer = durationSeconds; // Direkt in Sekunden speichern
            Console.WriteLine($"[ParalizeHandler] Player 2 paralyzed for {durationSeconds} seconds");
            Console.WriteLine($"[ParalizeHandler] Player 2 Mana-Timer gestoppt - keine Mana-Generierung während Paralize");
        }

        // Sende Update an alle Clients
        SendParalizeStatusToClients();
        Console.WriteLine($"[ParalizeHandler] Paralize-Status an alle Clients gesendet");
    }

    /// <summary>
    /// Aktualisiert die Paralize-Timer basierend auf der verstrichenen Zeit
    /// </summary>
    /// <param name="deltaTimeSeconds">Verstrichene Zeit in Sekunden</param>
    public void UpdateParalizeTimers(float deltaTimeSeconds)
    {
        bool statusChanged = false;

        if (player1ParalizeTimer > 0f)
        {
            player1ParalizeTimer -= deltaTimeSeconds;
            if (player1ParalizeTimer <= 0f)
            {
                player1ParalizeTimer = 0f;
                Console.WriteLine("[ParalizeHandler] Player 1 paralysis ended");
                Console.WriteLine("[ParalizeHandler] Player 1 Mana-Generierung wieder aktiviert");
                statusChanged = true;
            }
        }

        if (player2ParalizeTimer > 0f)
        {
            player2ParalizeTimer -= deltaTimeSeconds;
            if (player2ParalizeTimer <= 0f)
            {
                player2ParalizeTimer = 0f;
                Console.WriteLine("[ParalizeHandler] Player 2 paralysis ended");
                Console.WriteLine("[ParalizeHandler] Player 2 Mana-Generierung wieder aktiviert");
                statusChanged = true;
            }
        }

        // Sende Update an Clients wenn sich der Status geändert hat
        if (statusChanged)
        {
            SendParalizeStatusToClients();
            Console.WriteLine("[ParalizeHandler] Paralize-Status-Update an Clients gesendet (Effekt beendet)");
        }
    }

    /// <summary>
    /// Prüft, ob ein Spieler paralysiert ist
    /// </summary>
    /// <param name="playerIndex">Index des Spielers (0 oder 1)</param>
    /// <returns>True wenn der Spieler paralysiert ist</returns>
    public bool IsPlayerParalized(int playerIndex)
    {
        return playerIndex == 0 ? IsPlayer1Paralized : IsPlayer2Paralized;
    }

    /// <summary>
    /// Sendet den aktuellen Paralize-Status an alle Clients
    /// </summary>
    private void SendParalizeStatusToClients()
    {
        Console.WriteLine($"[ParalizeHandler] Sende Paralize-Status an {gameState.Server.ConnectedPeersCount} Clients");

        for (int i = 0; i < gameState.Server.ConnectedPeersCount; i++)
        {
            var peer = gameState.Server.ConnectedPeerList[i];
            bool isParalized = IsPlayerParalized(i);

            var writer = new NetDataWriter();
            writer.Put("ParalizeStatus");
            writer.Put(i); // Spielerindex
            writer.Put(isParalized); // Paralize-Status

            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            Console.WriteLine($"[ParalizeHandler] ParalizeStatus gesendet an {peer.ToString()} (Port: {peer.Port}) - PlayerIndex: {i}, IsParalized: {isParalized}");
        }
    }
}