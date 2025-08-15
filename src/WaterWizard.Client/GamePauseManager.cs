// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 65 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;
using Raylib_cs;

namespace WaterWizard.Client;

/// <summary>
/// Verwaltet das Pausieren und Fortsetzen des Spiels und des Timers.
/// </summary>
public class GamePauseManager
{
    private readonly GameTimer _gameTimer;
    private bool _isGameEffectivelyPaused = false;

    /// <summary>
    /// Gibt an, ob das Spiel aktuell durch diesen Manager pausiert wurde.
    /// </summary>
    public bool IsGamePaused => _isGameEffectivelyPaused;

    /// <summary>
    /// Initialisiert eine neue Instanz des GamePauseManager.
    /// </summary>
    /// <param name="gameTimer">Der GameTimer, der gesteuert werden soll.</param>
    public GamePauseManager(GameTimer gameTimer)
    {
        _gameTimer = gameTimer ?? throw new ArgumentNullException(nameof(gameTimer));
    }

    /// <summary>
    /// Schaltet den Pausenzustand des Spiels um.
    /// </summary>
    public void TogglePause()
    {
        if (_isGameEffectivelyPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pausiert das Spiel und den Spieltimer.
    /// </summary>
    public void PauseGame()
    {
        if (!_isGameEffectivelyPaused)
        {
            _gameTimer.Pause();
            _isGameEffectivelyPaused = true;
            Console.WriteLine("[GamePauseManager] Game paused.");
        }
    }

    /// <summary>
    /// Setzt das Spiel und den Spieltimer fort.
    /// </summary>
    public void ResumeGame()
    {
        if (_isGameEffectivelyPaused)
        {
            _gameTimer.Resume();
            _isGameEffectivelyPaused = false;
            Console.WriteLine("[GamePauseManager] Game resumed.");
        }
    }
}
