using System.Numerics;
using Raylib_cs;

namespace WaterWizard.Client;

public class GameScreen(GameStateManager gameStateManager, GameBoard playerBoard, GameBoard opponentBoard, int screenWidth, int screenHeight, GameTimer gameTimer)
{
    GameStateManager _gameStateManager = gameStateManager;

    public readonly int cardWidth = (int)Math.Round(screenWidth * 1 / 12f);
    public readonly int cardHeight = (int)Math.Round(screenHeight * 7 / 45f);

    public void Draw()
    {
        if (playerBoard == null || opponentBoard == null)
        {
            Raylib.DrawText("Initializing game boards...", 10, 50, 20, Color.Gray);
            return;
        }

        float zonePadding = screenWidth * 0.02f;
        int boardPixelWidth = playerBoard.GridWidth * playerBoard.CellSize;
        int boardPixelHeight = playerBoard.GridHeight * playerBoard.CellSize;


        // Opponent Hand
        float handWidth = screenWidth * 0.25f;
        float handHeight = screenHeight * 0.15f;
        DrawOpponentHand(zonePadding, handWidth, handHeight);

        // Player Hand 
        DrawPlayerHand(zonePadding, handWidth, handHeight);

        // Graveyard Area 
        float outerBufferWidth = cardWidth * 0.1f;
        float outerBufferHeight = cardHeight * 0.1f;
        float graveyardWidth = cardWidth + outerBufferWidth * 2;
        float graveyardHeight = cardHeight + outerBufferHeight * 2;
        float graveyardX = zonePadding;
        float graveyardY = (screenHeight - graveyardHeight) / 2f;
        DrawGraveyard(graveyardWidth, graveyardHeight, graveyardX, graveyardY, cardWidth, cardHeight, outerBufferWidth, outerBufferHeight);

        // Update and Draw Game Boards
        GameBoard.Point? clickedCell = opponentBoard.Update();
        if (clickedCell.HasValue)
        {
            Console.WriteLine($"Attack initiated at ({clickedCell.Value.X}, {clickedCell.Value.Y})");
            // TODO: Send attack command
        }

        // Draw board titles in boards when not hovering
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

        // Draw Timer 
        float timerX = opponentBoard.Position.X + boardPixelWidth + zonePadding * 2;
        float timerY = screenHeight / 2f;
        gameTimer.Draw((int)timerX, (int)timerY, 20, Color.Red);

        // Draw Back Button
        int backButtonWidth = 100;
        int backButtonHeight = 30;
        Rectangle backButton = new Rectangle(zonePadding, screenHeight - backButtonHeight - zonePadding, backButtonWidth, backButtonHeight);
        bool hoverBack = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);
        Raylib.DrawRectangleRec(backButton, hoverBack ? Color.DarkGray : Color.Gray);
        Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 5, 20, Color.White);

        if (hoverBack && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            NetworkManager.Instance.Shutdown();
            _gameStateManager.SetStateToMainMenu();
        }
    }

    private void DrawPlayerHand(float zonePadding, float handWidth, float handHeight)
    {
        Rectangle playerHandZone = new(screenWidth - handWidth - zonePadding, screenHeight - handHeight - zonePadding, handWidth, handHeight);
        Raylib.DrawRectangleRec(playerHandZone, Color.LightGray);
        Raylib.DrawRectangleLinesEx(playerHandZone, 2, Color.DarkGray);
        Raylib.DrawText("Player Hand", (int)(playerHandZone.X + 10), (int)(playerHandZone.Y + 10), 10, Color.Black);
    }

    private void DrawOpponentHand(float zonePadding, float handWidth, float handHeight)
    {
        Rectangle opponentHandZone = new(screenWidth - handWidth - zonePadding, zonePadding, handWidth, handHeight);
        Raylib.DrawRectangleRec(opponentHandZone, Color.LightGray);
        Raylib.DrawRectangleLinesEx(opponentHandZone, 2, Color.DarkGray);
        Raylib.DrawText("Opponent Hand", (int)(opponentHandZone.X + 10), (int)(opponentHandZone.Y + 10), 10, Color.Black);
    }

    private static void DrawGraveyard(float graveyardWidth, float graveyardHeight, float graveyardX, float graveyardY, int cardWidth, int cardHeight, float outerBufferWidth, float outerBufferHeight)
    {
        Rectangle outerZone = new(graveyardX, graveyardY, graveyardWidth, graveyardHeight);
        Rectangle cardZone = new(graveyardX + outerBufferWidth, graveyardY + outerBufferHeight, cardWidth, cardHeight);
        int lineThickness = 2;
        //Draw underlying "Outer Zone" of Graveyard
        Raylib.DrawRectangleRec(outerZone, Color.DarkGray);
        Raylib.DrawRectangleLinesEx(outerZone, lineThickness, Color.Black);
        //Draw Space for Dead Cards to go on top of
        Raylib.DrawRectangleRec(cardZone, Color.LightGray);
        Raylib.DrawText("Graveyard", (int)outerZone.X + lineThickness, (int)outerZone.Y + lineThickness, 10, Color.White);
    }
}