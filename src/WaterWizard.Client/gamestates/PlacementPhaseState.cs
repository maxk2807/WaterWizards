using Raylib_cs;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamestates;

public class PlacementPhaseState : IGameState
{
    public void UpdateAndDraw(GameStateManager manager)
    {
        manager.GameScreen.Draw(manager.screenWidth, manager.screenHeight);
        string infoText = "Platzierungsphase: Platziere alle Schiffe!";
        int textWidth = Raylib.MeasureText(infoText, 30);
        int textY = (int)(manager.screenHeight * 0.036f);
        Raylib.DrawText(infoText, (manager.screenWidth - textWidth) / 2, textY, 30, Color.Red);
        
        int buttonWidth = 200;
        int buttonHeight = 50;
        int buttonX = (manager.screenWidth - buttonWidth) / 2;
        int buttonY = textY + (int)(manager.screenHeight * 0.15f);
        Rectangle readyButton = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
        bool hoverReady = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), readyButton);
        Raylib.DrawRectangleRec(readyButton, hoverReady ? Color.DarkGreen : Color.Green);
        string readyText = "Fertig";
        int readyTextWidth = Raylib.MeasureText(readyText, 24);
        Raylib.DrawText(readyText, buttonX + (buttonWidth - readyTextWidth) / 2, buttonY + (buttonHeight - 24) / 2, 24, Color.White);
        if (hoverReady && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            NetworkManager.Instance.SendPlacementReady();
            //manager.SetStateToInGame();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.L))
        {
            manager.SetStateToInGame();
        }
    }
}