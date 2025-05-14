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
    private const int _timeLimitSeconds = 400;
    private GameStateManager _gameStateManager = gameStateManager;
    public bool IsTimeUp { get; private set; } = false;

    private bool _isPaused = false;
    public bool IsPaused => _isPaused;


    /// <summary>
    /// Updates the game timer. If the time limit is reached, it sets IsTimeUp to true and changes the game state.
    /// </summary>
    public void Update()
    {
        if (_isPaused) return;
        _timerSeconds += Raylib.GetFrameTime();
        if (IsTimeUp) return;
        if (GetMinutes() >= _timeLimitMinutes && _timerSeconds % 60 >= _timeLimitSeconds)
        {
            IsTimeUp = true;
            _gameStateManager.SetStateToMainMenu();
        }
    }

    public void Pause()
    {
        _isPaused = true;
        Console.WriteLine("[GameTimer] Game paused.");
    }

    public void Resume()
    {
        _isPaused = false;
        Console.WriteLine("[GameTimer] Game resumed.");
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
        Raylib.DrawText(TimerString(), x, y, fontSize, color);
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
        IsTimeUp = false;
        _isPaused = false;
    }

    public string TimerString()
    {
        int totalSeconds = (int)_timerSeconds;
        int minutes = GetMinutes();
        int seconds = totalSeconds % 60;
        string timerText = $"Time: {minutes:D2}:{seconds:D2}";
        return timerText;
    }

    public int TextWidth(int fontsize)
    {
        return Raylib.MeasureText(TimerString(), fontsize);
    }

    internal static float MaxTextWidth(int fontsize)
    {
        return Raylib.MeasureText("Time: 99:99", fontsize);
    }
}