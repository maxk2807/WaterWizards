// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 259 Zeilen
// - jdewi001: 151 Zeilen
// - justinjd00: 47 Zeilen
// - erick: 13 Zeilen
// - Erickk0: 8 Zeilen
// - Erick Zeiler: 2 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - private Rectangle Rectangle => new(X, Y, Width, Height);   (maxk2807: 7 Zeilen)
// - private Vector2 offset = new();   (maxk2807: 231 Zeilen)
// ===============================================

using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.gamestates;
using WaterWizard.Client.network;
using WaterWizard.Client.Gamescreen;

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

    private static readonly Dictionary<int, int> PlacementLimits = new()
    {
        { 5, 1 },
        { 4, 2 },
        { 3, 2 },
        { 2, 4 },
        { 1, 5 },
    };

    private bool IsShipSizeLimitReached(int size)
    {
        // Prüfe, ob die Platzierungsphase aktiv ist
        if (GameStateManager.Instance.GetCurrentState() is InGameState)
            return false;
        if (GameStateManager.Instance.GetCurrentState() is not PlacementPhaseState)
            return false;
        // Prüfe, ob das Limit serverseitig gemeldet wurde
        if (gameScreen.IsShipSizeLimitReached(size))
            return true;
        int placed = gameScreen.playerBoard!.Ships.Count(s => Math.Max(s.Width, s.Height) == size);
        return PlacementLimits.TryGetValue(size, out int limit) && placed >= limit;
    }

    private bool IsShipPlacementAllowed()
    {
        bool allowed = false;
        // Während der Platzierungsphase immer erlaubt
        if (GameStateManager.Instance.GetCurrentState() is PlacementPhaseState)
            allowed = true;
        // Im InGameState nur, wenn das Flag gesetzt ist
        var field = gameScreen.GetType().GetField("allowSingleShipPlacement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (GameStateManager.Instance.GetCurrentState() is InGameState && field != null && field.GetValue(gameScreen) is bool allowedFlag && allowedFlag)
            allowed = true;
        Console.WriteLine($"IsShipPlacementAllowed: {allowed}");
        return allowed;
    }

    /// <summary>
    /// This class represents a type of Ship on the <see cref="ShipField"/> that can be dragged
    /// onto the Board to place. Once placed, the dragged <see cref="DraggingShip"/> spawns
    /// a normal Ship on that location.
    /// In the beginning, there are 1 Ship of length 5, 2 Ships of length 4, 2 Ships of length 3,
    /// 4 ships of length 2 and 5 ships of length 1. Further Ships can be summoned with the cards.
    /// The number of ships is indicated by the number written in the middle
    /// </summary>
    /// <param name="gameScreen"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="length"></param>
    /// <param name="currentNumber"></param>
    /// <param name="orientation"></param>
    public DraggingShip(
        GameScreen gameScreen,
        int x,
        int y,
        int width,
        int height,
        int currentNumber
    )
    {
        this.gameScreen = gameScreen;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        this.currentNumber = currentNumber;
        DraggedShipRectangle = new(x, y, width, height);
        rotateIcon = TextureManager.LoadTexture(
            "src/WaterWizard.Client/Assets/icons8-rotate-48.png"
        );
        confirmIcon = TextureManager.LoadTexture(
            "src/WaterWizard.Client/Assets/icons8-tick-60.png"
        );
    }

    /// <summary>
    /// Handles Rendering of the Ships that can be Dragged and placed on the board. Handles dragging and placement
    /// </summary>
    public void Draw()
    {
        Console.WriteLine($"Draw() aufgerufen, dragging: {dragging}");
        Rectangle rec = new(X, Y, Width, Height);

        // Wenn das Limit erreicht ist, Schiff ausgegraut zeichnen
        int size = Math.Max(Width / CellSize, Height / CellSize);
        bool limitReached = IsShipSizeLimitReached(size);
        Color shipColor = limitReached ? Color.Gray : Color.DarkPurple;

        Raylib.DrawRectangleRec(rec, shipColor);
        Raylib.DrawText(currentNumber.ToString(), X + Width / 2, Y + Height / 2, 10, Color.White);

        // Nur Dragging erlauben, wenn das Limit nicht erreicht ist und Platzierung erlaubt ist
        if (!limitReached && IsShipPlacementAllowed())
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
    /// Handles the placement of the ship with the Confirm Mechanic
    /// </summary>
    private void HandleDragging()
    {
        bool hovering = Raylib.CheckCollisionPointRec(
            Raylib.GetMousePosition(),
            DraggedShipRectangle
        );

        // Zeichne das Originalschiff immer
        Raylib.DrawRectangleRec(Rectangle, Color.DarkPurple);
        Raylib.DrawText(currentNumber.ToString(), X + Width / 2, Y + Height / 2, 10, Color.White);

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
                    // Prüfe hier nochmal das Limit, falls sich der Zustand geändert hat
                    int size = Math.Max(Width / CellSize, Height / CellSize);
                    if (!IsShipSizeLimitReached(size))
                    {
                        dragging = true;
                        offset = new(
                            Raylib.GetMousePosition().X - DraggedShipRectangle.X,
                            Raylib.GetMousePosition().Y - DraggedShipRectangle.Y
                        );
                    }
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
                else
                {
                    confirming = false;
                    DraggedShipRectangle.X = Rectangle.X;
                    DraggedShipRectangle.Y = Rectangle.Y;
                    DraggedShipRectangle.Width = Rectangle.Width;
                    DraggedShipRectangle.Height = Rectangle.Height;
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
                }
            }
        }
    }

    /// <summary>
    /// Handles the Confirm Mechanic of Ship Placement.
    /// Renders the Ship Outline and offers
    /// Buttons to rotate the ship as well as a Confirm button.
    /// Spawns the <see cref="GameShip"/> upon Confirming
    /// </summary>
    private void HandleConfirm()
    {
        Raylib.DrawRectangleRec(
            DraggedShipRectangle,
            validPlacement ? new(30, 200, 200) : new(255, 0, 0)
        );

        float screenHeight = gameScreen._gameStateManager.screenHeight;
        float buttonAreaHeight = CellSize;

        var rotateX = DraggedShipRectangle.X + DraggedShipRectangle.Width / 2 - CellSize;
        var rotateY = DraggedShipRectangle.Y + DraggedShipRectangle.Height;
        var confirmX = DraggedShipRectangle.X + DraggedShipRectangle.Width / 2;
        var confirmY = DraggedShipRectangle.Y + DraggedShipRectangle.Height;

        if (confirmY + buttonAreaHeight > screenHeight)
        {
            rotateY = DraggedShipRectangle.Y - buttonAreaHeight;
            confirmY = DraggedShipRectangle.Y - buttonAreaHeight;
        }

        Rectangle rotateButton = new(rotateX, rotateY, CellSize, CellSize);
        bool rotateHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rotateButton);
        Raylib.DrawRectangleRec(rotateButton, rotateHovered ? Color.LightGray : Color.Gray);

        float rotateIconScale = CellSize * 0.7f / rotateIcon.Height;
        float rotateIconSize = rotateIcon.Height * rotateIconScale;
        int rotateIconX = (int)(rotateX + (CellSize - rotateIconSize) / 2f);
        int rotateIconY = (int)(rotateY + (CellSize - rotateIconSize) / 2f);
        Raylib.DrawTextureEx(
            rotateIcon,
            new(rotateIconX, rotateIconY),
            0,
            rotateIconScale,
            Color.White
        );

        if (rotateHovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            var prevWidth = DraggedShipRectangle.Width;
            var prevHeight = DraggedShipRectangle.Height;
            DraggedShipRectangle.Width = prevHeight;
            DraggedShipRectangle.Height = prevWidth;
            validPlacement = IsValid(DraggedShipRectangle);
        }

        Rectangle confirmButton = new(confirmX, confirmY, CellSize, CellSize);
        bool confirmHovered = Raylib.CheckCollisionPointRec(
            Raylib.GetMousePosition(),
            confirmButton
        );
        if (validPlacement)
        {
            Raylib.DrawRectangleRec(confirmButton, confirmHovered ? Color.LightGray : Color.Gray);

            float confirIconScale = CellSize * 0.8f / confirmIcon.Height;
            float confirmIconSize = confirmIcon.Height * confirIconScale;
            int confirmIconX = (int)(confirmX + (CellSize - confirmIconSize) / 2f);
            int confirmIconY = (int)(confirmY + (CellSize - confirmIconSize) / 2f);
            Raylib.DrawTextureEx(
                confirmIcon,
                new(confirmIconX, confirmIconY),
                0,
                confirIconScale,
                Color.White
            );
        }

        if (validPlacement && confirmHovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            SpawnShip(
                (int)DraggedShipRectangle.X,
                (int)DraggedShipRectangle.Y,
                Math.Max(Width, Height)
            );
            confirming = false;
            DraggedShipRectangle = new(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);

            // Nach Platzierung im InGameState das Flag zurücksetzen
            if (GameStateManager.Instance.GetCurrentState() is InGameState)
            {
                var prop = gameScreen.GetType().GetProperty("allowSingleShipPlacement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                    prop.SetValue(gameScreen, false);
            }
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
        if (gameScreen.playerBoard == null)
        {
            Console.WriteLine("Warning: playerBoard is null, cannot spawn ship");
            return;
        }
        //gameScreen.playerBoard!.putShip(new(gameScreen, x, y, ShipType.DEFAULT, (int)DraggedShipRectangle.Width, (int)DraggedShipRectangle.Height));
        int boardX = (int)gameScreen.playerBoard.Position.X;
        int boardY = (int)gameScreen.playerBoard.Position.Y;
        NetworkManager.Instance.SendShipPlacement(
            (x - boardX) / CellSize,
            (y - boardY) / CellSize,
            (int)(DraggedShipRectangle.Width / CellSize),
            (int)(DraggedShipRectangle.Height / CellSize)
        );
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
            DraggedShipRectangle = new(
                snappedX,
                snappedY,
                DraggedShipRectangle.Width,
                DraggedShipRectangle.Height
            );
            bool valid = IsValid(DraggedShipRectangle);
            Raylib.DrawRectangleRec(
                DraggedShipRectangle,
                valid ? new(30, 200, 200) : new(255, 0, 0)
            );
            return valid;
        }
        else
        {
            DraggedShipRectangle = new(
                mousePos.X - offset.X,
                mousePos.Y - offset.Y,
                DraggedShipRectangle.Width,
                DraggedShipRectangle.Height
            );
            Raylib.DrawRectangleRec(DraggedShipRectangle, new(200, 0, 0, 0.5f));
            return false;
        }
    }

    /// <summary>
    /// Checks whether the Placement of ship is obstructed by being out of bounds
    /// of the board or obstructed by other ships or rocks
    /// </summary>
    /// <param name="dragShip"></param>
    /// <returns>Whether the placement is valid or not</returns>
    private bool IsValid(Rectangle dragShip)
    {
        GameBoard board = gameScreen.playerBoard!;
        bool isOutOfBounds =
            dragShip.X < board.Position.X
            || dragShip.Y < board.Position.Y
            || dragShip.X + dragShip.Width > board.Position.X + (float)board.GridWidth * CellSize
            || dragShip.Y + dragShip.Height > board.Position.Y + (float)board.GridHeight * CellSize;

        if (isOutOfBounds)
            return false;

        // Prüfe Kollision mit anderen Schiffen
        bool collidesWithShips = false;
        foreach (var ship in gameScreen.playerBoard!.Ships)
        {
            Rectangle shipRec = new(ship.X, ship.Y, ship.Width, ship.Height);
            collidesWithShips = Raylib.CheckCollisionRecs(dragShip, shipRec);
            if (collidesWithShips)
                break;
        }

        if (collidesWithShips)
            return false;

        // Prüfe Kollision mit Steinen
        bool collidesWithRocks = false;
        int startX = (int)((dragShip.X - board.Position.X) / CellSize);
        int startY = (int)((dragShip.Y - board.Position.Y) / CellSize);
        int width = (int)(dragShip.Width / CellSize);
        int height = (int)(dragShip.Height / CellSize);

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x >= 0 && x < board.GridWidth && y >= 0 && y < board.GridHeight)
                {
                    if (board._gridStates[x, y] == CellState.Rock)
                    {
                        collidesWithRocks = true;
                        break;
                    }
                }
            }
            if (collidesWithRocks)
                break;
        }

        return !collidesWithRocks;
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
        else
            return null;
    }

    public void StartDragging()
    {
        dragging = true;
        firstDown = false;
        // Setze das Schiff direkt unter den Mauszeiger
        var mousePos = Raylib.GetMousePosition();
        DraggedShipRectangle.X = mousePos.X - DraggedShipRectangle.Width / 2f;
        DraggedShipRectangle.Y = mousePos.Y - DraggedShipRectangle.Height / 2f;
        offset = new(
            mousePos.X - DraggedShipRectangle.X,
            mousePos.Y - DraggedShipRectangle.Y
        );
    }
}
