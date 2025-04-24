using Raylib_cs;

namespace WaterWizard.Client;

/// <summary>
/// GameTimer class handles the game timer functionality.
/// </summary>
/// <param name="gameStateManager">to set the gamestate of the game</param>
public class GameTimer(GameStateManager gameStateManager)
{
    private float _timerSeconds;
    private const int _timeLimitMinutes = 3;
    private const int _timeLimitSeconds = 10;
    private GameStateManager _gameStateManager = gameStateManager;
    public bool IsTimeUp { get; private set; } = false;

    /// <summary>
    /// Updates the game timer. If the time limit is reached, it sets IsTimeUp to true and changes the game state.
    /// </summary>
    public void Update()
    {
        _timerSeconds += Raylib.GetFrameTime();
        if (IsTimeUp) return; 
            _timerSeconds += Raylib.GetFrameTime();
            if (GetMinutes() >= _timeLimitMinutes && _timerSeconds % 60 >= _timeLimitSeconds)
            {
                IsTimeUp = true;
                _gameStateManager.SetStateToMainMenu();
            }
            else if (_timerSeconds >= _timeLimitSeconds)
            {
                IsTimeUp = true;
                _timerSeconds = _timeLimitSeconds;
            }
    }

    /// <summary>
    /// Draws the timer on the screen at the specified position with the specified font size and color.
    /// </summary>
    /// <param name="x">X-Coordinate of the timer</param>
    /// <param name="y">Y-Coordinate of the timer</param>
    /// <param name="fontSize">size of the font of the timer</param>
    /// <param name="color">color of the timer</param>
    public void Draw(int x, int y, int fontSize, Color color)
    {
        int totalSeconds = (int)_timerSeconds;
        int minutes = GetMinutes();
        int seconds = totalSeconds % 60;
        string timerText = $"Time: {minutes:D2}:{seconds:D2}";
        Raylib.DrawText(timerText, x, y, fontSize, color);
    }

    /// <summary>
    /// Calculates the number of minutes from the timer seconds.
    /// </summary>
    /// <returns>Minutes calculated by the seconds</returns>
    private int GetMinutes()
    {
        return (int)_timerSeconds / 60;
    }

    /// <summary>
    /// Resets the timer to 0 seconds.
    /// </summary>
    public void Reset()
    {
        _timerSeconds = 0;
    }

}