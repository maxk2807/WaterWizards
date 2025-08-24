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

using System;
using System.Diagnostics;
using Raylib_cs;
using WaterWizard.Client;
using WaterWizard.Client.Assets.Sounds.Manager;

/// <summary>
/// Einstiegspunkt-Anwendungsklasse für Water Wizard.
/// Verwaltet das Starten und Beenden des Programms sowie die
/// Initialisierung und Freigabe globaler Ressourcen (Audio, Texturen).
/// </summary>
class Program
{
    
    /// <summary>
    /// Startet das Spiel: Initialisiert Audio und Fenster, lädt Sounds,
    /// erstellt den <see cref="GameStateManager"/>, behandelt Fenstergröße
    /// und Vollbild-Toggle (F11/F) und führt den Hauptspiel-Loop mit fixer
    /// Ziel-Framerate (≈60 FPS) aus. Bei Größenänderungen wird die UI via
    /// <c>GameStateManager.Instance.UpdateScreenSize</c> angepasst.
    /// Fehler werden abgefangen und protokolliert; in jedem Fall werden am Ende
    /// alle Ressourcen (Texturen, Sounds, AudioDevice) ordnungsgemäß freigegeben.
    /// </summary>
    static void Main()
    {
        try
        {
            Raylib.InitAudioDevice();
            SoundManager.LoadSounds();
            const int defaultWidth = 1200;
            const int defaultHeight = 900;

            Raylib.InitWindow(defaultWidth, defaultHeight, "Water Wizard");
            Raylib.SetWindowState(ConfigFlags.ResizableWindow);
            Raylib.SetExitKey(KeyboardKey.Escape);

            bool isFullscreen = false;
            int screenWidth = defaultWidth;
            int screenHeight = defaultHeight;

            GameStateManager.Initialize(screenWidth, screenHeight);

            Stopwatch stopWatch = new();
            stopWatch.Start();

            double lastTime = stopWatch.ElapsedMilliseconds;
            double frameTime = 1000d / 60; // 1 Second durch 60 fps = Zeit pro Frame in Ms
            double timeAccumulator = 0;
            // Hauptspiel-Loop
            while (!Raylib.WindowShouldClose())
            {
                double currentTime = stopWatch.ElapsedMilliseconds;
                double elapsedTime = currentTime - lastTime;
                timeAccumulator += elapsedTime;

                if (timeAccumulator >= frameTime)
                {
                    timeAccumulator -= frameTime;

                    if (Raylib.IsWindowResized())
                    {
                        screenWidth = Raylib.GetScreenWidth();
                        screenHeight = Raylib.GetScreenHeight();
                        GameStateManager.Instance.UpdateScreenSize(screenWidth, screenHeight);
                    }

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

                lastTime = currentTime;
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
        finally
        {
            TextureManager.UnloadAllTextures();
            SoundManager.UnloadSounds();
            Raylib.CloseAudioDevice();
        }
    }
}
