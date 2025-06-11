namespace WaterWizard.Server;

/// <summary>
/// Represents a ship that has been placed on the game board.
/// </summary>
public class PlacedShip
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public int MaxHealth => Width * Height;
    public HashSet<(int X, int Y)> DamagedCells { get; set; } = new();
    public bool IsDestroyed => DamagedCells.Count >= MaxHealth;

    public bool IsCellDamaged(int cellX, int cellY) => DamagedCells.Contains((cellX, cellY));

    public bool DamageCell(int cellX, int cellY)
    {
        return DamagedCells.Add((cellX, cellY));
    }
}
