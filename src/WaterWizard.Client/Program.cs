// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 56 Zeilen
// - jdewi001: 10 Zeilen
// - maxk2807: 3 Zeilen
// - Erickk0: 1 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

﻿using System;
using Raylib_cs;
using WaterWizard.Client;
using WaterWizard.Client.Assets.Sounds.Manager;
class Program
{
    static void Main()
    {
        try
        {
            Raylib.InitAudioDevice();
            SoundManager.LoadSounds();
            const int defaultWidth = 1200;
            const int defaultHeight = 900;

            Raylib.InitWindow(defaultWidth, defaultHeight, "Water Wizard");
            Raylib.SetExitKey(KeyboardKey.Escape); // Escape-Taste zum Beenden

            bool isFullscreen = false;
            int screenWidth = defaultWidth;
            int screenHeight = defaultHeight;

            GameStateManager.Initialize(screenWidth, screenHeight);

            // Hauptspiel-Loop
            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsKeyPressed(KeyboardKey.F11) || Raylib.IsKeyPressed(KeyboardKey.F))
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

            TextureManager.UnloadAllTextures();
            Raylib.CloseWindow();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("Drücke eine beliebige Taste zum Beenden...");
            Console.ReadKey();
        }
        finally{
            SoundManager.UnloadSounds();
            Raylib.CloseAudioDevice();
        }
    }
}
