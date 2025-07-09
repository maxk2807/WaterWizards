// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 141 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Shared;

namespace WaterWizard.Server.handler;

/// <summary>
/// Handler-Klasse für die Verwaltung von Steinen auf dem Spielfeld.
/// Behandelt die Generierung, Synchronisation und Validierung von Steinen.
/// </summary>
public static class RockHandler
{
    /// <summary>
    /// Generiert Steine für alle Spieler-Boards und synchronisiert sie mit den Clients.
    /// </summary>
    /// <param name="gameState">Der aktuelle Spielzustand</param>
    public static void GenerateAndSyncRocks(GameState gameState)
    {
        Console.WriteLine("[RockHandler] Starte Stein-Generierung für alle Spieler...");
        
        for (int playerIndex = 0; playerIndex < gameState.boards.Length; playerIndex++)
        {
            if (gameState.players[playerIndex] != null)
            {
                var rockPositions = RockFactory.GenerateRocks(gameState.boards[playerIndex]);
                SyncRocksToClient(gameState.players[playerIndex], rockPositions);
                
                Console.WriteLine($"[RockHandler] {rockPositions.Count} Steine für Spieler {playerIndex + 1} generiert und synchronisiert");
            }
        }
    }
    
    /// <summary>
    /// Synchronisiert die Stein-Positionen mit einem Client.
    /// </summary>
    /// <param name="peer">Der Client-Peer</param>
    /// <param name="rockPositions">Liste der Stein-Positionen</param>
    public static void SyncRocksToClient(NetPeer peer, List<(int X, int Y)> rockPositions)
    {
        var writer = new NetDataWriter();
        writer.Put("RockSync");
        writer.Put(rockPositions.Count);
        
        foreach (var (x, y) in rockPositions)
        {
            writer.Put(x);
            writer.Put(y);
        }
        
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        Console.WriteLine($"[RockHandler] Stein-Synchronisation an {peer} gesendet: {rockPositions.Count} Steine");
    }
    
    /// <summary>
    /// Überprüft, ob eine Position von einem Stein blockiert ist.
    /// </summary>
    /// <param name="board">Das Spielfeld</param>
    /// <param name="x">X-Koordinate</param>
    /// <param name="y">Y-Koordinate</param>
    /// <returns>True, wenn die Position von einem Stein blockiert ist</returns>
    public static bool IsPositionBlockedByRock(Cell[,] board, int x, int y)
    {
        if (x < 0 || x >= board.GetLength(0) || y < 0 || y >= board.GetLength(1))
        {
            return true; // Außerhalb des Spielfelds ist blockiert
        }
        
        return board[x, y].CellState == CellState.Rock;
    }
    
    /// <summary>
    /// Überprüft, ob ein Bereich von einem Stein blockiert ist.
    /// </summary>
    /// <param name="board">Das Spielfeld</param>
    /// <param name="startX">Start-X-Koordinate</param>
    /// <param name="startY">Start-Y-Koordinate</param>
    /// <param name="width">Breite des Bereichs</param>
    /// <param name="height">Höhe des Bereichs</param>
    /// <returns>True, wenn der Bereich von einem Stein blockiert ist</returns>
    public static bool IsAreaBlockedByRocks(Cell[,] board, int startX, int startY, int width, int height)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (IsPositionBlockedByRock(board, x, y))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Entfernt alle Steine von einem Spielfeld (für Reset-Zwecke).
    /// </summary>
    /// <param name="board">Das Spielfeld</param>
    public static void ClearRocks(Cell[,] board)
    {
        int clearedCount = 0;
        
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y].CellState == CellState.Rock)
                {
                    board[x, y].CellState = CellState.Empty;
                    clearedCount++;
                }
            }
        }
        
        Console.WriteLine($"[RockHandler] {clearedCount} Steine vom Spielfeld entfernt");
    }
    
    /// <summary>
    /// Gibt alle Stein-Positionen auf einem Spielfeld zurück.
    /// </summary>
    /// <param name="board">Das Spielfeld</param>
    /// <returns>Liste der Stein-Positionen</returns>
    public static List<(int X, int Y)> GetRockPositions(Cell[,] board)
    {
        var rockPositions = new List<(int X, int Y)>();
        
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y].CellState == CellState.Rock)
                {
                    rockPositions.Add((x, y));
                }
            }
        }
        
        return rockPositions;
    }
} 