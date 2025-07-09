// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 83 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handler-Klasse für die Behandlung von Stein-Synchronisation vom Server.
/// </summary>
public static class HandleRocks
{
    /// <summary>
    /// Behandelt die Stein-Synchronisation vom Server.
    /// </summary>
    /// <param name="reader">Der NetPacketReader mit den Stein-Daten vom Server</param>
    public static void HandleRockSync(NetPacketReader reader)
    {
        int rockCount = reader.GetInt();
        var gameScreen = GameStateManager.Instance.GameScreen;
        
        if (gameScreen?.playerBoard == null)
        {
            Console.WriteLine("[Client] Fehler: playerBoard ist null bei RockSync.");
            return;
        }
        
        Console.WriteLine($"[Client] Empfange {rockCount} Steine vom Server...");
        
        for (int i = 0; i < rockCount; i++)
        {
            int x = reader.GetInt();
            int y = reader.GetInt();
            
            if (x >= 0 && x < gameScreen.playerBoard.GridWidth && 
                y >= 0 && y < gameScreen.playerBoard.GridHeight)
            {
                gameScreen.playerBoard.SetCellState(x, y, Gamescreen.CellState.Rock);
                Console.WriteLine($"[Client] Stein platziert bei ({x}, {y})");
            }
            else
            {
                Console.WriteLine($"[Client] Ungültige Stein-Position: ({x}, {y})");
            }
        }
        
        Console.WriteLine($"[Client] {rockCount} Steine erfolgreich synchronisiert");
    }
    
    /// <summary>
    /// Behandelt die Stein-Synchronisation für das Gegner-Board.
    /// </summary>
    /// <param name="reader">Der NetPacketReader mit den Stein-Daten vom Server</param>
    public static void HandleOpponentRockSync(NetPacketReader reader)
    {
        int rockCount = reader.GetInt();
        var gameScreen = GameStateManager.Instance.GameScreen;
        
        if (gameScreen?.opponentBoard == null)
        {
            Console.WriteLine("[Client] Fehler: opponentBoard ist null bei OpponentRockSync.");
            return;
        }
        
        Console.WriteLine($"[Client] Empfange {rockCount} Steine für Gegner-Board vom Server...");
        
        for (int i = 0; i < rockCount; i++)
        {
            int x = reader.GetInt();
            int y = reader.GetInt();
            
            if (x >= 0 && x < gameScreen.opponentBoard.GridWidth && 
                y >= 0 && y < gameScreen.opponentBoard.GridHeight)
            {
                gameScreen.opponentBoard.SetCellState(x, y, Gamescreen.CellState.Rock);
                Console.WriteLine($"[Client] Stein für Gegner platziert bei ({x}, {y})");
            }
            else
            {
                Console.WriteLine($"[Client] Ungültige Stein-Position für Gegner: ({x}, {y})");
            }
        }
        
        Console.WriteLine($"[Client] {rockCount} Steine für Gegner erfolgreich synchronisiert");
    }
} 