// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Erickk0: 269 Zeilen
// - jdewi001: 194 Zeilen
// - erick: 186 Zeilen
// - maxk2807: 125 Zeilen
// - justinjd00: 50 Zeilen
// - Paul: 1 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - private readonly Random _random = new();   (jdewi001: 1 Zeilen)
// - private Dictionary<(int x, int y), float> _shieldedCells = new();   (Erickk0: 22 Zeilen)
// - private readonly Random random = new();   (jdewi001: 161 Zeilen)
// - public readonly record struct Point(int X, int Y);   (erick: 178 Zeilen)
// ===============================================

using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.Gamescreen;
using WaterWizard.Client.gamescreen.cards;
using WaterWizard.Client.gamescreen.ships;
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

    // Shield effect tracking
    private Dictionary<(int x, int y), float> _shieldedCells = new();
    private class ShieldEffect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Duration { get; set; }
        public float MaxDuration { get; } = 6.0f; // 6 seconds
        public float Alpha => Math.Min(1.0f, Duration / MaxDuration);
        public bool IsActive => Duration > 0;

        public ShieldEffect(int x, int y)
        {
            X = x;
            Y = y;
            Duration = MaxDuration;
        }

        public void Update(float deltaTime)
        {
            Duration -= deltaTime;
        }
    }

    private class ThunderStrike
    {
        public Vector2 Position { get; set; }
        public float Duration { get; set; }
        public float MaxDuration { get; } = 1.0f;
        public float Alpha => Duration / MaxDuration;
        public bool IsActive => Duration > 0;
        public bool Hit { get; set; }
        private Vector2[] miniLightningPoints;
        private readonly Random random = new();

        public ThunderStrike(Vector2 position, bool hit = false)
        {
            Position = position;
            Duration = MaxDuration;
            Hit = hit;

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
            if (alpha <= 0)
                return;

            Color baseColor = Hit ?
                new Color(255, 100, 100, (int)(255 * alpha)) :
                new Color(255, 255, 0, (int)(255 * alpha));

            Color glowColor = Hit ?
                new Color(255, 150, 150, (int)(100 * alpha)) :
                new Color(255, 255, 100, (int)(100 * alpha));

            float glowSize = cellSize * (0.75f + 0.5f * alpha);
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, glowSize, glowColor);

            float size = cellSize * 2.5f;
            Vector2 end = new(Position.X, Position.Y - size);

            for (int i = 0; i < 3; i++)
            {
                float offset = (float)(random.NextDouble() * 12 - 6) * alpha;
                Vector2 mid1 = new(Position.X + offset, Position.Y - size / 3);
                Vector2 mid2 = new(Position.X - offset, Position.Y - size * 2 / 3);

                float lineWidth = 2f + alpha * 2f;
                Raylib.DrawLineEx(Position, mid1, lineWidth, baseColor);
                Raylib.DrawLineEx(mid1, mid2, lineWidth, baseColor);
                Raylib.DrawLineEx(mid2, end, lineWidth, baseColor);
            }

            foreach (var point in miniLightningPoints)
            {
                Vector2 endPoint = new(Position.X + point.X * alpha, Position.Y + point.Y * alpha);
                Raylib.DrawLineEx(Position, endPoint, 1f, baseColor);
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
    /// Handles Changing the Cell States of moving a Ship (e.g. due to CallWind Card Casting). 
    /// </summary>
    /// <param name="ship">the ship to be moved</param>
    /// <param name="oldCoords">the old Coordinates of the ship (in board coords so the small numbers)</param>
    /// <param name="newCoords">the new Coordinates of the ship (in board coords so small numbers)</param>
    public void MoveShip(GameShip ship, Vector2 oldCoords, Vector2 newCoords)
    {
        int startX = (int)oldCoords.X;
        int startY = (int)oldCoords.Y;
        int width = ship.Width / CellSize;
        int height = ship.Height / CellSize;
        List<(int X, int Y)> hit = [];

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
                {
                    if (_gridStates[x, y] == CellState.Hit)
                    {
                        hit.Add((x, y));
                        _gridStates[x, y] = CellState.Miss;
                    }
                    else
                    {
                        Console.WriteLine($"Set to Unknown you know for moving and stuff: {(x, y)}, {_gridStates[x, y]}");
                        _gridStates[x, y] = CellState.Unknown;
                    }
                }
            }
        }



        startX = (int)newCoords.X;
        startY = (int)newCoords.Y;

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
                {
                    if (hit.Any(cell => cell.X == x && cell.Y == y))
                    {
                        _gridStates[x, y] = CellState.Hit;

                    }
                    else
                    {
                        Console.WriteLine($"Set to Ship you know for moving and stuff: {(x, y)}, {_gridStates[x, y]}");
                        _gridStates[x, y] = CellState.Ship;
                    }
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

        // Update shield effects
        UpdateShieldEffects(deltaTime);

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            Point? clickedCell = GetCellFromScreenCoords(mousePos);
            if (clickedCell.HasValue)
            {
                Console.WriteLine(
                    $"Clicked on cell: ({clickedCell.Value.X}, {clickedCell.Value.Y})"
                );
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

        // Thunder-Effekte zeichnen
        foreach (var strike in _activeThunderStrikes)
        {
            strike.Draw(CellSize);
        }

        // Shield-Effekte zeichnen
        DrawShieldEffects();
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
            // CellState.Ship => Color.Gray, //because of texture now blank
            CellState.Ship => Color.Blank,
            CellState.Rock => Color.DarkGray,
            CellState.Hit => Color.Blank,
            CellState.Miss => Color.Blue,
            CellState.Unknown => new Color(135, 206, 235, 0), //transparenz hinzugefügt um den background sichtbar zu machen 
            CellState.Thunder => new Color(30, 30, 150, 255), // Dunkelblau für Thunder
            CellState.Shield => new Color(0, 255, 255, 100), // Cyan für Shield mit Transparenz
            _ => Color.Black,
        };
    }

    public void DrawCastAim(GameCard gameCard)
    {
        var mousePos = Raylib.GetMousePosition();
        Vector2 aim = gameCard.card.TargetAsVector();

        // Spezialbehandlung für battlefield-Ziele wie Paralize
        if (gameCard.card.Target!.Target == "battlefield")
        {
            Raylib.DrawText(
                $"Klicken Sie irgendwo, um {gameCard.card.Variant} zu wirken",
                (int)mousePos.X - 100,
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
            if (_gridStates[x, y] == CellState.Hit && state == CellState.Ship)
            {
                _gridStates[x, y] = state;
                Console.WriteLine($"[GameBoard] SetCellState: ({x},{y}) = {state}");
                return;
            }

            if ((_gridStates[x, y] == CellState.Hit || _gridStates[x, y] == CellState.Miss) &&
                state != CellState.Hit && state != CellState.Miss)
            {
                Console.WriteLine($"[GameBoard] SetCellState: ({x},{y}) already has final state {_gridStates[x, y]}, ignoring {state}");
                return;
            }

            _gridStates[x, y] = state;
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

            Console.WriteLine(
                $"[GameBoard] Cell ({x},{y}) marked as {(hit ? "HIT" : "MISS")} - State: {_gridStates[x, y]}"
            );
        }
    }

    /// <summary>
    /// Marks a cell as revealed by hovering eye (different from attack reveals)
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="hasShip">Whether there's a ship at this location</param>
    public void MarkCellAsHoveringEyeRevealed(int x, int y, bool hasShip)
    {
        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
        {
            if (hasShip)
            {
                _gridStates[x, y] = CellState.Ship;
            }
            else
            {
                _gridStates[x, y] = CellState.HoveringEyeRevealed;
            }
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

        _shieldedCells.Clear();
        _activeThunderStrikes.Clear();

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

    public void AddThunderStrike(int x, int y, bool hit = false)
    {
        Vector2 position = new(
            Position.X + x * (float)CellSize + CellSize / 2f,
            Position.Y + y * (float)CellSize + CellSize / 2f
        );

        var strike = new ThunderStrike(position, hit);
        _activeThunderStrikes.Add(strike);

        Console.WriteLine($"[GameBoard] Added thunder visual effect at ({x}, {y})");

        SetCellState(x, y, hit ? CellState.Hit : CellState.Miss);
    }

    public void ResetThunderFields()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (_gridStates[x, y] == CellState.Thunder)
                {
                    _gridStates[x, y] = CellState.Unknown;
                }
            }
        }
        _activeThunderStrikes.Clear();
    }

    /// <summary>
    /// Adds a shield effect to the game board at the specified position
    /// </summary>
    /// <param name="x">X coordinate of the shield center</param>
    /// <param name="y">Y coordinate of the shield center</param>
    /// <param name="duration">Duration of the shield effect</param>
    public void AddShieldEffect(int x, int y, float duration)
    {
        Console.WriteLine($"[GameBoard] Adding shield effect at CENTER ({x}, {y}) with duration {duration}");
        
        for (int dx = -1; dx <= 1; dx++)  
        {
            for (int dy = -1; dy <= 1; dy++)  
            {
                int shieldX = x + dx;
                int shieldY = y + dy;
                
                if (shieldX >= 0 && shieldX < GridWidth && shieldY >= 0 && shieldY < GridHeight)
                {
                    if (_shieldedCells.ContainsKey((shieldX, shieldY)))
                    {
                        _shieldedCells[(shieldX, shieldY)] = Math.Max(_shieldedCells[(shieldX, shieldY)], duration);
                    }
                    else
                    {
                        _shieldedCells[(shieldX, shieldY)] = duration;
                    }
                    Console.WriteLine($"[GameBoard] Shield cell added at ({shieldX}, {shieldY}) for {duration} seconds [offset: dx={dx}, dy={dy}]");
                }
                else
                {
                    Console.WriteLine($"[GameBoard] Shield cell ({shieldX}, {shieldY}) is OUT OF BOUNDS [offset: dx={dx}, dy={dy}]");
                }
            }
        }
        
        Console.WriteLine($"[GameBoard] Shield placement complete. Total shielded cells: {_shieldedCells.Count}");
    }

    /// <summary>
    /// Removes a shield effect from the game board at the specified position
    /// </summary>
    /// <param name="x">X coordinate of the shield center</param>
    /// <param name="y">Y coordinate of the shield center</param>
    public void RemoveShieldEffect(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int shieldX = x + dx;
                int shieldY = y + dy;

                if (shieldX >= 0 && shieldX < GridWidth && shieldY >= 0 && shieldY < GridHeight)
                {
                    _shieldedCells.Remove((shieldX, shieldY));
                    Console.WriteLine($"[GameBoard] Shield cell removed at ({shieldX}, {shieldY})");
                }
            }
        }
    }

    /// <summary>
    /// Checks if a cell is currently shielded
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>True if the cell is shielded</returns>
    public bool IsCellShielded(int x, int y)
    {
        return _shieldedCells.ContainsKey((x, y)) && _shieldedCells[(x, y)] > 0;
    }

    /// <summary>
    /// Updates all active shield effects, reducing their duration and removing expired ones
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds</param>
    public void UpdateShieldEffects(float deltaTime)
    {
        var keysToRemove = new List<(int, int)>();

        foreach (var kvp in _shieldedCells.ToList())
        {
            var (x, y) = kvp.Key;
            float duration = kvp.Value - deltaTime;

            if (duration <= 0)
            {
                keysToRemove.Add((x, y));
            }
            else
            {
                _shieldedCells[(x, y)] = duration;
            }
        }

        foreach (var key in keysToRemove)
        {
            _shieldedCells.Remove(key);
            Console.WriteLine($"[GameBoard] Shield expired at ({key.Item1}, {key.Item2})");
        }
    }

    private void DrawCell(int x, int y)
    {
        int cellX = (int)Position.X + x * CellSize;
        int cellY = (int)Position.Y + y * CellSize;

        switch (_gridStates[x, y])
        {
            case CellState.Empty:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, Color.LightGray);
                break;
            case CellState.Ship:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, Color.Gray);
                break;
            case CellState.Rock:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, Color.DarkGray);
                break;
            case CellState.Hit:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, Color.Orange);
                break;
            case CellState.Miss:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, Color.DarkBlue);
                break;
            case CellState.Shield:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, GetColorForState(CellState.Shield));
                break;
            case CellState.Unknown:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, new Color(135, 206, 235, 0));
                break;
            case CellState.Thunder:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, new Color(30, 30, 150, 255));
                break;
            case CellState.HoveringEyeRevealed:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, new Color(100, 150, 255, 100));
                Raylib.DrawRectangleLines(cellX, cellY, CellSize, CellSize, Color.Blue);
                break;
            default:
                Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, Color.Black);
                break;
        }

        // Draw shield effect overlay if cell is shielded
        if (IsCellShielded(x, y))
        {
            float duration = _shieldedCells[(x, y)];
            float alpha = Math.Min(1.0f, duration / 6.0f); // Fade out as duration decreases
            Color shieldColor = new Color(0, 255, 255, (int)(150 * alpha)); // Cyan with transparency

            Raylib.DrawRectangle(cellX, cellY, CellSize, CellSize, shieldColor);
            Raylib.DrawRectangleLines(cellX, cellY, CellSize, CellSize, new Color(0, 200, 200, (int)(255 * alpha)));
        }
    }

    /// <summary>
    /// Draws shield effects on the game board
    /// </summary>
    private void DrawShieldEffects()
    {
        foreach (var kvp in _shieldedCells)
        {
            var (x, y) = kvp.Key;
            float duration = kvp.Value;

            int posX = (int)Position.X + x * CellSize;
            int posY = (int)Position.Y + y * CellSize;

            float alpha = Math.Min(1.0f, duration / 6.0f);
            int alphaValue = (int)(alpha * 150);

            Color shieldColor = new(0, 255, 255, alphaValue);
            Raylib.DrawRectangle(posX, posY, CellSize, CellSize, shieldColor);

            Color borderColor = new(0, 200, 200, Math.Min(255, alphaValue + 100));
            Raylib.DrawRectangleLines(posX, posY, CellSize, CellSize, borderColor);

            int centerX = posX + CellSize / 2;
            int centerY = posY + CellSize / 2;
            int symbolSize = CellSize / 4;

            Vector2[] points = new Vector2[]
            {
                new(centerX, centerY - symbolSize),
                new(centerX + symbolSize, centerY),
                new(centerX, centerY + symbolSize),
                new(centerX - symbolSize, centerY)
            };

            Color symbolColor = new(255, 255, 255, Math.Min(255, alphaValue + 100));
            for (int i = 0; i < points.Length; i++)
            {
                int next = (i + 1) % points.Length;
                Raylib.DrawLineEx(points[i], points[next], 2f, symbolColor);
            }
        }
    }
    
    /// <summary>
    /// Clears all shield effects from the game board
    /// </summary>
    public void ClearAllShieldEffects()
    {
        _shieldedCells.Clear();
        Console.WriteLine("[GameBoard] All shield effects cleared");
    }
}
