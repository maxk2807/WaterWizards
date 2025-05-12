
using Raylib_cs;

namespace WaterWizard.Client.gamescreen;

public class GameShip(GameScreen gameScreen, int x, int y, ShipType type, int width, int height)
{
    internal void Draw()
    {
        Rectangle rec = new(x,y,width,height); 
        Raylib.DrawRectangleRec(rec, Color.DarkPurple);
    }
}