// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Erickk0: 51 Zeilen
// - justinjd00: 24 Zeilen
// - maxk2807: 17 Zeilen
// - jdewi001: 6 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using Raylib_cs;

namespace WaterWizard.Client;

/// <summary>
/// Verwaltet die Spielzeit, das Pausieren und das Anzeigen des Timers.
/// </summary>
public class GameTimer(GameStateManager gameStateManager)
{
    private float _timerSeconds;
    private const int _timeLimitMinutes = 3;
    private const int _timeLimitSeconds = 400;
    private GameStateManager _gameStateManager = gameStateManager;

    /// <summary>
    /// Gibt an, ob die Zeit abgelaufen ist.
    /// </summary>
    public bool IsTimeUp { get; private set; } = false;

    private bool _isPaused = false;
    /// <summary>
    /// Gibt an, ob der Timer aktuell pausiert ist.
    /// </summary>
    public bool IsPaused => _isPaused;

    /// <summary>
    /// Aktualisiert den Timer. Setzt IsTimeUp und wechselt ins Hauptmenü, wenn die Zeit abgelaufen ist.
    /// </summary>
    public void Update()
    {
        if (_isPaused)
            return;
        _timerSeconds += Raylib.GetFrameTime();
        if (IsTimeUp)
            return;
        if (GetMinutes() >= _timeLimitMinutes && _timerSeconds % 60 >= _timeLimitSeconds)
        {
            IsTimeUp = true;
            _gameStateManager.SetStateToMainMenu();
        }
    }

    /// <summary>
    /// Pausiert den Timer.
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
        Console.WriteLine("[GameTimer] Game paused.");
    }

    /// <summary>
    /// Setzt den Timer fort.
    /// </summary>
    public void Resume()
    {
        _isPaused = false;
        Console.WriteLine("[GameTimer] Game resumed.");
    }

    /// <summary>
    /// Zeichnet den Timer an die angegebene Position.
    /// </summary>
    /// <param name="x">X-Koordinate</param>
    /// <param name="y">Y-Koordinate</param>
    /// <param name="fontSize">Schriftgröße</param>
    /// <param name="color">Farbe</param>
    public void Draw(int x, int y, int fontSize, Color color)
    {
        Raylib.DrawText(TimerString(), x, y, fontSize, color);
    }

    /// <summary>
    /// Berechnet die Anzahl der Minuten aus den Timer-Sekunden.
    /// </summary>
    /// <returns>Minuten, die aus den Sekunden berechnet wurden</returns>
    private int GetMinutes()
    {
        return (int)_timerSeconds / 60;
    }

    /// <summary>
    /// Setzt den Timer zurück.
    /// </summary>
    public void Reset()
    {
        _timerSeconds = 0;
        IsTimeUp = false;
        _isPaused = false;
    }

    /// <summary>
    /// Gibt die aktuelle Zeit als formatierten String zurück.
    /// </summary>
    /// <returns>String im Format "Time: mm:ss"</returns>
    public string TimerString()
    {
        int totalSeconds = (int)_timerSeconds;
        int minutes = GetMinutes();
        int seconds = totalSeconds % 60;
        string timerText = $"Time: {minutes:D2}:{seconds:D2}";
        return timerText;
    }

    /// <summary>
    /// Gibt die Textbreite des Timers für die angegebene Schriftgröße zurück.
    /// </summary>
    /// <param name="fontsize">Schriftgröße</param>
    /// <returns>Textbreite in Pixel</returns>
    public int TextWidth(int fontsize)
    {
        return Raylib.MeasureText(TimerString(), fontsize);
    }

    /// <summary>
    /// Gibt die maximale Textbreite für den Timer zurück.
    /// </summary>
    /// <param name="fontsize">Schriftgröße</param>
    /// <returns>Maximale Textbreite in Pixel</returns>
    internal static float MaxTextWidth(int fontsize)
    {
        return Raylib.MeasureText("Time: 99:99", fontsize);
    }
}
