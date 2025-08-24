// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 120 Zeilen
// - Erickk0: 9 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using Raylib_cs;
using WaterWizard.Client.Assets.Sounds.Manager;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamestates;

/// <summary>
/// Repräsentiert den Game-Over-Zustand. Zeigt je nach Ergebnis (Sieg/Niederlage)
/// einen animierten Titel, eine optionale Gewinnernachricht sowie Buttons zum
/// erneuten Spielen oder Zurückkehren ins Hauptmenü.
/// </summary>
public class GameOverState(bool isWinner, string winnerMessage = "") : IGameState
{
    private readonly string winnerMessage = winnerMessage;
    private readonly bool isWinner = isWinner;
    private float titleAnimTime = 0;
    private const float TITLE_ANIM_SPEED = 2.0f;
    private const float TITLE_FLOAT_AMPLITUDE = 15.0f;

    /// <summary>
    /// Updates the game state and draws the game over screen.
    /// </summary>
    /// <param name="manager">
    /// The <see cref="GameStateManager"/> responsible for managing the current game state and screen dimensions.
    /// </param>
    public void UpdateAndDraw(GameStateManager manager)
    {
        DrawGameOverScreen(manager);
    }

    /// <summary>
    /// Draws the game over screen with victory or defeat message, buttons for replay and main menu.
    /// </summary>
    /// <param name="manager">
    /// The <see cref="GameStateManager"/> responsible for managing the current game state and screen dimensions.
    /// </param>
    private void DrawGameOverScreen(GameStateManager manager)
    {
        Raylib.ClearBackground(Color.DarkBlue);

        string title = isWinner ? "VICTORY!" : "DEFEAT!";
        Color titleColor = isWinner ? Color.Gold : Color.Red;

        titleAnimTime += Raylib.GetFrameTime() * TITLE_ANIM_SPEED;
        float titleVerticalPosition = (float)System.Math.Sin(titleAnimTime) * TITLE_FLOAT_AMPLITUDE;

        int titleFontSize = 60;
        int titleWidth = Raylib.MeasureText(title, titleFontSize);
        int titleX = (manager.screenWidth - titleWidth) / 2;
        int titleY = manager.screenHeight / 3 + (int)titleVerticalPosition;

        Raylib.DrawText(title, titleX, titleY, titleFontSize, titleColor);

        // Winner message
        if (!string.IsNullOrEmpty(winnerMessage))
        {
            int messageWidth = Raylib.MeasureText(winnerMessage, 30);
            Raylib.DrawText(
                winnerMessage,
                (manager.screenWidth - messageWidth) / 2,
                titleY + 80,
                30,
                Color.White
            );
        }

        // Buttons
        int buttonWidth = 200;
        int buttonHeight = 50;
        int buttonSpacing = 20;
        int totalButtonsWidth = buttonWidth * 2 + buttonSpacing;
        int buttonsStartX = (manager.screenWidth - totalButtonsWidth) / 2;
        int buttonsY = manager.screenHeight * 2 / 3;

        // Play Again Button
        Rectangle playAgainButton = new Rectangle(
            buttonsStartX,
            buttonsY,
            buttonWidth,
            buttonHeight
        );
        bool hoverPlayAgain = Raylib.CheckCollisionPointRec(
            Raylib.GetMousePosition(),
            playAgainButton
        );
        Raylib.DrawRectangleRec(playAgainButton, hoverPlayAgain ? Color.DarkGreen : Color.Green);

        string playAgainText = "Play Again";
        int playAgainTextWidth = Raylib.MeasureText(playAgainText, 20);
        Raylib.DrawText(
            playAgainText,
            (int)playAgainButton.X + (buttonWidth - playAgainTextWidth) / 2,
            (int)playAgainButton.Y + (buttonHeight - 20) / 2,
            20,
            Color.White
        );

        // Main Menu Button
        Rectangle mainMenuButton = new Rectangle(
            buttonsStartX + buttonWidth + buttonSpacing,
            buttonsY,
            buttonWidth,
            buttonHeight
        );
        bool hoverMainMenu = Raylib.CheckCollisionPointRec(
            Raylib.GetMousePosition(),
            mainMenuButton
        );
        Raylib.DrawRectangleRec(mainMenuButton, hoverMainMenu ? Color.DarkBlue : Color.Blue);

        string mainMenuText = "Main Menu";
        int mainMenuTextWidth = Raylib.MeasureText(mainMenuText, 20);
        Raylib.DrawText(
            mainMenuText,
            (int)mainMenuButton.X + (buttonWidth - mainMenuTextWidth) / 2,
            (int)mainMenuButton.Y + (buttonHeight - 20) / 2,
            20,
            Color.White
        );

        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            Raylib.PlaySound(SoundManager.ButtonSound);
            if (hoverPlayAgain)
            {
                manager.ResetGame();
                manager.SetStateToLobby();
            }
            else if (hoverMainMenu)
            {
                NetworkManager.Instance.Shutdown();
                manager.SetStateToMainMenu();
            }
        }
    }
}
