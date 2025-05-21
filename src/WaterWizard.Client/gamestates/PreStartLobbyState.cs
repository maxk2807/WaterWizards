using Raylib_cs;
using WaterWizard.Client.gamescreen;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamestates;

public class PreStartLobbyState : IGameState
{
    public void UpdateAndDraw(GameStateManager manager)
    {
        DrawPreStartLobby(manager);
    }

    private void DrawPreStartLobby(GameStateManager manager)
    {
        manager.ChatLog.Draw(manager.screenWidth, manager.screenHeight);
        float availableWidth = manager.screenWidth - (manager.screenWidth * 0.3f + 40);
        int titleWidth = Raylib.MeasureText("Waiting for players...", 30);
        Raylib.DrawText("Waiting for players...", (int)(availableWidth - titleWidth) / 2, manager.screenHeight / 10, 30, Color.DarkBlue);
        var players = NetworkManager.Instance.GetConnectedPlayers();
        string playerCountText = $"Connected Players: {players.Count}";
        int playerCountWidth = Raylib.MeasureText(playerCountText, 20);
        Raylib.DrawText(playerCountText, (int)(availableWidth - playerCountWidth) / 2, manager.screenHeight / 4, 20, Color.DarkGreen);
        int playerListY = manager.screenHeight / 4 + 40;
        int maxListHeight = manager.screenHeight / 2 - 80;
        for (int i = 0; i < players.Count; i++)
        {
            string status = players[i].IsReady ? "(Ready)" : "(Not Ready)";
            string playerText = $"{players[i].Name} {status}";
            int textWidth = Raylib.MeasureText(playerText, 18);
            if (playerListY + i * 30 < playerListY + maxListHeight)
            {
                Raylib.DrawText(playerText, (int)(availableWidth - textWidth) / 2, playerListY + i * 30, 18, Color.Black);
            }
        }
        int actionButtonY = manager.screenHeight * 2 / 3;
        int buttonWidth = 200;
        int buttonHeight = 50;
        int buttonX = (int)(availableWidth - buttonWidth) / 2;
        bool isHost = NetworkManager.Instance.IsHost();
        PreStartLobbyTimer.DrawCountdown(manager);
        if (isHost)
        {
            Rectangle startButton = new Rectangle(buttonX, actionButtonY, buttonWidth, buttonHeight);
            bool hoverStart = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), startButton);
            bool allReady = players.Count > 0 && players.All(p => p.IsReady);
            Color startBtnColor = allReady ? (hoverStart ? Color.DarkGreen : Color.Green) : Color.Gray;
            Raylib.DrawRectangleRec(startButton, startBtnColor);
            string startText = "Start Game";
            int textWidth = Raylib.MeasureText(startText, 20);
            Raylib.DrawText(startText, (int)startButton.X + (buttonWidth - textWidth) / 2, (int)startButton.Y + (buttonHeight - 20) / 2, 20, Color.White);
            if (allReady && hoverStart && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.BroadcastStartGame();
            }
        }
        else
        {
            bool isReady = NetworkManager.Instance.IsClientReady();
            Rectangle readyButton = new Rectangle(buttonX, actionButtonY, buttonWidth, buttonHeight);
            bool hoverReady = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), readyButton);
            Color readyBtnColor = isReady ? (hoverReady ? Color.DarkGreen : Color.Green) : (hoverReady ? Color.DarkGray : Color.Gray);
            Raylib.DrawRectangleRec(readyButton, readyBtnColor);
            string readyText = isReady ? "Ready!" : "Get Ready";
            int textWidth = Raylib.MeasureText(readyText, 20);
            Raylib.DrawText(readyText, (int)readyButton.X + (buttonWidth - textWidth) / 2, (int)readyButton.Y + (buttonHeight - 20) / 2, 20, Color.White);
            if (hoverReady && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.ToggleReadyStatus();
            }
        }
        int backButtonWidth = 120;
        int backButtonHeight = 40;
        int backButtonMargin = 20;
        Rectangle backButton = new Rectangle(backButtonMargin, manager.screenHeight - backButtonHeight - backButtonMargin, backButtonWidth, backButtonHeight);
        bool hoverBack = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);
        Raylib.DrawRectangleRec(backButton, hoverBack ? new Color(100, 100, 100, 255) : Color.Gray);
        string backText = "Disconnect";
        int backTextWidth = Raylib.MeasureText(backText, 20);
        Raylib.DrawText(backText, (int)backButton.X + (backButtonWidth - backTextWidth) / 2, (int)backButton.Y + (backButtonHeight - 20) / 2, 20, Color.White);
        if (hoverBack && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            NetworkManager.Instance.Shutdown();
            manager.SetStateToMainMenu();
        }
    }

    // Called when the client joins a server and receives the EnterLobby message
    // This ensures we always switch to the PreStartLobbyState after joining
    public static void SwitchToPreStartLobby()
    {
        GameStateManager.Instance.SetStateToLobby();
    }
}