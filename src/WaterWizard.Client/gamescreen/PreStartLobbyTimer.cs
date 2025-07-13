// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 23 Zeilen
// - jdewi001: 8 Zeilen
// - maxk2807: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using Raylib_cs;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamescreen;

/// <summary>
/// Zeichnet den Countdown für den Spielstart in der Lobby.
/// /// </summary>
public static class PreStartLobbyTimer
{
    /// <summary>
    /// Zeichnet den Countdown für den Spielstart in der Lobby.
    /// </summary>
    /// <param name="manager">Der GameStateManager, der den Zustand verwaltet.</param>
    /// <returns></returns>
    public static void DrawCountdown(GameStateManager manager)
    {
        var countdown = NetworkManager.Instance.LobbyCountdownSeconds;
        if (countdown.HasValue && countdown.Value > 0)
        {
            string countdownText = $"Game starts in {countdown.Value}...";
            int countdownWidth = Raylib.MeasureText(countdownText, 40);
            Raylib.DrawText(
                countdownText,
                (manager.screenWidth - countdownWidth) / 2,
                manager.screenHeight / 2,
                40,
                Color.Red
            );
        }
    }
}
