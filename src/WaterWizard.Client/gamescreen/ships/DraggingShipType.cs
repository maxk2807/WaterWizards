using System.Numerics;
using Raylib_cs;

namespace WaterWizard.Client.gamescreen;

/// <summary>
/// This class represents a type of Ship on the <see cref="ShipField"/> that can be dragged 
/// onto the Board to place. Once placed, the dragged <see cref="DraggingShipType"/> spawns 
/// a normal Ship on that location.
/// In the beginning, there are 1 Ship of length 5, 2 Ships of length 4, 2 Ships of length 3,
/// 4 ships of length 2 and 5 ships of length 5. Further Ships can be summoned with the cards.
/// The number of ships is indicated by the number written in the middle
/// </summary>
/// <param name="gameScreen"></param>
/// <param name="x"></param>
/// <param name="y"></param>
/// <param name="length"></param>
/// <param name="currentNumber"></param>
/// <param name="orientation"></param>
public class DraggingShipType(GameScreen gameScreen, int x, int y, int width, int height, int currentNumber)
{
    private readonly int X = x;
    private readonly int Y = y;
    private readonly int Width = width;
    private readonly int Height = height;

    private bool dragging = false;
    private Vector2 offset = new();
    private bool firstDown = true;
    private bool firstUp = true;

    public void Draw()
    {
        Rectangle rec = new(X, Y, Width, Height);
        Raylib.DrawRectangleRec(rec, Color.DarkPurple);
        Raylib.DrawText(currentNumber.ToString(), X + Width / 2, Y + Height / 2, 10, Color.White);

        HandleDragging();
    }

    private void HandleDragging()
    {
        Rectangle rec = new(X, Y, Width, Height);
        bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rec);
        if (!dragging)
        {
            bool clicking = Raylib.IsMouseButtonDown(MouseButton.Left);
            if (clicking && firstDown)
            {
                if (hovering)
                {
                    dragging = true;
                    offset = new(Raylib.GetMousePosition().X - rec.X, Raylib.GetMousePosition().Y - rec.Y);
                }
                firstDown = false;
            }else if(!clicking && !firstDown){
                firstDown = true;
            }
        }
        else
        {
            bool released = Raylib.IsMouseButtonUp(MouseButton.Left);
            if (released)
            {
                OnRelease(offset, rec);
                dragging = false;
                firstDown = true;
            }
            else
            {
                DrawDrag(offset);
            }
        }
    }

    private void OnRelease(Vector2 offset, Rectangle rec)
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        GameBoard.Point? cell = gameScreen.playerBoard!.GetCellFromScreenCoords(new(mousePos.X - offset.X, mousePos.Y - offset.Y));
    }


    private void DrawDrag(Vector2 offset)
    {
        var pos = Raylib.GetMousePosition();
        Rectangle rec2 = new(pos.X - offset.X, pos.Y - offset.Y, Width, Height);
        Raylib.DrawRectangleRec(rec2, Color.DarkPurple);
    }
}