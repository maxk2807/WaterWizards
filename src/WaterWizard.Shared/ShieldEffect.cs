using System;
using System.Numerics;

namespace WaterWizard.Shared;

public class ShieldEffect
{
    public Vector2 Position { get; }
    public int PlayerIndex { get; }
    public float RemainingDuration { get; private set; }
    public bool IsActive => RemainingDuration > 0;

    public ShieldEffect(Vector2 position, int playerIndex, float duration)
    {
        Position = position;
        PlayerIndex = playerIndex;
        RemainingDuration = duration;
    }

    public void Update(float deltaTime)
    {
        if (RemainingDuration > 0)
        {
            RemainingDuration -= deltaTime;
            if (RemainingDuration <= 0)
            {
                RemainingDuration = 0;
            }
        }
    }

    /// <summary>
    /// Checks if the given coordinates are protected by this shield
    /// </summary>
    /// <param name="x">X coordinate to check</param>
    /// <param name="y">Y coordinate to check</param>
    /// <returns>True if the coordinates are within the shield's 3x3 area</returns>
    public bool IsCoordinateProtected(int x, int y)
    {
        if (!IsActive) return false;

        return x >= Position.X && x < Position.X + 3 &&
               y >= Position.Y && y < Position.Y + 3;
    }
}
