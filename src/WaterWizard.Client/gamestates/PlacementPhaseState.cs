using Raylib_cs;

namespace WaterWizard.Client.gamestates;

public class PlacementPhaseState : IGameState
{
    public void UpdateAndDraw(GameStateManager manager)
    {
        manager.GameScreen.Draw(manager.screenWidth, manager.screenHeight);
        string infoText = "Platzierungsphase: Platziere alle Schiffe!";
        int textWidth = Raylib.MeasureText(infoText, 30);
        Raylib.DrawText(infoText, (manager.screenWidth - textWidth) / 2, manager.screenHeight / 2 - 20, 30, Color.Red);
        if (Raylib.IsKeyPressed(KeyboardKey.L))
        {
            manager.SetStateToInGame();
        }
    }
}