// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 51 Zeilen
// - Erickk0: 1 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles the cell position message received from the server.
/// </summary>
public class HandleCell
{
    /// <summary>
    /// Handles the cell reveal message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleCellReveal(NetPacketReader reader)
    {
        int x = reader.GetInt();
        int y = reader.GetInt();
        bool isHit = reader.GetBool();
        bool isDefender = reader.GetBool();

        var gameScreen = GameStateManager.Instance.GameScreen;
        if (gameScreen != null)
        {
            if (isDefender)
            {
                var playerBoard = gameScreen.playerBoard;
                if (playerBoard != null)
                {
                    playerBoard.SetCellState(
                        x,
                        y,
                        isHit ? Gamescreen.CellState.Hit : Gamescreen.CellState.Miss
                    );
                    Console.WriteLine($"[Client] Defender - Cell revealed on own board: ({x},{y}) = {(isHit ? "hit" : "miss")}");
                }
            }
            else
            {
                var opponentBoard = gameScreen.opponentBoard;
                if (opponentBoard != null)
                {
                    opponentBoard.SetCellState(
                        x,
                        y,
                        isHit ? Gamescreen.CellState.Hit : Gamescreen.CellState.Miss
                    );
                    Console.WriteLine($"[Client] Attacker - Cell revealed on opponent board: ({x},{y}) = {(isHit ? "hit" : "miss")}");
                }
            }
        }
    }
}
