// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Erickk0: 89 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles shield-related messages received from the server.
/// </summary>
public static class HandleShield
{
    /// <summary>
    /// Handles the shield created message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the shield data</param>
    public static void HandleShieldCreated(NetPacketReader reader)
    {
        int playerIndex = reader.GetInt();
        int x = reader.GetInt();
        int y = reader.GetInt();
        float duration = reader.GetFloat();

        var gameScreen = GameStateManager.Instance.GameScreen;
        if (gameScreen != null)
        {
            int myPlayerIndex = GameStateManager.Instance.MyPlayerIndex;
            
            // Determine which board to apply the shield visual to
            GameBoard? targetBoard = null;
            if (playerIndex == myPlayerIndex)
            {
                // Shield on my own board
                targetBoard = gameScreen.playerBoard;
                Console.WriteLine($"[Client] Shield created on MY board at ({x}, {y}) for {duration} seconds");
            }
            else
            {
                // Shield on opponent's board (visible to me)
                targetBoard = gameScreen.opponentBoard;
                Console.WriteLine($"[Client] Shield created on OPPONENT's board at ({x}, {y}) for {duration} seconds");
            }

            if (targetBoard != null)
            {
                targetBoard.AddShieldEffect(x, y, duration);
            }
        }
        
        Console.WriteLine($"[Client] Shield created: Player {playerIndex + 1} at ({x}, {y}) duration: {duration}s");
    }

    /// <summary>
    /// Handles the shield expired message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the shield expiration data</param>
    public static void HandleShieldExpired(NetPacketReader reader)
    {
        int playerIndex = reader.GetInt();
        int x = reader.GetInt();
        int y = reader.GetInt();

        var gameScreen = GameStateManager.Instance.GameScreen;
        if (gameScreen != null)
        {
            int myPlayerIndex = GameStateManager.Instance.MyPlayerIndex;
            
            // Determine which board to remove the shield visual from
            GameBoard? targetBoard = null;
            if (playerIndex == myPlayerIndex)
            {
                // Shield on my own board
                targetBoard = gameScreen.playerBoard;
                Console.WriteLine($"[Client] Shield expired on MY board at ({x}, {y})");
            }
            else
            {
                // Shield on opponent's board (visible to me)
                targetBoard = gameScreen.opponentBoard;
                Console.WriteLine($"[Client] Shield expired on OPPONENT's board at ({x}, {y})");
            }

            if (targetBoard != null)
            {
                targetBoard.RemoveShieldEffect(x, y);
            }
        }
        
        Console.WriteLine($"[Client] Shield expired: Player {playerIndex + 1} at ({x}, {y})");
    }
}
