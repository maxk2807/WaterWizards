using System.Numerics;
using Raylib_cs;

namespace WaterWizard.Client;

public class GameScreen(GameStateManager gameStateManager, GameBoard? playerBoard, GameBoard? opponentBoard, GameTimer gameTimer)
{
    readonly GameStateManager _gameStateManager = gameStateManager;

    public void Draw(int currentScreenWidth, int currentScreenHeight)
    {
        if (playerBoard == null || opponentBoard == null)
        {
            Raylib.DrawText("Initializing game boards...", 10, 50, 20, Color.Gray);
            return;
        }

        // Calculate dynamic layout values inside Draw
        int cardWidth = (int)Math.Round(currentScreenWidth * (1 / 12f)); 
        int cardHeight = (int)Math.Round(currentScreenHeight * (7 / 45f)); 
        float zonePadding = currentScreenWidth * 0.02f;
        float zonePadding1 = currentScreenHeight * 0.02f; 
        int boardPixelWidth = playerBoard.GridWidth * playerBoard.CellSize;
        int boardPixelHeight = playerBoard.GridHeight * playerBoard.CellSize;

        // Opponent Hand
        float handWidth = currentScreenWidth * 0.25f;
        float handHeight = currentScreenHeight * 0.15f;
        DrawOpponentHand(zonePadding, handWidth, handHeight, currentScreenWidth, currentScreenHeight);

        // Player Hand
        DrawPlayerHand(zonePadding, handWidth, handHeight, currentScreenWidth, currentScreenHeight);
        
        // Draw Timer
        float timerX = zonePadding;
        float timerY = (currentScreenHeight / 2f) - 10; 
        gameTimer.Draw((int)timerX, (int)timerY, 20, Color.Red);
        

        // Graveyard Area - Use current dimensions for calculation
        float outerBufferWidth = cardWidth * 0.1f; 
        float outerBufferHeight = cardHeight * 0.1f; 
        float graveyardWidth = cardWidth + outerBufferWidth * 2; 
        float graveyardHeight = cardHeight + outerBufferHeight * 2; 
        float graveyardX = zonePadding * 2 + GameTimer.MaxTextWidth(20);
        float graveyardY = (currentScreenHeight - graveyardHeight) / 2f;
        DrawGraveyard(graveyardWidth, graveyardHeight, graveyardX, graveyardY, cardWidth, cardHeight, outerBufferWidth, outerBufferHeight);

        // Update and Draw Game Boards
        GameBoard.Point? clickedCell = opponentBoard.Update();
        if (clickedCell.HasValue)
        {
            Console.WriteLine($"Attack initiated at ({clickedCell.Value.X}, {clickedCell.Value.Y})");
            // TODO: Send attack command
        }

        // Draw board titles (rest of the code is mostly the same as previous fix)
        opponentBoard.Draw();
        Rectangle opponentBoardRectangle = new(opponentBoard.Position, boardPixelWidth, boardPixelHeight);
        bool hoverOpponentBoard = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), opponentBoardRectangle);
        if (!hoverOpponentBoard)
        {
            int opponentTitleWidth = Raylib.MeasureText("Opponent's Board", 15);
            Raylib.DrawText("Opponent's Board", (int)opponentBoard.Position.X + (boardPixelWidth - opponentTitleWidth) / 2, (int)opponentBoard.Position.Y + 15, 15, Color.Black);
        }

        playerBoard.Draw();
        Rectangle playerBoardRectangle = new(playerBoard.Position, boardPixelWidth, boardPixelHeight);
        bool hoverPlayerBoard = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), playerBoardRectangle);
        if (!hoverPlayerBoard)
        {
            int playerTitleWidth = Raylib.MeasureText("Your Board", 15);
            Raylib.DrawText("Your Board", (int)playerBoard.Position.X + (boardPixelWidth - playerTitleWidth) / 2, (int)playerBoard.Position.Y + 15, 15, Color.Black);
        }

        // Draw Back Button
        int backButtonWidth = 100;
        int backButtonHeight = 30;
        Rectangle backButton = new Rectangle(zonePadding, currentScreenHeight - backButtonHeight - zonePadding, backButtonWidth, backButtonHeight);
        bool hoverBack = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);
        Raylib.DrawRectangleRec(backButton, hoverBack ? Color.DarkGray : Color.Gray);
        Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 5, 20, Color.White);

        if (hoverBack && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            NetworkManager.Instance.Shutdown();
            _gameStateManager.SetStateToMainMenu();
        }
    }

    // Update helper methods to accept screen dimensions
    private void DrawPlayerHand(float zonePadding, float handWidth, float handHeight, int currentScreenWidth, int currentScreenHeight)
    {
        Rectangle playerHandZone = new(currentScreenWidth - handWidth - zonePadding, currentScreenHeight - handHeight - zonePadding, handWidth, handHeight);
        Raylib.DrawRectangleRec(playerHandZone, Color.LightGray);
        Raylib.DrawRectangleLinesEx(playerHandZone, 2, Color.DarkGray);
        Raylib.DrawText("Player Hand", (int)(playerHandZone.X + 10), (int)(playerHandZone.Y + 10), 10, Color.Black);
    }

    private void DrawOpponentHand(float zonePadding, float handWidth, float handHeight, int currentScreenWidth, int currentScreenHeight)
    {
        Rectangle opponentHandZone = new(currentScreenWidth - handWidth - zonePadding, zonePadding, handWidth, handHeight);
        Raylib.DrawRectangleRec(opponentHandZone, Color.LightGray);
        Raylib.DrawRectangleLinesEx(opponentHandZone, 2, Color.DarkGray);
        Raylib.DrawText("Opponent Hand", (int)(opponentHandZone.X + 10), (int)(opponentHandZone.Y + 10), 10, Color.Black);
    }

    // Update DrawGraveyard signature to accept local cardWidth/Height
    private static void DrawGraveyard(float graveyardWidth, float graveyardHeight, float graveyardX, float graveyardY, int cardWidth, int cardHeight, float outerBufferWidth, float outerBufferHeight)
    {
        Rectangle outerZone = new(graveyardX, graveyardY, graveyardWidth, graveyardHeight);
        // Use passed-in cardWidth/Height
        Rectangle cardZone = new(graveyardX + outerBufferWidth, graveyardY + outerBufferHeight, cardWidth, cardHeight);
        int lineThickness = 2;
        Raylib.DrawRectangleRec(outerZone, Color.DarkGray);
        Raylib.DrawRectangleLinesEx(outerZone, 2, Color.Black);
        Raylib.DrawRectangleRec(cardZone, Color.LightGray);
        Raylib.DrawText("Graveyard", (int)outerZone.X + lineThickness, (int)outerZone.Y + lineThickness, 10, Color.White);
    }
}