using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.gamescreen.cards;
using WaterWizard.Client.gamescreen.ships;
using WaterWizard.Client.Gamescreen;

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

    private readonly CellState[,] _gridStates;

    private bool aiming = false;
    private GameCard? cardToAim;

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
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            Point? clickedCell = GetCellFromScreenCoords(mousePos);
            if (clickedCell.HasValue)
            {
                Console.WriteLine(
                    $"Clicked on cell: ({clickedCell.Value.X}, {clickedCell.Value.Y})"
                );
                // TODO: Add logic to handle the click (e.g., send attack to server)
                if (_gridStates[clickedCell.Value.X, clickedCell.Value.Y] == CellState.Unknown)
                {
                    _gridStates[clickedCell.Value.X, clickedCell.Value.Y] = CellState.Miss;
                }
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
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                int posX = (int)Position.X + x * CellSize;
                int posY = (int)Position.Y + y * CellSize;
                
                Color backgroundColor = _gridStates[x, y] switch
                {
                    CellState.Hit => new Color(220, 50, 50, 255),    
                    CellState.Miss => new Color(100, 150, 220, 255), 
                    _ => Color.SkyBlue                              
                };
                
                Raylib.DrawRectangle(posX, posY, CellSize, CellSize, backgroundColor);
                Raylib.DrawRectangleLines(posX, posY, CellSize, CellSize, Color.Gray);
                
                if (_gridStates[x, y] == CellState.Hit)
                {
                    int centerX = posX + CellSize / 2;
                    int centerY = posY + CellSize / 2;
                    int markSize = CellSize / 3;
                    
                    Raylib.DrawLineEx(
                        new Vector2(centerX - markSize, centerY - markSize),
                        new Vector2(centerX + markSize, centerY + markSize),
                        3.0f,
                        Color.White
                    );
                    Raylib.DrawLineEx(
                        new Vector2(centerX + markSize, centerY - markSize),
                        new Vector2(centerX - markSize, centerY + markSize),
                        3.0f,
                        Color.White
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
        
        foreach (GameShip ship in Ships)
        {
            ship.Draw();
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
            _ => Color.Black,
        };
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
            _gridStates[x, y] = state;
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

    // Add method to mark cells as hit by enemy
    public void MarkCellAsHit(int x, int y, bool wasHit)
    {
        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
        {
            _gridStates[x, y] = wasHit ? CellState.Hit : CellState.Miss;
        }
    }
}
