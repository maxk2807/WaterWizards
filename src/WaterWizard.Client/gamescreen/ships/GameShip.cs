// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 43 Zeilen
// - Erickk0: 20 Zeilen
// - erick: 15 Zeilen
// - jdewi001: 2 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.gamescreen.handler;

namespace WaterWizard.Client.gamescreen.ships;

public class GameShip(GameScreen gameScreen, int x, int y, ShipType type, int width, int height)
{
    private readonly GameScreen gameScreen = gameScreen;
    private readonly ShipType type = type;

    public int X = x;
    public int Y = y;
    public int Width = width;
    public int Height = height;

    public bool Rotated => Math.Max(Width / CellSize, Height / CellSize) == Width / CellSize;

    private int CellSize => gameScreen.playerBoard!.CellSize;

    public HashSet<(int X, int Y)> DamagedCells { get; private set; } = new();
    public bool IsDestroyed =>
        DamagedCells.Count
        >= (Width * Height / (CellSize * CellSize));

    public bool IsRevealed { get; set; } = false;
    public float Transparency { get; set; } = 1.0f;

    public void AddDamage(int cellX, int cellY)
    {
        DamagedCells.Add((cellX, cellY));
    }

    /// <summary>
    /// Draws the ship on the game screen.
    /// </summary>
    public void Draw()
    {
        Texture2D texture = HandleShips.TextureFromLength(Rotated, Math.Max(Width / CellSize, Height / CellSize));
        Rectangle rec = new(X, Y, Width, Height);
        Rectangle textureRec = new(0, 0, texture.Width, texture.Height);
        Raylib.DrawTexturePro(
            texture,
            textureRec,
            rec,
            Vector2.Zero,
            0f,
            Color.White
        );


        Color shipColor = DamagedCells.Count > 0 ? new(190, 33, 55, 0.3f) : new Color(112, 31, 126, 0.3f);
        if (IsDestroyed)
            shipColor = new(255, 255, 255, 0.3f);

        if (IsRevealed && Transparency < 1.0f)
        {
            shipColor = new Color(shipColor.R, shipColor.G, shipColor.B, 0.3f);
        }

        // Raylib.DrawRectangleRec(rec, shipColor);


        if (!IsRevealed || Transparency >= 1.0f)
        {
            foreach (var (damageX, damageY) in DamagedCells)
            {
                int pixelX = X + (damageX * CellSize);
                int pixelY = Y + (damageY * CellSize);

                Raylib.DrawCircle(
                    pixelX + CellSize / 2,
                    pixelY + CellSize / 2,
                    CellSize / 4f,
                    Color.Red
                );
            }
        }
    }
}
