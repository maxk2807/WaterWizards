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
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamestates;

/// <summary>
/// Repräsentiert den Spielzustand während eines laufenden Spiels (In-Game).
/// </summary>
public class InGameState : IGameState
{
    /// <summary>
    /// Aktualisiert den Spielzustand und zeichnet das Spielfeld oder das Pause-Overlay.
    /// </summary>
    /// <param name="manager">GameStateManager mit Zugriff auf Komponenten und Status</param>
    public void UpdateAndDraw(GameStateManager manager)
    {
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
        }
        else
        {
            DrawGameScreen(manager);

            string surrenderHint = "Press 'S' to Surrender";
            int hintWidth = Raylib.MeasureText(surrenderHint, 12);
            Raylib.DrawText(
                surrenderHint,
                manager.screenWidth - hintWidth - 10,
                10,
                12,
                Color.Gray);
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
    /// Zeichnet das Spielfeld und relevante UI-Elemente.
    /// </summary>
    /// <param name="manager">GameStateManager mit Zugriff auf Komponenten und Status</param>
    private static void DrawGameScreen(GameStateManager manager)
    {
        manager.GameScreen.Draw(manager.screenWidth, manager.screenHeight);
    }
}
