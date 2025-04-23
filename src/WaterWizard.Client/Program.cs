using Raylib_cs;
using System;

namespace WaterWizard.Client
{
    static class Program
    {
        static void Main()
        {
            const int screenWidth = 800;
            const int screenHeight = 600;

            try
            {
                Raylib.InitWindow(screenWidth, screenHeight, "WaterWizards - Battleship Game");
                Raylib.SetTargetFPS(60);

                // Initialisiere den GameStateManager
                var gameStateManager = new GameStateManager(screenWidth, screenHeight);

                while (!Raylib.WindowShouldClose())
                {
                    // Poll Events für Netzwerk
                    NetworkManager.Instance.PollEvents();

                    // Zeichne den aktuellen Spielzustand
                    gameStateManager.UpdateAndDraw();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                NetworkManager.Instance.Shutdown();
                if (Raylib.IsWindowReady())
                {
                    Raylib.CloseWindow();
                }
            }
        }
    }
}
