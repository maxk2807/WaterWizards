using System;
using Raylib_cs;
using WaterWizard.Client;

class Program
{
    static void Main()
    {
        try
        {
            // Initialisiere Raylib vor GameStateManager
            Raylib.InitWindow(800, 600, "Water Wizard");

            // Initialisiere GameStateManager mit Bildschirmgröße
            GameStateManager.Initialize(800, 600);

            // Hauptspiel-Loop
            while (!Raylib.WindowShouldClose())
            {
                GameStateManager.Instance.UpdateAndDraw();
            }

            // Aufräumen
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
