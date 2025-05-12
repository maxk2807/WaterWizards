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

    private Rectangle Rectangle => new(X, Y, Width, Height);
    private Rectangle DraggedRectangle = new(x,y,width,height);
    private bool dragging = false;
    private Vector2 offset = new();
    private bool firstDown = true;
    private bool validPlacement = false;
    private bool confirming = false;
    private int CellSize => gameScreen.playerBoard!.CellSize;
    private int ZonePadding => (int)gameScreen.ZonePadding;

    public void Draw()
    {
        Rectangle rec = new(X, Y, Width, Height);
        Raylib.DrawRectangleRec(rec, Color.DarkPurple);
        Raylib.DrawText(currentNumber.ToString(), X + Width / 2, Y + Height / 2, 10, Color.White);

        HandleDragging();
    }

    private void HandleDragging()
    {
        bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), Rectangle);
        if(confirming){
            HandleConfirm();
            return;
        }
        if (!dragging)
        {
            bool clicking = Raylib.IsMouseButtonDown(MouseButton.Left);
            if (clicking && firstDown)
            {
                if (hovering)
                {
                    dragging = true;
                    offset = new(Raylib.GetMousePosition().X - Rectangle.X, Raylib.GetMousePosition().Y - Rectangle.Y);
                }
                firstDown = false;
            }
            else if (!clicking && !firstDown)
            {
                firstDown = true;
            }
        }
        else
        {
            bool released = Raylib.IsMouseButtonUp(MouseButton.Left);
            if (released)
            {
                if(validPlacement){
                    confirming = true;
                }
                dragging = false;
                firstDown = true;
            }
            else
            {
                validPlacement = DrawDrag(offset);
            }
        }
    }

    private void HandleConfirm()
    {
        //TODO: Create buttons for confirmation and rotation of ship
        //TODO: Reduce number of current ships

        SpawnShip((int)DraggedRectangle.X, (int)DraggedRectangle.Y, Math.Max(Width, Height));
        confirming = false;
    }

    private void SpawnShip(int x, int y, int shipSize)
    {
        gameScreen.playerBoard!.putShip(new(gameScreen, x, y, ShipType.DEFAULT, Width, Height));
    }

    /// <summary>
    /// Handles the Ship while it is being dragged across the Screen.
    /// <list type="bullet">
    /// <item>Drawing the dragged ship over the screen and board</item>
    /// <item>Snapping the Ship to the Cell it is currently over while dragging
    /// over the player board</item>
    /// <item>Checks whether the Ship is in a valid position for placement</item>
    /// </summary>
    /// <param name="offset"></param>
    /// <returns>true if in valid position for placement, false if not</returns>
    private bool DrawDrag(Vector2 offset)
    {
        var mousePos = Raylib.GetMousePosition();
        var shipPos = SnapToNearestCell(mousePos, offset);
        if (shipPos.HasValue)
        {
            GameBoard board = gameScreen.playerBoard!;
            float snappedX = board.Position.X + shipPos.Value.X * CellSize;
            float snappedY = board.Position.Y + shipPos.Value.Y * CellSize;
            DraggedRectangle = new(snappedX, snappedY, Width, Height);
            bool valid = IsValid(DraggedRectangle);
            Raylib.DrawRectangleRec(DraggedRectangle, valid ? new(30, 200, 200) : new(255, 0, 0));
            return valid;
        }
        else
        {
            DraggedRectangle = new(mousePos.X - offset.X, mousePos.Y - offset.Y, Width, Height);
            Raylib.DrawRectangleRec(DraggedRectangle, new(0, 0, 0, 0.5f));
            return false;
        }
    }

    private bool IsValid(Rectangle rec)
    {
        GameBoard board = gameScreen.playerBoard!;
        bool isOutOfBounds = rec.X < board.Position.X ||rec.Y < board.Position.Y ||
                   rec.X + rec.Width > board.Position.X + (float)board.GridWidth * CellSize ||
                   rec.Y + rec.Height > board.Position.Y + (float)board.GridHeight * CellSize;
        return !isOutOfBounds;
        //TODO: Check for other ships and rocks
    }

    private Vector2? SnapToNearestCell(Vector2 mousePos, Vector2 offset)
    {
        Vector2 shipPos = new(mousePos.X - offset.X, mousePos.Y - offset.Y);
        var point = gameScreen.playerBoard!.GetCellFromScreenCoords(shipPos);
        if (point.HasValue)
        {
            return new(point.Value.X, point.Value.Y);
        }
        else return null;
    }
}