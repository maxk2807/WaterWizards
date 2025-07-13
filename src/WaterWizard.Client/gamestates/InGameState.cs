// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 36 Zeilen
// - Erickk0: 32 Zeilen
// - erick: 10 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using Raylib_cs;
using WaterWizard.Client.Assets.Sounds.Manager;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamestates;

public class InGameState : IGameState
{
    /// <summary>
    /// Updates the game state and draws the game screen or pause overlay based on the game pause status.
    /// </summary>
    /// <param name="manager">The GameStateManager instance that provides access to game components, screen dimensions, pause state, and rendering systems.</param>
    public void UpdateAndDraw(GameStateManager manager)
    {
        Raylib.UpdateMusicStream(SoundManager.PauseSound);
        if (Raylib.IsKeyPressed(KeyboardKey.S))
        {
            HandleSurrender();
        }
        if (manager.GetGamePauseManager().IsGamePaused)
        {
            Raylib.DrawRectangle(
                0,
                0,
                manager.screenWidth,
                manager.screenHeight,
                new Color(0, 0, 0, 128)
            );
            string pauseText = "PAUSED";
            int textWidth = Raylib.MeasureText(pauseText, 40);
            Raylib.DrawText(
                pauseText,
                (manager.screenWidth - textWidth) / 2,
                manager.screenHeight / 2 - 20,
                40,
                Color.Yellow
            );
            // PauseMusic als Loop abspielen
            if (!Raylib.IsMusicStreamPlaying(SoundManager.PauseSound))
            {
                Raylib.PlayMusicStream(SoundManager.PauseSound);
            }
        }
        else
        {
            // Falls PauseMusic läuft, stoppen
            if (Raylib.IsMusicStreamPlaying(SoundManager.PauseSound))
            {
                Raylib.StopMusicStream(SoundManager.PauseSound);
            }
            DrawGameScreen(manager);

            string surrenderHint = "Press 'S' to Surrender";
            int hintWidth = Raylib.MeasureText(surrenderHint, 12);
            Raylib.DrawText(
                surrenderHint,
                manager.screenWidth - hintWidth - 10,
                10,
                12,
                Color.Gray
            );
        }
    }

    private static void HandleSurrender()
    {
        var client = NetworkManager.Instance.clientService.client;
        if (client != null && client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("Surrender");
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine("[Client] Surrender message sent to server");
        }
        else
        {
            Console.WriteLine("[Client] Cannot surrender - not connected to server");
        }
    }

    /// <summary>
    /// Draws the game screen, including the game state and any relevant UI elements.
    /// </summary>
    /// <param name="manager">The GameStateManager instance that provides access to game components, screen dimensions, pause state, and rendering systems.</param>
    private static void DrawGameScreen(GameStateManager manager)
    {
        manager.GameScreen.Draw(manager.screenWidth, manager.screenHeight);
    }
}
