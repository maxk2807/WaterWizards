// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 69 Zeilen
// - Paul: 60 Zeilen
// - jdewi001: 30 Zeilen
// - justinjd00: 3 Zeilen
// - erick: 3 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.Assets.Sounds.Manager;

namespace WaterWizard.Client.gamestates;

/// <summary>
/// Repräsentiert den Hauptmenü-Zustand des Spiels.
/// </summary>
public class MainMenuState : IGameState
{
    private const float TITLE_ANIM_SPEED = 1.5f;
    private const float TITLE_FLOAT_AMPLITUDE = 10.0f;

    private Texture2D menuBackground; //Hintergrund Variable
    private Texture2D titleAsset; //Hintergrund Variable
    
    // Convert these to instance variables instead of static
    private Texture2D joinButtonAsset;
    private Texture2D hostButtonAsset;
    private Texture2D mapButtonAsset;

    /// <summary>
    /// Lädt die benötigten Assets für das Hauptmenü.
    /// </summary>
    public void LoadAssets()
    {
        if (menuBackground.Id != 0)
            return;
        menuBackground = TextureManager.LoadTexture(
            "Background/WaterWizardsMenu1200x900.png"
        );

        if (titleAsset.Id != 0)
            return;
        titleAsset = TextureManager.LoadTexture(
            "Ui/MainMenu/titleAsset.png"
        );

        // Load button assets lazily
        if (joinButtonAsset.Id == 0)
            joinButtonAsset = TextureManager.LoadTexture(
                "Ui/MainMenu/JoinLobby.png"
            );

        if (hostButtonAsset.Id == 0)
            hostButtonAsset = TextureManager.LoadTexture(
                "Ui/MainMenu/HostLobby.png"
            );

        if (mapButtonAsset.Id == 0)
            mapButtonAsset = TextureManager.LoadTexture(
                "Ui/MainMenu/MapTest.png"
            );
    }

    /// <summary>
    /// Aktualisiert und zeichnet das Hauptmenü.
    /// </summary>
    /// <param name="manager">GameStateManager mit Zugriff auf Komponenten und Status</param>
    public void UpdateAndDraw(GameStateManager manager)
    {
        LoadAssets();

        DrawMainMenu(manager);
    }

    /// <summary>
    /// Zeichnet das Hauptmenü mit Hintergrund, Titelgrafik und ruft die Methoden
    /// zum Rendern und Behandeln der Buttons (Join, Host, Map) auf.
    /// </summary>
    /// <param name="manager">Verwalter für Bildschirmmaße und Zustandswechsel.</param>
    private void DrawMainMenu(GameStateManager manager)
    {
        //Raylib.DrawTexture(menuBackground, 0, 0, Color.White); //Zeichnen des Hintergrundbildes

        Raylib.DrawTexturePro(
            menuBackground,
            new Rectangle(0, 0, menuBackground.Width, menuBackground.Height), // vollständiger Bildausschnitt
            new Rectangle(0, 0, manager.screenWidth, manager.screenHeight), // füllt komplettes Fenster
            Vector2.Zero,
            0f,
            Color.White
        );

        //einbinden des Titels
        float scaleX = (float)manager.screenWidth / titleAsset.Width;
        float scaleY = (float)manager.screenHeight / titleAsset.Height;
        float scaleFactor = Math.Min(scaleX, scaleY);

        // Berechnung der Position
        Rectangle destRect = new(
            (float)manager.screenWidth / 2 - (titleAsset.Width * scaleFactor) / 2, // Zentrierung auf X-Achse
            (float)manager.screenHeight / 9, // Y-Position relativ zum Bildschirm
            titleAsset.Width * scaleFactor,
            titleAsset.Height * scaleFactor
        );

        Raylib.DrawTexturePro(
            titleAsset,
            new Rectangle(0, 0, titleAsset.Width, titleAsset.Height),
            destRect,
            new Vector2(0, 0),
            0.0f,
            Color.White
        );

        HandleJoinButton(manager);
        HandleHostButton(manager);
        HandleMapButton(manager);
    }

    // Update these methods to use instance variables instead of static

    /// <summary>
    /// Zeichnet den Button für den Map-Test und verarbeitet Klicks,
    /// die den Zustand in die Platzierungsphase wechseln.
    /// </summary>
    /// <param name="manager">Verwalter für Zustandswechsel.</param>
    private void HandleMapButton(GameStateManager manager)
    {
        Rectangle mapButton = new(
            (float)manager.screenWidth / 2 - 100,
            (float)manager.screenHeight / 2 + 189,
            220,
            54
        );
        bool hoverMap = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), mapButton);
        if (hoverMap && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            Raylib.PlaySound(SoundManager.ButtonSound);
            manager.SetStateToPlacementPhase();
        }
        Rectangle textureRec = new(0, 0, mapButtonAsset.Width, mapButtonAsset.Height);
        Raylib.DrawTexturePro(mapButtonAsset, textureRec, mapButton, Vector2.Zero, 0f, Color.White);
        Raylib.DrawRectangleRec(mapButton, hoverMap ? new(255, 255, 255, 31) : Color.Blank);
    }

    /// <summary>
    /// Zeichnet den Button zum Erstellen einer Lobby und verarbeitet Klicks,
    /// die den Zustand ins Hosting-Menü wechseln.
    /// </summary>
    /// <param name="manager">Verwalter für Zustandswechsel.</param>
    private void HandleHostButton(GameStateManager manager)
    {
        Rectangle hostButton = new(
            (float)manager.screenWidth / 2 - 100,
            (float)manager.screenHeight / 2 + 92,
            246,
            72
        );
        bool hoverHost = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), hostButton);
        if (hoverHost && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            Raylib.PlaySound(SoundManager.ButtonSound);
            manager.SetStateToHostingMenu();
        }
        Rectangle textureRec = new(0, 0, hostButtonAsset.Width, hostButtonAsset.Height);
        Raylib.DrawTexturePro(
            hostButtonAsset,
            textureRec,
            hostButton,
            Vector2.Zero,
            0f,
            Color.White
        );
        Raylib.DrawRectangleRec(hostButton, hoverHost ? new(255, 255, 255, 31) : Color.Blank);
    }

    /// <summary>
    /// Zeichnet den Button zum Beitreten einer Lobby und verarbeitet Klicks,
    /// die den Zustand zur Lobby-Liste wechseln.
    /// </summary>
    /// <param name="manager">Verwalter für Zustandswechsel.</param>
    private void HandleJoinButton(GameStateManager manager)
    {
        Rectangle joinButton = new(
            (float)manager.screenWidth / 2 - 140,
            (float)manager.screenHeight / 2,
            246,
            72
        );
        bool hoverJoin = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), joinButton);
        if (hoverJoin && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            Raylib.PlaySound(SoundManager.ButtonSound);
            manager.SetStateToLobbyList();
        }
        Rectangle textureRec = new(0, 0, joinButtonAsset.Width, joinButtonAsset.Height);
        Raylib.DrawTexturePro(
            joinButtonAsset,
            textureRec,
            joinButton,
            Vector2.Zero,
            0f,
            Color.White
        );
        Raylib.DrawRectangleRec(joinButton, hoverJoin ? new(255, 255, 255, 31) : Color.Blank);
    }
}
