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
        Raylib.DrawText("Enter IP Address:", manager.screenWidth / 3, manager.screenHeight / 2 - 40, 20, Color.DarkBlue);
        Rectangle inputBox = new((float)manager.screenWidth / 3, (float)manager.screenHeight / 2, 250, 40); 
        Raylib.DrawRectangleRec(inputBox, manager.IsEditingIp() ? Color.White : Color.LightGray);
        Raylib.DrawRectangleLinesEx(inputBox, 1, Color.DarkBlue);
        Raylib.DrawText(manager.GetInputText(), (int)inputBox.X + 5, (int)inputBox.Y + 10, 20, Color.Black);

        int portInputX = (int)inputBox.X + (int)inputBox.Width + 20; 
        Raylib.DrawText("Enter Port:", portInputX, manager.screenHeight / 2 - 40, 20, Color.DarkBlue);
        Rectangle portInputBox = new(portInputX, (float)manager.screenHeight / 2, 100, 40);
        Raylib.DrawRectangleRec(portInputBox, manager.IsEditingPort() ? Color.White : Color.LightGray); 
        Raylib.DrawRectangleLinesEx(portInputBox, 1, Color.DarkBlue);
        Raylib.DrawText(manager.GetInputPortText(), (int)portInputBox.X + 5, (int)portInputBox.Y + 10, 20, Color.Black); 

        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), inputBox))
            {
                manager.SetEditingIp(true);
                manager.SetEditingPort(false); 
            }
            else if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), portInputBox))
            {
                manager.SetEditingPort(true);
                manager.SetEditingIp(false); 
            }
            else
            {
                manager.SetEditingIp(false);
                manager.SetEditingPort(false);
            }
        }

        if (manager.IsEditingIp() || manager.IsEditingPort()) 
        {
            manager.HandleTextInput();
        }

        Rectangle connectButton = new Rectangle(manager.screenWidth / 2f - 80, manager.screenHeight / 2f + 60, 160, 40);
        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), connectButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            if (int.TryParse(manager.GetInputPortText(), out int port))
            {
                NetworkManager.Instance.ConnectToServer(manager.GetInputText(), port);
            }
            else
            {
                NetworkManager.Instance.ConnectToServer(manager.GetInputText(), 7777); 
                Console.WriteLine("Invalid port, using default 7777"); 
            }
        }
        Raylib.DrawRectangleRec(connectButton, Color.Blue);
        Raylib.DrawText("Connect", (int)connectButton.X + (160 - Raylib.MeasureText("Connect", 20)) / 2, (int)connectButton.Y + 10, 20, Color.White);

        Rectangle backButton = new(manager.screenWidth / 2f - 80, manager.screenHeight / 2f + 120, 160, 40);
        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            manager.SetStateToMainMenu();
            manager.SetEditingIp(false);
            manager.SetEditingPort(false); 
        }
        Raylib.DrawRectangleRec(backButton, Color.Gray);
        Raylib.DrawText("Back", (int)backButton.X + (160 - Raylib.MeasureText("Back", 20)) / 2, (int)backButton.Y + 10, 20, Color.White);
    }
}