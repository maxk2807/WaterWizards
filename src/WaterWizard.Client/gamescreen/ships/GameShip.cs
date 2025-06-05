using Raylib_cs;

namespace WaterWizard.Client.gamescreen.ships;

public class GameShip(GameScreen gameScreen, int x, int y, ShipType type, int width, int height)
{
    private readonly GameScreen gameScreen = gameScreen;
    private readonly ShipType type = type;

    public int X = x;
    public int Y = y;
    public int Width = width;
    public int Height = height;
    
    public HashSet<(int X, int Y)> DamagedCells { get; private set; } = new();
    public bool IsDestroyed => DamagedCells.Count >= (Width * Height / (gameScreen.playerBoard!.CellSize * gameScreen.playerBoard.CellSize));

    public void AddDamage(int cellX, int cellY)
    {
        DamagedCells.Add((cellX, cellY));
    }

    /// <summary>
    /// Draws the ship on the game screen.
    /// </summary>
    public void Draw()
    {
        Rectangle rec = new(X, Y, Width, Height);

        Color shipColor = DamagedCells.Count > 0 ? Color.Maroon : Color.DarkPurple;
        if (IsDestroyed)
            shipColor = Color.Black;

        Raylib.DrawRectangleRec(rec, shipColor);

        int cellSize = gameScreen.playerBoard!.CellSize;
        foreach (var (damageX, damageY) in DamagedCells)
        {
            int pixelX = X + (damageX * cellSize);
            int pixelY = Y + (damageY * cellSize);

            Raylib.DrawCircle(
                pixelX + cellSize / 2,
                pixelY + cellSize / 2,
                cellSize / 4f,
                Color.Red
            );
        }
    }
}
