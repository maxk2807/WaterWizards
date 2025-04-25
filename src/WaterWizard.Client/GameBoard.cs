using Raylib_cs;
using System.Numerics;

namespace WaterWizard.Client;

/// <summary>
/// CellState enum represents the possible states of a cell in the game board.
/// </summary>
public enum CellState
{
    Empty,
    Ship, 
    Hit,
    Miss,
    Unknown 
}

/// <summary>
/// GameBoard class represents the game board for the battleship game.
/// </summary>
public class GameBoard
{
    public int GridWidth { get; }
    public int GridHeight { get; }
    public int CellSize { get; }
    public Vector2 Position { get; set; } 

    private CellState[,] gridStates;

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

        gridStates = new CellState[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                gridStates[x, y] = CellState.Unknown;
            }
        }
    }

    /// <summary>
    /// Converts screen coordinates to grid cell coordinates.
    /// Returns null if the coordinates are outside the board.
    /// </summary>
    public Point? GetCellFromScreenCoords(Vector2 screenPos)
    {
        if (screenPos.X < Position.X || screenPos.Y < Position.Y ||
            screenPos.X >= Position.X + (float)GridWidth * CellSize ||
            screenPos.Y >= Position.Y + (float)GridHeight * CellSize)
        {
            return null; 
        }

        int gridX = (int)((screenPos.X - Position.X) / CellSize);
        int gridY = (int)((screenPos.Y - Position.Y) / CellSize);

        return new Point(gridX, gridY);
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
                Console.WriteLine($"Clicked on cell: ({clickedCell.Value.X}, {clickedCell.Value.Y})");
                // TODO: Add logic to handle the click (e.g., send attack to server)
                if (gridStates[clickedCell.Value.X, clickedCell.Value.Y] == CellState.Unknown)
                {
                        gridStates[clickedCell.Value.X, clickedCell.Value.Y] = CellState.Miss;
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

                Color cellColor = GetColorForState(gridStates[x, y]);
                Raylib.DrawRectangle(posX, posY, CellSize, CellSize, cellColor);

                Raylib.DrawRectangleLines(posX, posY, CellSize, CellSize, Color.DarkGray);

                if (gridStates[x, y] == CellState.Hit)
                {
                    Raylib.DrawCircle(posX + CellSize / 2, posY + CellSize / 2, (float)CellSize / 4, Color.Red);
                }
                else if (gridStates[x, y] == CellState.Miss)
                {
                        Raylib.DrawCircle(posX + CellSize / 2, posY + CellSize / 2, (float)CellSize / 4, Color.White);
                }
            }
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
}
