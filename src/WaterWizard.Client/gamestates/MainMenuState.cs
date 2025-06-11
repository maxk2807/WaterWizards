using Raylib_cs;

namespace WaterWizard.Client.gamestates;

public class MainMenuState : IGameState
{
    private float titleAnimTime = 0;
    private const float TITLE_ANIM_SPEED = 1.5f;
    private const float TITLE_FLOAT_AMPLITUDE = 10.0f;

    private Texture2D menuBackground; //Hintergrund Variable

    public void LoadAssets()
    {
        if (menuBackground.Id != 0) return;
        menuBackground = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Background/WaterWizardsMenu1200x900.png");
    }

    public void UpdateAndDraw(GameStateManager manager)
    {
        LoadAssets();    
        
        DrawMainMenu(manager);
    }

    private void DrawMainMenu(GameStateManager manager)
    {
        Raylib.DrawTexture(menuBackground, 0, 0, Color.White); //Zeichnen des Hintergrundbildes

        string title = "Welcome to WaterWizards!";
        float letterSpacing = 3;
        float totalTitleWidth = 0;
        for (int i = 0; i < title.Length; i++)
        {
            totalTitleWidth += Raylib.MeasureText(title[i].ToString(), 30) + letterSpacing;
        }
        totalTitleWidth -= letterSpacing;
        titleAnimTime += Raylib.GetFrameTime() * TITLE_ANIM_SPEED;
        float titleVerticalPosition = (float)System.Math.Sin(titleAnimTime) * TITLE_FLOAT_AMPLITUDE;
        float titleX = (manager.screenWidth - totalTitleWidth) / 2;
        int titleY = manager.screenHeight / 4 + (int)titleVerticalPosition;
        for (int i = 0; i < title.Length; i++)
        {
            float hue = (titleAnimTime * 0.3f + i * 0.05f) % 1.0f;
            Color charColor = manager.ColorFromHSV(hue * 360, 0.7f, 0.9f);
            int charWidth = Raylib.MeasureText(title[i].ToString(), 30);
            Raylib.DrawText(title[i].ToString(), (int)titleX, titleY, 30, charColor);
            titleX += charWidth + letterSpacing;
        }
        Rectangle joinButton = new(
            (float)manager.screenWidth / 2 - 100,
            (float)manager.screenHeight / 2,
            200,
            40
        );
        Rectangle hostButton = new(
            (float)manager.screenWidth / 2 - 100,
            (float)manager.screenHeight / 2 + 60,
            200,
            40
        );
        Rectangle mapButton = new(
            (float)manager.screenWidth / 2 - 100,
            (float)manager.screenHeight / 2 + 120,
            200,
            40
        );
        bool hoverJoin = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), joinButton);
        if (hoverJoin && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            manager.SetStateToLobbyList();
        }
        bool hoverHost = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), hostButton);
        if (hoverHost && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            manager.SetStateToHostingMenu();
        }
        bool hoverMap = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), mapButton);
        if (hoverMap && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            manager.SetStateToPlacementPhase();
        }
        Raylib.DrawRectangleRec(joinButton, hoverJoin ? Color.DarkBlue : Color.Blue);
        Raylib.DrawText(
            "Join Lobby",
            (int)joinButton.X + 50,
            (int)joinButton.Y + 10,
            20,
            Color.White
        );
        Raylib.DrawRectangleRec(hostButton, hoverHost ? Color.DarkBlue : Color.Blue);
        Raylib.DrawText(
            "Host Lobby",
            (int)hostButton.X + 50,
            (int)hostButton.Y + 10,
            20,
            Color.White
        );
        Raylib.DrawRectangleRec(mapButton, hoverMap ? Color.DarkBlue : Color.Blue);
        Raylib.DrawText("Map Test", (int)mapButton.X + 50, (int)mapButton.Y + 10, 20, Color.White);
    }
}
