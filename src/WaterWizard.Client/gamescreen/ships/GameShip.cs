
using Raylib_cs;

namespace WaterWizard.Client.gamescreen.ships;

public class GameShip(GameScreen gameScreen, int x, int y, ShipType type, int width, int height)
{
    private readonly GameScreen gameScreen = gameScreen;
    private readonly ShipType type = type;

    public void Draw()
    {
        Rectangle rec = new(x,y,width,height); 
        Raylib.DrawRectangleRec(rec, Color.DarkPurple);
    }
}