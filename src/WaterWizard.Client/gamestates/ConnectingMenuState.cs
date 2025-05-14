using Raylib_cs;

namespace WaterWizard.Client.gamestates;

public class ConnectingMenuState : IGameState
{
    public void UpdateAndDraw(GameStateManager manager)
    {
        DrawConnectMenu(manager);
    }

    private void DrawConnectMenu(GameStateManager manager)
    {
        Raylib.DrawText("Enter IP Address to Connect:", manager.screenWidth / 3, manager.screenHeight / 3, 20, Color.DarkBlue);
        Rectangle inputBox = new((float)manager.screenWidth / 3, (float)manager.screenHeight / 2, 300, 40);
        Raylib.DrawRectangleRec(inputBox, manager.IsEditingIp() ? Color.White : Color.LightGray);
        Raylib.DrawRectangleLines((int)inputBox.X, (int)inputBox.Y, (int)inputBox.Width, (int)inputBox.Height, Color.DarkBlue);
        Raylib.DrawText(manager.GetInputText(), (int)inputBox.X + 5, (int)inputBox.Y + 10, 20, Color.Black);
        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), inputBox) && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            manager.SetEditingIp(true);
        }
        if (manager.IsEditingIp())
        {
            manager.HandleTextInput();
        }
        Rectangle connectButton = new Rectangle((float)manager.screenWidth / 2 - 80, (float)manager.screenHeight / 2 + 60, 160, 40);
        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), connectButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            NetworkManager.Instance.ConnectToServer(manager.GetInputText(), 7777);
        }
        Raylib.DrawRectangleRec(connectButton, Color.Blue);
        Raylib.DrawText("Connect", (int)connectButton.X + 40, (int)connectButton.Y + 10, 20, Color.White);
        Rectangle backButton = new((float)manager.screenWidth / 3, (float)manager.screenHeight / 2 + 120, 100, 40);
        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            manager.SetStateToMainMenu();
            manager.SetEditingIp(false);
        }
        Raylib.DrawRectangleRec(backButton, Color.Gray);
        Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 10, 20, Color.White);
    }
}