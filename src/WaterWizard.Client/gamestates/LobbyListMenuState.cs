using Raylib_cs;
using System.Numerics;
using WaterWizard.Client.gamescreen.handler;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamestates;

public class LobbyListMenuState : IGameState
{

    private Texture2D menuBackground;

    public void LoadAssets()
{
    if (menuBackground.Id != 0) return; // Falls bereits geladen, nichts tun

    menuBackground = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Background/WaterWizardsMenu1200x900.png");
}


    public void UpdateAndDraw(GameStateManager manager)
    {
        DrawLobbyListMenu(manager);
    }

    private void DrawLobbyListMenu(GameStateManager manager)
    {
        LoadAssets();

        //Raylib.DrawTexture(menuBackground, 0, 0, Color.White);

        Raylib.DrawTexturePro(
            menuBackground,
            new Rectangle(0, 0, menuBackground.Width, menuBackground.Height),
            new Rectangle(0, 0, manager.screenWidth, manager.screenHeight),
            Vector2.Zero,
            0f,
            Color.White
        );


        int titleWidth = Raylib.MeasureText("Verfügbare Lobbies", 30);
        Raylib.DrawText(
            "Verfügbare Lobbies",
            (manager.screenWidth - titleWidth) / 2,
            manager.screenHeight / 10,
            30,
            Color.DarkBlue
        );
        var lobbies = NetworkManager.Instance.GetDiscoveredLobbies();
        if (lobbies.Count == 0)
        {
            int noLobbiesWidth = Raylib.MeasureText("Suche nach verfügbaren Lobbies...", 20);
            Raylib.DrawText(
                "Suche nach verfügbaren Lobbies...",
                (manager.screenWidth - noLobbiesWidth) / 2,
                manager.screenHeight / 3,
                20,
                Color.DarkGray
            );
        }
        else
        {
            int yPos = manager.screenHeight / 4;
            int headerSpacing = 300;
            int nameWidth = Raylib.MeasureText("Lobby Name", 20);
            int spielerWidth = Raylib.MeasureText("Spieler", 20);
            int tableWidth = nameWidth + headerSpacing + spielerWidth;
            int tableX = (manager.screenWidth - tableWidth) / 2;
            Raylib.DrawText("Lobby Name", tableX, yPos, 20, Color.DarkBlue);
            Raylib.DrawText("Spieler", tableX + headerSpacing, yPos, 20, Color.DarkBlue);
            yPos += 30;
            for (int i = 0; i < lobbies.Count; i++)
            {
                var lobby = lobbies[i];
                Rectangle lobbyRect = new Rectangle(tableX - 20, yPos - 5, 400, 35);
                if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), lobbyRect))
                {
                    Raylib.DrawRectangleRec(lobbyRect, new Color(200, 200, 255, 100));
                    if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                    {
                        string ip = lobby.IP.Split(':')[0];
                        NetworkManager.Instance.ConnectToServer(ip, 7777);
                    }
                }
                Raylib.DrawText(
                    lobby.Name + " " + lobby.IP[0] + lobby.IP[1] + "...",
                    tableX,
                    yPos,
                    18,
                    Color.Black
                );
                Raylib.DrawText(
                    $"{lobby.PlayerCount}",
                    tableX + headerSpacing,
                    yPos,
                    18,
                    Color.Black
                );
                Rectangle joinBtn = new Rectangle(tableX + tableWidth - 100, yPos - 5, 100, 30);
                bool hoverJoin = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), joinBtn);
                Raylib.DrawRectangleRec(joinBtn, hoverJoin ? Color.DarkGreen : Color.Green);
                Raylib.DrawText("Join", (int)joinBtn.X + 35, (int)joinBtn.Y + 5, 18, Color.White);
                if (hoverJoin && Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    string ip = lobby.IP.Split(':')[0];
                    NetworkManager.Instance.ConnectToServer(ip, 7777);
                }
                yPos += 40;
            }
        }
        int margin = 20;
        int buttonHeight = 40;
        int buttonWidth = 120;
        Rectangle backButton = new Rectangle(
            margin,
            manager.screenHeight - buttonHeight - margin,
            buttonWidth,
            buttonHeight
        );
        bool hoverBack = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);
        Raylib.DrawRectangleRec(backButton, hoverBack ? new Color(100, 100, 100, 255) : Color.Gray);
        string backText = "Zurück";
        int backTextWidth = Raylib.MeasureText(backText, 20);
        Raylib.DrawText(
            backText,
            (int)backButton.X + (buttonWidth - backTextWidth) / 2,
            (int)backButton.Y + (buttonHeight - 20) / 2,
            20,
            Color.White
        );
        if (hoverBack && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            NetworkManager.Instance.Shutdown();
            manager.SetStateToMainMenu();
        }
        string refreshText = "Aktualisieren";
        int refreshTextWidth = Raylib.MeasureText(refreshText, 20);
        int refreshButtonWidth = refreshTextWidth + 40;
        Rectangle refreshButton = new Rectangle(
            manager.screenWidth - refreshButtonWidth - margin,
            manager.screenHeight - buttonHeight - margin,
            refreshButtonWidth,
            buttonHeight
        );
        bool hoverRefresh = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), refreshButton);
        Raylib.DrawRectangleRec(
            refreshButton,
            hoverRefresh ? new Color(70, 120, 70, 255) : new Color(60, 160, 60, 255)
        );
        Raylib.DrawText(
            refreshText,
            (int)refreshButton.X + (refreshButtonWidth - refreshTextWidth) / 2,
            (int)refreshButton.Y + (buttonHeight - 20) / 2,
            20,
            Color.White
        );
        if (hoverRefresh && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            LobbyHandler.RefreshLobbies();
        }
        int manualBtnWidth = 300;
        Rectangle manualIpButton = new Rectangle(
            (float)(manager.screenWidth - manualBtnWidth) / 2,
            manager.screenHeight - buttonHeight - margin,
            manualBtnWidth,
            buttonHeight
        );
        bool hoverManualIp = Raylib.CheckCollisionPointRec(
            Raylib.GetMousePosition(),
            manualIpButton
        );
        Raylib.DrawRectangleRec(
            manualIpButton,
            hoverManualIp ? new Color(70, 70, 200, 255) : Color.Blue
        );
        string manualText = "Manuell verbinden";
        int manualTextWidth = Raylib.MeasureText(manualText, 20);
        Raylib.DrawText(
            manualText,
            (int)manualIpButton.X + (manualBtnWidth - manualTextWidth) / 2,
            (int)manualIpButton.Y + (buttonHeight - 20) / 2,
            20,
            Color.White
        );
        if (hoverManualIp && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            manager.SetStateToConnectingMenu();
        }
    }
}
