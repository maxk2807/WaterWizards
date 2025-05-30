using Raylib_cs;

namespace WaterWizard.Client.gamestates;

public class InGameState : IGameState
{
    public void UpdateAndDraw(GameStateManager manager)
    {       
        if (manager.GetGamePauseManager().IsGamePaused)
        {
            Raylib.DrawRectangle(
                0,
                0,
                manager.screenWidth,
                manager.screenHeight,
                new Color(0, 0, 0, 128)
            );
            string pauseText = "PAUSED";
            int textWidth = Raylib.MeasureText(pauseText, 40);
            Raylib.DrawText(
                pauseText,
                (manager.screenWidth - textWidth) / 2,
                manager.screenHeight / 2 - 20,
                40,
                Color.Yellow
            );
        }
        else
        {
            DrawGameScreen(manager);
        }
    }

    private static void DrawGameScreen(GameStateManager manager)
    {
        manager.GameScreen.Draw(manager.screenWidth, manager.screenHeight);
    }
}
