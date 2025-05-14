using System.Numerics;
using Raylib_cs;

namespace WaterWizard.Client.gamescreen.ships;

public class DraggingShip
{
    private readonly GameScreen gameScreen;
    private readonly int X;
    private readonly int Y;
    private readonly int Width;
    private readonly int Height;
    private readonly int currentNumber;

    public Texture2D rotateIcon;
    public Texture2D confirmIcon;

    /// <summary>
    /// <see cref="Raylib_cs.Rectangle"/> of the Original Ship on the <see cref="ShipField"/>.
    /// </summary>
    private Rectangle Rectangle => new(X, Y, Width, Height);
    /// <summary>
    /// <see cref="Raylib_cs.Rectangle"/> of the Ship that is being dragged onto the Screen to place
    /// a <see cref="GameShip"/>.
    /// </summary>
    private Rectangle DraggedShipRectangle;
    private bool dragging = false;
    private Vector2 offset = new();
    private bool firstDown = true;
    private bool validPlacement = false;
    /// <summary>
    /// Whether there is currently a confirmation process 
    /// (i.e. whether a ship was moved onto the board and //TODO: can now be rotated)
    /// </summary>
    private bool confirming = false;
    /// <summary>
    /// Whether the confirmation button was clicked
    /// </summary>
    private int CellSize => gameScreen.playerBoard!.CellSize;

    /// <summary>
    /// This class represents a type of Ship on the <see cref="ShipField"/> that can be dragged 
    /// onto the Board to place. Once placed, the dragged <see cref="DraggingShip"/> spawns 
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
    public DraggingShip(GameScreen gameScreen, int x, int y, int width, int height, int currentNumber)
    {
        this.gameScreen = gameScreen;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        this.currentNumber = currentNumber;
        DraggedShipRectangle = new(x, y, width, height);
        rotateIcon = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/icons8-rotate-48.png");
        confirmIcon = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/icons8-tick-60.png");
    }

    /// <summary>
    /// Handles Rendering of the Ships that can be Dragged and placed on the board. Handles dragging and placement
    /// </summary>
    public void Draw()
    {
        Rectangle rec = new(X, Y, Width, Height);
        Raylib.DrawRectangleRec(rec, Color.DarkPurple);
        Raylib.DrawText(currentNumber.ToString(), X + Width / 2, Y + Height / 2, 10, Color.White);

        HandleDragging();
    }

    /// <summary>
    /// Handles the dragging of the Ships.
    /// <para/>
    /// Checks if the type of ship you want to drag is being hovered, checks Mouse Buttons
    /// if you are currently "dragging" (holding left mouse button over a ship)
    /// <para/>
    /// Checks if the placement of the ship is valid, i.e. not obstructed or out of bounds
    /// <para/>
    /// Handles the placement of the ship //TODO: with the Confirm Mechanic
    /// </summary>
    private void HandleDragging()
    {
        bool hovering = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), DraggedShipRectangle);
        if (confirming)
        {
            HandleConfirm();
        }
        if (!dragging)
        {
            bool clicking = Raylib.IsMouseButtonDown(MouseButton.Left);
            if (clicking && firstDown)
            {
                if (hovering)
                {
                    dragging = true;
                    offset = new(Raylib.GetMousePosition().X - DraggedShipRectangle.X, Raylib.GetMousePosition().Y - DraggedShipRectangle.Y);
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
                if (validPlacement)
                {
                    confirming = true;
                }
                dragging = false;
                firstDown = true;
            }
            else
            {
                validPlacement = DrawDrag(offset);
                if (!validPlacement)
                {
                    confirming = false;
                    DraggedShipRectangle.X = Rectangle.X;
                    DraggedShipRectangle.Y = Rectangle.Y;
                }
            }
        }
    }

    /// <summary>
    /// Handles the Confirm Mechanic of Ship Placement. 
    /// //TODO: Renders the Ship Outline and offers
    /// Buttons to rotate the ship as well as a Confirm button.
    /// Spawns the <see cref="GameShip"/> upon Confirming 
    /// </summary>
    private void HandleConfirm()
    {
        //TODO: Create buttons for confirmation and rotation of ship
        //TODO: Reduce number of current ships
        Raylib.DrawRectangleRec(DraggedShipRectangle, new(30, 200, 200));

        var confirmX = DraggedShipRectangle.X + DraggedShipRectangle.Width / 2;
        var confirmY = DraggedShipRectangle.Y + DraggedShipRectangle.Height;
        Rectangle confirmButton = new(confirmX, confirmY, CellSize, CellSize);
        bool confirmHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), confirmButton);
        Raylib.DrawRectangleRec(confirmButton, confirmHovered ? Color.LightGray : Color.Gray);

        float confirIconScale = CellSize * 0.8f / confirmIcon.Height;
        float confirmIconSize = confirmIcon.Height * confirIconScale;
        int confirmIconX = (int)(confirmX + (CellSize - confirmIconSize)/2f);
        int confirmIconY = (int)(confirmY + (CellSize - confirmIconSize)/2f);
        Raylib.DrawTextureEx(confirmIcon, new(confirmIconX, confirmIconY), 0, confirIconScale, Color.White);

        if (confirmHovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            SpawnShip((int)DraggedShipRectangle.X, (int)DraggedShipRectangle.Y, Math.Max(Width, Height));
            confirming = false;
            DraggedShipRectangle = new(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
        }

        var rotateX = DraggedShipRectangle.X + DraggedShipRectangle.Width / 2 - CellSize;
        var rotateY = DraggedShipRectangle.Y + DraggedShipRectangle.Height;
        Rectangle rotateButton = new(rotateX, rotateY, CellSize, CellSize);
        bool rotateHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rotateButton);
        Raylib.DrawRectangleRec(rotateButton, rotateHovered ? Color.LightGray : Color.Gray);

        float rotateIconScale = CellSize * 0.7f / rotateIcon.Height;
        float rotateIconSize = rotateIcon.Height * rotateIconScale;
        int rotateIconX = (int)(rotateX + (CellSize - rotateIconSize)/2f);
        int rotateIconY = (int)(rotateY + (CellSize - rotateIconSize)/2f);
        Raylib.DrawTextureEx(rotateIcon, new(rotateIconX, rotateIconY), 0, rotateIconScale, Color.White);

        if (rotateHovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            var prevWidth = DraggedShipRectangle.Width;
            var prevHeight = DraggedShipRectangle.Height;
            DraggedShipRectangle.Width = prevHeight;
            DraggedShipRectangle.Height = prevWidth;
        }
    }

    /// <summary>
    /// Spawn the GameShip at the given point on the board
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="shipSize"></param>
    private void SpawnShip(int x, int y, int shipSize)
    {
        gameScreen.playerBoard!.putShip(new(gameScreen, x, y, ShipType.DEFAULT, (int)DraggedShipRectangle.Width, (int)DraggedShipRectangle.Height));
    }

    /// <summary>
    /// Handles the Ship while it is being dragged across the Screen.
    /// <para />
    /// ->Drawing the dragged ship over the screen and board
    /// <para />
    /// ->Snapping the Ship to the Cell it is currently over while dragging
    /// over the player board
    /// <para />
    /// ->Checks whether the Ship is in a valid position for placement
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
            DraggedShipRectangle = new(snappedX, snappedY, DraggedShipRectangle.Width, DraggedShipRectangle.Height);
            bool valid = IsValid(DraggedShipRectangle);
            Raylib.DrawRectangleRec(DraggedShipRectangle, valid ? new(30, 200, 200) : new(255, 0, 0));
            return valid;
        }
        else
        {
            DraggedShipRectangle = new(mousePos.X - offset.X, mousePos.Y - offset.Y, DraggedShipRectangle.Width, DraggedShipRectangle.Height);
            Raylib.DrawRectangleRec(DraggedShipRectangle, new(200, 0, 0, 0.5f));
            return false;
        }
    }

    /// <summary>
    /// Checks whether the Placement of ship is obstructed by being out of bounds 
    /// of the board or //TODO: obstructed by other ships and Rocks  
    /// </summary>
    /// <param name="rec"></param>
    /// <returns>Whether the placement is valid or not</returns>
    private bool IsValid(Rectangle rec)
    {
        GameBoard board = gameScreen.playerBoard!;
        bool isOutOfBounds = rec.X < board.Position.X || rec.Y < board.Position.Y ||
                   rec.X + rec.Width > board.Position.X + (float)board.GridWidth * CellSize ||
                   rec.Y + rec.Height > board.Position.Y + (float)board.GridHeight * CellSize;
        return !isOutOfBounds;
        //TODO: Check for other ships and rocks
    }

    /// <summary>
    /// Snaps Ship to the Cell that the ships X and Y Coords currently hover over.
    /// Can only Snap while inside the Board.
    /// //TODO: Snap to the nearest Cell 
    /// </summary>
    /// <param name="mousePos"></param>
    /// <param name="offset"></param>
    /// <returns>Optional <see cref="Vector2"/> of the snapped Position of the Ship </returns>
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