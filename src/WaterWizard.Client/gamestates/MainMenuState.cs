using Raylib_cs;
using System.Numerics;


namespace WaterWizard.Client.gamestates;

public class MainMenuState : IGameState
{
    private const float TITLE_ANIM_SPEED = 1.5f;
    private const float TITLE_FLOAT_AMPLITUDE = 10.0f;

    private Texture2D menuBackground; //Hintergrund Variable
    private Texture2D titleAsset; //Hintergrund Variable
    private static Texture2D joinButtonAsset = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ui/MainMenu/JoinLobby.png");
    private static Texture2D hostButtonAsset = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ui/MainMenu/HostLobby.png");
    private static Texture2D mapButtonAsset = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ui/MainMenu/MapTest.png");

    public void LoadAssets()
    {
        if (menuBackground.Id != 0) return;
        menuBackground = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Background/WaterWizardsMenu1200x900.png");

        if (titleAsset.Id != 0) return;
        titleAsset = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Ui/MainMenu/titleAsset.png");
    }

    public void UpdateAndDraw(GameStateManager manager)
    {
        LoadAssets();

        DrawMainMenu(manager);
    }

    private void DrawMainMenu(GameStateManager manager)
    {
        //Raylib.DrawTexture(menuBackground, 0, 0, Color.White); //Zeichnen des Hintergrundbildes

        Raylib.DrawTexturePro(
            menuBackground,
            new Rectangle(0, 0, menuBackground.Width, menuBackground.Height),  // vollständiger Bildausschnitt
            new Rectangle(0, 0, manager.screenWidth, manager.screenHeight),     // füllt komplettes Fenster
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



        Raylib.DrawTexturePro(titleAsset, new Rectangle(0, 0, titleAsset.Width, titleAsset.Height), destRect, new Vector2(0, 0), 0.0f, Color.White);


        // string title = "Welcome to WaterWizards!";
        // float letterSpacing = 3;
        // float totalTitleWidth = 0;
        // for (int i = 0; i < title.Length; i++)
        // {
        //     totalTitleWidth += Raylib.MeasureText(title[i].ToString(), 30) + letterSpacing;
        // }
        // totalTitleWidth -= letterSpacing;
        // titleAnimTime += Raylib.GetFrameTime() * TITLE_ANIM_SPEED;
        // float titleVerticalPosition = (float)System.Math.Sin(titleAnimTime) * TITLE_FLOAT_AMPLITUDE;
        // float titleX = (manager.screenWidth - totalTitleWidth) / 2;
        // int titleY = manager.screenHeight / 4 + (int)titleVerticalPosition;
        // for (int i = 0; i < title.Length; i++)
        // {
        //     float hue = (titleAnimTime * 0.3f + i * 0.05f) % 1.0f;
        //     Color charColor = manager.ColorFromHSV(hue * 360, 0.7f, 0.9f);
        //     int charWidth = Raylib.MeasureText(title[i].ToString(), 30);
        //     Raylib.DrawText(title[i].ToString(), (int)titleX, titleY, 30, charColor);
        //     titleX += charWidth + letterSpacing;
        // }

        HandleJoinButton(manager);
        HandleHostButton(manager);
        HandleMapButton(manager);
    }

    private static void HandleMapButton(GameStateManager manager)
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
            manager.SetStateToPlacementPhase();
        }
        Rectangle textureRec = new(0, 0, mapButtonAsset.Width, mapButtonAsset.Height);
        Raylib.DrawTexturePro(
            mapButtonAsset,
            textureRec,
            mapButton,
            Vector2.Zero,
            0f,
            Color.White
        );
        Raylib.DrawRectangleRec(mapButton, hoverMap ? new(255, 255, 255, 31) : Color.Blank);
    }

    private static void HandleHostButton(GameStateManager manager)
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

    private static void HandleJoinButton(GameStateManager manager)
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
