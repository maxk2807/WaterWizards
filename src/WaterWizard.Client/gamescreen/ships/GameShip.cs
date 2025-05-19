
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

    public void Draw()
    {
        Rectangle rec = new(X,Y,Width,Height); 
        Raylib.DrawRectangleRec(rec, Color.DarkPurple);
    }
}