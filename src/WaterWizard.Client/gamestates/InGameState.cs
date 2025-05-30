using Raylib_cs;

namespace WaterWizard.Client.gamestates;

public class InGameState : IGameState
{
    /// <summary>
    /// Updates the game state and draws the game screen or pause overlay based on the game pause status.
    /// </summary>
    /// <param name="manager">The GameStateManager instance that provides access to game components, screen dimensions, pause state, and rendering systems.</param>
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

    /// <summary>
    /// Draws the game screen, including the game state and any relevant UI elements.
    /// </summary>
    /// <param name="manager">The GameStateManager instance that provides access to game components, screen dimensions, pause state, and rendering systems.</param>
    private static void DrawGameScreen(GameStateManager manager)
    {
        manager.GameScreen.Draw(manager.screenWidth, manager.screenHeight);
    }
}
