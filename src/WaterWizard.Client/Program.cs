using System;
using Raylib_cs;
using WaterWizard.Client;

class Program
{
    static void Main()
    {
        try
        {
            const int defaultWidth = 1024;
            const int defaultHeight = 768;

            Raylib.InitWindow(defaultWidth, defaultHeight, "Water Wizard");
            Raylib.SetExitKey(KeyboardKey.Escape); // Escape-Taste zum Beenden

            bool isFullscreen = false;
            int screenWidth = defaultWidth;
            int screenHeight = defaultHeight;

            GameStateManager.Initialize(screenWidth, screenHeight);

            // Hauptspiel-Loop
            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsKeyPressed(KeyboardKey.F11))
                {
                    isFullscreen = !isFullscreen;

                    if (isFullscreen)
                    {
                        int monitorWidth = Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor());
                        int monitorHeight = Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor());

                        Raylib.ToggleFullscreen();
                        Raylib.SetWindowSize(monitorWidth, monitorHeight);

                        screenWidth = monitorWidth;
                        screenHeight = monitorHeight;
                        GameStateManager.Instance.UpdateScreenSize(screenWidth, screenHeight);
                    }
                    else
                    {
                        if (Raylib.IsWindowFullscreen())
                        {
                            Raylib.ToggleFullscreen();
                        }

                        Raylib.SetWindowSize(defaultWidth, defaultHeight);
                        screenWidth = defaultWidth;
                        screenHeight = defaultHeight;
                        GameStateManager.Instance.UpdateScreenSize(screenWidth, screenHeight);
                    }
                }

                GameStateManager.Instance.UpdateAndDraw();
            }

            Raylib.CloseWindow();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("Drücke eine beliebige Taste zum Beenden...");
            Console.ReadKey();
        }
    }
}