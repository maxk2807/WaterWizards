using System.Drawing;
using WaterWizard.Shared;

namespace WaterWizard.Server.handler;

/// <summary>
/// Factory-Klasse für die intelligente Generierung von Steinen auf dem Spielfeld.
/// Generiert zufällig 6 Steine, die strategisch platziert werden, um das Spiel interessanter zu machen.
/// </summary>
public static class RockFactory
{
    private static readonly Random _random = new();
    
    /// <summary>
    /// Generiert 6 zufällige Steine auf dem Spielfeld.
    /// </summary>
    /// <param name="board">Das Spielfeld, auf dem die Steine platziert werden sollen</param>
    /// <param name="rockCount">Anzahl der zu generierenden Steine (Standard: 6)</param>
    /// <returns>Liste der generierten Stein-Positionen</returns>
    public static List<(int X, int Y)> GenerateRocks(Cell[,] board, int rockCount = 6)
    {
        var rockPositions = new List<(int X, int Y)>();
        var availablePositions = GetAvailablePositions(board);
        
        // Strategische Zonen definieren für interessantere Platzierung
        var strategicZones = DefineStrategicZones(board.GetLength(0), board.GetLength(1));
        
        // Steine in strategischen Zonen platzieren
        int strategicRocks = Math.Min(rockCount / 2, 3); // Mindestens 3 Steine in strategischen Zonen
        PlaceStrategicRocks(board, rockPositions, availablePositions, strategicZones, strategicRocks);
        
        // Restliche Steine zufällig platzieren
        int remainingRocks = rockCount - rockPositions.Count;
        PlaceRandomRocks(board, rockPositions, availablePositions, remainingRocks);
        
        Console.WriteLine($"[RockFactory] {rockPositions.Count} Steine erfolgreich generiert");
        return rockPositions;
    }
    
    /// <summary>
    /// Definiert strategische Zonen für die Steinplatzierung.
    /// </summary>
    private static List<Rectangle> DefineStrategicZones(int boardWidth, int boardHeight)
    {
        var zones = new List<Rectangle>();
        
        // Zentrale Zone (verhindert einfache diagonale Schüsse)
        int centerX = boardWidth / 2;
        int centerY = boardHeight / 2;
        zones.Add(new Rectangle(centerX - 1, centerY - 1, 3, 3));
        
        // Ecken-Zonen (verhindert einfache Eckenschüsse)
        zones.Add(new Rectangle(0, 0, 2, 2)); // Obere linke Ecke
        zones.Add(new Rectangle(boardWidth - 2, 0, 2, 2)); // Obere rechte Ecke
        zones.Add(new Rectangle(0, boardHeight - 2, 2, 2)); // Untere linke Ecke
        zones.Add(new Rectangle(boardWidth - 2, boardHeight - 2, 2, 2)); // Untere rechte Ecke
        
        // Mittlere Randzonen (verhindert einfache Randschüsse)
        zones.Add(new Rectangle(0, centerY - 1, 2, 3)); // Linker Rand
        zones.Add(new Rectangle(boardWidth - 2, centerY - 1, 2, 3)); // Rechter Rand
        zones.Add(new Rectangle(centerX - 1, 0, 3, 2)); // Oberer Rand
        zones.Add(new Rectangle(centerX - 1, boardHeight - 2, 3, 2)); // Unterer Rand
        
        return zones;
    }
    
    /// <summary>
    /// Platziert Steine in strategischen Zonen.
    /// </summary>
    private static void PlaceStrategicRocks(
        Cell[,] board, 
        List<(int X, int Y)> rockPositions, 
        HashSet<(int X, int Y)> availablePositions,
        List<Rectangle> strategicZones,
        int count)
    {
        var shuffledZones = strategicZones.OrderBy(x => _random.Next()).ToList();
        
        foreach (var zone in shuffledZones)
        {
            if (rockPositions.Count >= count) break;
            
            var zonePositions = GetPositionsInZone(zone, availablePositions);
            if (zonePositions.Count > 0)
            {
                var selectedPosition = zonePositions[_random.Next(zonePositions.Count)];
                PlaceRock(board, rockPositions, availablePositions, selectedPosition);
            }
        }
    }
    
    /// <summary>
    /// Platziert zufällige Steine auf verfügbaren Positionen.
    /// </summary>
    private static void PlaceRandomRocks(
        Cell[,] board,
        List<(int X, int Y)> rockPositions,
        HashSet<(int X, int Y)> availablePositions,
        int count)
    {
        var availableList = availablePositions.ToList();
        
        for (int i = 0; i < count && availableList.Count > 0; i++)
        {
            int randomIndex = _random.Next(availableList.Count);
            var position = availableList[randomIndex];
            
            PlaceRock(board, rockPositions, availablePositions, position);
            availableList.RemoveAt(randomIndex);
        }
    }
    
    /// <summary>
    /// Platziert einen einzelnen Stein an der angegebenen Position.
    /// </summary>
    private static void PlaceRock(
        Cell[,] board,
        List<(int X, int Y)> rockPositions,
        HashSet<(int X, int Y)> availablePositions,
        (int X, int Y) position)
    {
        if (availablePositions.Contains(position) && IsValidRockPlacement(board, position.X, position.Y, rockPositions))
        {
            board[position.X, position.Y].CellState = CellState.Rock;
            rockPositions.Add(position);
            availablePositions.Remove(position);
            
            Console.WriteLine($"[RockFactory] Stein platziert bei ({position.X}, {position.Y})");
        }
    }
    
    /// <summary>
    /// Ermittelt alle verfügbaren Positionen auf dem Spielfeld.
    /// </summary>
    private static HashSet<(int X, int Y)> GetAvailablePositions(Cell[,] board)
    {
        var positions = new HashSet<(int X, int Y)>();
        
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y].CellState == CellState.Empty)
                {
                    positions.Add((x, y));
                }
            }
        }
        
        return positions;
    }
    
    /// <summary>
    /// Ermittelt alle Positionen innerhalb einer Zone, die verfügbar sind.
    /// </summary>
    private static List<(int X, int Y)> GetPositionsInZone(Rectangle zone, HashSet<(int X, int Y)> availablePositions)
    {
        var zonePositions = new List<(int X, int Y)>();
        
        for (int x = zone.X; x < zone.X + zone.Width; x++)
        {
            for (int y = zone.Y; y < zone.Y + zone.Height; y++)
            {
                if (availablePositions.Contains((x, y)))
                {
                    zonePositions.Add((x, y));
                }
            }
        }
        
        return zonePositions;
    }
    
    /// <summary>
    /// Validiert, ob eine Steinplatzierung gültig ist (nicht zu nah an anderen Steinen).
    /// </summary>
    private static bool IsValidRockPlacement(Cell[,] board, int x, int y, List<(int X, int Y)> existingRocks)
    {
        // Mindestabstand zu anderen Steinen (optional für zukünftige Erweiterungen)
        const int minDistance = 1;
        
        foreach (var rock in existingRocks)
        {
            int distance = Math.Max(Math.Abs(x - rock.X), Math.Abs(y - rock.Y));
            if (distance < minDistance)
            {
                return false;
            }
        }
        
        return true;
    }
} 