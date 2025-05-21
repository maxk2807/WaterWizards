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
        Console.WriteLine($"[DrawCountdown] Called. Countdown value: {countdown}");
        if (countdown.HasValue && countdown.Value > 0)
        {
            string countdownText = $"Game starts in {countdown.Value}...";
            int countdownWidth = Raylib.MeasureText(countdownText, 40);
            Raylib.DrawText(countdownText, (manager.screenWidth - countdownWidth) / 2, manager.screenHeight / 2, 40, Color.Red);
        }
    }
}