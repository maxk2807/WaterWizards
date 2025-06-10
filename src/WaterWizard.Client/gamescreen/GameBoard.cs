using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.gamescreen.cards;
using WaterWizard.Client.gamescreen.ships;
using WaterWizard.Client.Gamescreen;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamescreen;


/// <summary>
/// GameBoard class represents the game board for the battleship game.
/// </summary>
public class GameBoard
{
    public List<GameShip> Ships { get; private set; } = [];

    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public int CellSize { get; set; }
    public Vector2 Position { get; set; }

    public readonly CellState[,] _gridStates;

    private bool aiming = false;
    private GameCard? cardToAim;

    private List<ThunderStrike> _activeThunderStrikes = [];
    private readonly Random _random = new();

    private class ThunderStrike
    {
        public Vector2 Position { get; set; }
        public float Duration { get; set; }
        public float MaxDuration { get; } = 0.75f;
        public float Alpha => Duration / MaxDuration;
        public bool IsActive => Duration > 0;
        private Vector2[] miniLightningPoints;
        private readonly Random random = new();

        public ThunderStrike(Vector2 position)
        {
            Position = position;
            Duration = MaxDuration;

            // Generiere zufällige Punkte für Mini-Blitze
            miniLightningPoints = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = (float)(random.NextDouble() * 30 + 20);
                miniLightningPoints[i] = new Vector2(
                    (float)(Math.Cos(angle) * distance),
                    (float)(Math.Sin(angle) * distance)
                );
            }
        }

        public void Update(float deltaTime)
        {
            Duration -= deltaTime;
        }

        public void Draw(float cellSize)
        {
            float alpha = Alpha;
            if (alpha <= 0) return;

            // Hauptleuchteffekt
            float glowSize = cellSize * (0.75f + 0.5f * alpha);
            Color glowColor = new(255, 255, 100, (int)(100 * alpha));
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, glowSize, glowColor);

            // Blitzeffekte
            Color thunderColor = new(255, 255, 0, (int)(255 * alpha));

            // Hauptblitz
            float size = cellSize * 2.5f;
            Vector2 end = new(Position.X, Position.Y - size);

            for (int i = 0; i < 3; i++)
            {
                float offset = (float)(random.NextDouble() * 12 - 6) * alpha;
                Vector2 mid1 = new(Position.X + offset, Position.Y - size / 3);
                Vector2 mid2 = new(Position.X - offset, Position.Y - size * 2 / 3);

                float lineWidth = 2f + alpha * 2f;
                Raylib.DrawLineEx(Position, mid1, lineWidth, thunderColor);
                Raylib.DrawLineEx(mid1, mid2, lineWidth, thunderColor);
                Raylib.DrawLineEx(mid2, end, lineWidth, thunderColor);
            }

            // Mini-Blitze
            foreach (var point in miniLightningPoints)
            {
                Vector2 endPoint = new(
                    Position.X + point.X * alpha,
                    Position.Y + point.Y * alpha
                );
                Raylib.DrawLineEx(Position, endPoint, 1f, thunderColor);
            }
        }
    }

    /// <summary>
    /// Constructor for the GameBoard class.
    /// </summary>
    /// <param name="gridWidth">The width of the game board grid in cells.</param>
    /// <param name="gridHeight">The height of the game board grid in cells.</param>
    /// <param name="cellSize">The size of each cell in pixels.</param>
    /// <param name="position">The top-left position of the game board on the screen.</param>
    public GameBoard(int gridWidth, int gridHeight, int cellSize, Vector2 position)
    {
        GridWidth = gridWidth;
        GridHeight = gridHeight;
        CellSize = cellSize;
        Position = position;

        _gridStates = new CellState[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                _gridStates[x, y] = CellState.Unknown;
            }
        }
    }

    public void putShip(GameShip ship)
    {
        Ships.Add(ship);

        // Aktualisiere den Zellstatus für das platzierte Schiff
        int startX = (int)((ship.X - Position.X) / CellSize);
        int startY = (int)((ship.Y - Position.Y) / CellSize);
        int width = ship.Width / CellSize;
        int height = ship.Height / CellSize;

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
                {
                    _gridStates[x, y] = CellState.Ship;
                }
            }
        }
    }

    /// <summary>
    /// Converts screen coordinates to grid cell coordinates.
    /// Returns null if the coordinates are outside the board.
    /// </summary>
    public Point? GetCellFromScreenCoords(Vector2 screenPos)
    {
        if (IsPointOutside(screenPos))
        {
            return null;
        }

        int gridX = (int)((screenPos.X - Position.X) / CellSize);
        int gridY = (int)((screenPos.Y - Position.Y) / CellSize);

        return new Point(gridX, gridY);
    }

    public bool IsPointOutside(Vector2 screenPos)
    {
        return screenPos.X < Position.X
            || screenPos.Y < Position.Y
            || screenPos.X >= Position.X + (float)GridWidth * CellSize
            || screenPos.Y >= Position.Y + (float)GridHeight * CellSize;
    }

    /// <summary>
    /// Updates the game board state, primarily handling input.
    /// Returns the clicked cell coordinates if a valid cell was clicked, otherwise null.
    /// </summary>
    public Point? Update()
    {
        float deltaTime = Raylib.GetFrameTime();

        // Update Thunder-Animationen
        for (int i = _activeThunderStrikes.Count - 1; i >= 0; i--)
        {
            var strike = _activeThunderStrikes[i];
            strike.Update(deltaTime);

            // Wenn die Animation beendet ist, entferne den Strike
            if (!strike.IsActive)
            {
                _activeThunderStrikes.RemoveAt(i);
            }
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            Point? clickedCell = GetCellFromScreenCoords(mousePos);
            if (clickedCell.HasValue)
            {
                Console.WriteLine($"Clicked on cell: ({clickedCell.Value.X}, {clickedCell.Value.Y})");
                return clickedCell;
            }
        }
        return null;
    }

    /// <summary>
    /// Draws the game board grid and the state of each cell.
    /// </summary>
    public void Draw()
    {
        // Zeichne das Grundbrett
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                int posX = (int)Position.X + x * CellSize;
                int posY = (int)Position.Y + y * CellSize;

                // Zeichne die Grundfarbe der Zelle
                Color cellColor = GetColorForState(_gridStates[x, y]);
                Raylib.DrawRectangle(posX, posY, CellSize, CellSize, cellColor);
                Raylib.DrawRectangleLines(posX, posY, CellSize, CellSize, Color.DarkGray);
            }
        }

        // Zeichne die Schiffe
        foreach (GameShip ship in Ships)
        {
            ship.Draw();
        }

        // Zeichne die Treffer-Markierungen ÜBER den Schiffen
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                int posX = (int)Position.X + x * CellSize;
                int posY = (int)Position.Y + y * CellSize;

                // Zeichne Treffer-Markierungen
                if (_gridStates[x, y] == CellState.Hit)
                {
                    // Roter Kreis für Treffer
                    Raylib.DrawCircle(
                        posX + CellSize / 2,
                        posY + CellSize / 2,
                        (float)CellSize / 4,
                        Color.Red
                    );

                    // Zusätzliche X-Markierung für bessere Sichtbarkeit
                    float margin = CellSize * 0.2f;
                    Color hitColor = new(200, 0, 0, 255);
                    Raylib.DrawLineEx(
                        new(posX + margin, posY + margin),
                        new(posX + CellSize - margin, posY + CellSize - margin),
                        2f,
                        hitColor
                    );
                    Raylib.DrawLineEx(
                        new(posX + CellSize - margin, posY + margin),
                        new(posX + margin, posY + CellSize - margin),
                        2f,
                        hitColor
                    );
                }
                else if (_gridStates[x, y] == CellState.Miss)
                {
                    int centerX = posX + CellSize / 2;
                    int centerY = posY + CellSize / 2;
                    float radius = (float)CellSize / 4;

                    Raylib.DrawCircle(centerX, centerY, radius + 1, Color.DarkBlue);
                    Raylib.DrawCircle(centerX, centerY, radius, Color.White);
                }
            }
        }

        // Zeichne die Zielvorschau
        if (aiming)
        {
            DrawCastAim(cardToAim!);
        }

        // Thunder-Effekte zeichnen
        foreach (var strike in _activeThunderStrikes)
        {
            strike.Draw(CellSize);
        }
    }

    /// <summary>
    /// Returns the color associated with a cell state.
    /// </summary>
    /// <param name="state">State for the Cell after an Attach</param>
    /// <returns></returns>
    private static Color GetColorForState(CellState state)
    {
        return state switch
        {
            CellState.Empty => Color.LightGray,
            CellState.Ship => Color.Gray,
            CellState.Hit => Color.Orange,
            CellState.Miss => Color.Blue,
            CellState.Unknown => Color.SkyBlue,
            CellState.Thunder => new Color(30, 30, 150, 255), // Dunkelblau für Thunder
            _ => Color.Black,
        };
    }

    public void DrawCastAim(GameCard gameCard)
    {
        var mousePos = Raylib.GetMousePosition();
        Vector2 aim = gameCard.card.TargetAsVector();

        // Spezialbehandlung für battlefield-Ziele wie Thunder
        if (gameCard.card.Target!.Target == "battlefield")
        {
            Raylib.DrawText(
                "Klicken Sie irgendwo, um die Karte zu wirken",
                (int)mousePos.X,
                (int)mousePos.Y - 20,
                20,
                Color.Black
            );

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                aiming = false;
                NetworkManager.HandleCast(cardToAim!.card, new Point(0, 0));
            }
            return;
        }

        // Normale Zielbehandlung für andere Kartentypen
        if ((int)aim.X == 0 && (int)aim.Y == 0)
        {
            //TODO: handle other types of aims
            return;
        }

        Point? hoveredCoords;
        Vector2 boardPos;
        if (gameCard.card.Target!.Ally)
        {
            hoveredCoords = GetCellFromScreenCoords(mousePos);
            boardPos = Position;
        }
        else
        {
            hoveredCoords =
                GameStateManager.Instance.GameScreen.opponentBoard!.GetCellFromScreenCoords(
                    mousePos
                );
            boardPos = GameStateManager.Instance.GameScreen.opponentBoard!.Position;
        }

        if (!hoveredCoords.HasValue)
        {
            return;
        }

        Raylib.DrawText(
            "Click again to cast Card",
            (int)mousePos.X,
            (int)mousePos.Y - 20,
            20,
            Color.Black
        );

        var onScreenX =
            boardPos.X + (hoveredCoords.Value.X - (float)Math.Floor(aim.X / 2f)) * CellSize;
        var onScreenY =
            boardPos.Y + (hoveredCoords.Value.Y - (float)Math.Floor(aim.Y / 2f)) * CellSize;
        var r = new Rectangle(onScreenX, onScreenY, aim.X * CellSize, aim.Y * CellSize);
        Raylib.DrawRectangleLinesEx(r, 2, Color.Red);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            aiming = false;
            NetworkManager.HandleCast(cardToAim!.card, hoveredCoords.Value);
        }
    }

    public void StartDrawingCardAim(GameCard gameCard)
    {
        aiming = true;
        cardToAim = gameCard;
    }

    /// <summary>
    /// Represents a point in the grid with X and Y coordinates.
    /// </summary>
    /// <param name="X">X-Coordinate</param>
    /// <param name="Y">Y-Coordinate</param>
    public readonly record struct Point(int X, int Y);

    public void SetCellState(int x, int y, CellState state)
    {
        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
        {
            if (state == CellState.Hit || state == CellState.Miss)
            {
                _gridStates[x, y] = state;
            }
            else if (_gridStates[x, y] != CellState.Hit && _gridStates[x, y] != CellState.Ship)
            {
                _gridStates[x, y] = state;
            }
            
            Console.WriteLine($"[GameBoard] SetCellState: ({x},{y}) = {state}");
        }
    }

    /// <summary>
    /// Marks a cell as hit or missed based on the attack result.
    /// </summary>
    /// <param name="x">X coordinate of the cell</param>
    /// <param name="y">Y coordinate of the cell</param>
    /// <param name="hit">True if the attack hit, false if it missed</param>
    public void MarkCellAsHit(int x, int y, bool hit)
    {
        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
        {
            _gridStates[x, y] = hit ? CellState.Hit : CellState.Miss;
            
            Console.WriteLine($"[GameBoard] Cell ({x},{y}) marked as {(hit ? "HIT" : "MISS")} - State: {_gridStates[x, y]}");
        }
    }

    /// <summary>
    /// Clears the game board, removing all ships and resetting cell states to Unknown.
    /// </summary>
    public void ClearBoard()
    {
        Ships.Clear();
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                _gridStates[x, y] = CellState.Unknown;
            }
        }
        aiming = false;
        cardToAim = null;

        Console.WriteLine("[Client][GameBoard]GameBoard cleared.");
    }

    /// <summary>
    /// Resets the cell states to Unknown without clearing the ships.
    /// </summary>
    public void ResetCellStates()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                _gridStates[x, y] = CellState.Unknown;
            }
        }
    }

    public void AddThunderStrike(int x, int y)
    {
        // Position für den Blitzeinschlag berechnen
        Vector2 position = new(
            Position.X + (float)x * (float)CellSize + (float)CellSize / 2f,
            Position.Y + (float)y * (float)CellSize + (float)CellSize / 2f
        );

        // Neuen Blitzeinschlag hinzufügen
        _activeThunderStrikes.Add(new ThunderStrike(position));

        // Markiere das getroffene Feld
        if (x < GridWidth && y < GridHeight)
        {
            // Wenn das Feld bereits als "Hit" markiert ist, nicht überschreiben
            if (_gridStates[x, y] != CellState.Hit)
            {
                _gridStates[x, y] = CellState.Thunder;
            }
        }
    }

    public void ResetThunderFields()
    {
        // Setze alle Thunder-Felder zurück auf ihren vorherigen Zustand
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                // Nur Thunder-Felder zurücksetzen, Hit-Felder bleiben bestehen
                if (_gridStates[x, y] == CellState.Thunder)
                {
                    _gridStates[x, y] = CellState.Unknown;
                }
            }
        }
        _activeThunderStrikes.Clear();
    }
}
