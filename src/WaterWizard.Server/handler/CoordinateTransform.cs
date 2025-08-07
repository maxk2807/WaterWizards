namespace WaterWizard.Server.utils;

/// <summary>
/// Utility class for transforming coordinates between client and server perspectives
/// </summary>
public static class CoordinateTransform
{
    /// <summary>
    /// Transforms opponent board coordinates to match rotated perspective (180 degrees)
    /// </summary>
    /// <param name="x">Original X coordinate</param>
    /// <param name="y">Original Y coordinate</param>
    /// <param name="boardWidth">Width of the board (typically 12)</param>
    /// <param name="boardHeight">Height of the board (typically 10)</param>
    /// <returns>Transformed coordinates</returns>
    public static (int x, int y) RotateOpponentCoordinates(int x, int y, int boardWidth, int boardHeight)
    {
        return (boardWidth - 1 - x, boardHeight - 1 - y);
    }

    /// <summary>
    /// Inverse transformation for opponent board coordinates
    /// </summary>
    public static (int x, int y) UnrotateOpponentCoordinates(int x, int y, int boardWidth, int boardHeight)
    {
        return (boardWidth - 1 - x, boardHeight - 1 - y);
    }
}