using System.Numerics;
using Raylib_cs;

namespace WaterWizard.Client;

public class GameScreen(GameStateManager gameStateManager, int screenWidth, int screenHeight, GameTimer gameTimer)
{
    public readonly GameStateManager _gameStateManager = gameStateManager;

    public GameBoard? playerBoard, opponentBoard;
    public GameHand? playerHand, opponentHand;

    public int cardWidth;
    public int cardHeight;
    public float ZonePadding; 

    /// <summary>
    /// Initialize all elements rendered on the GameScreen:
    ///  the two Boards, Cardhands and Cardstacks as well as the //TODO: Graveyard and GameTimer
    /// </summary>
    public void Initialize()
    {
        cardWidth = (int)Math.Round(screenWidth * (1 / 12f));
        cardHeight = (int)Math.Round(screenHeight * (7 / 45f));
        ZonePadding = screenWidth * 0.02f;
        InitializeHands();
        InitializeBoards();
    }

    /// <summary>
    /// Initializes the Hands of Cards of both the Player and the opponent, 
    /// assigning the central position that the GameHand will be rendered around.
    /// </summary>
    private void InitializeHands()
    {
        float offsetX = screenWidth * 0.143f;

        int centralPlayerX = (int)(screenWidth - ZonePadding - offsetX);
        int centralPlayerY = (int)(screenHeight - ZonePadding - cardHeight);
        int centralOpponentX = (int)(screenWidth - ZonePadding - offsetX);
        int centralOpponentY = (int)ZonePadding;

        playerHand = new(this, centralPlayerX, centralPlayerY);
        opponentHand = new(this, centralOpponentX, centralOpponentY);
    }

    /// <summary>
    /// Initialize the Boards, defines variables for positioning the boards on the Screen.
    /// </summary>
    private void InitializeBoards()
    {
        //12x10 Cells Board
        int boardWidth = 12;
        int boardHeight = 10;
        //Ratio between width and height of the cell
        float boardRatio = boardWidth / (float)boardHeight;
        int boardPixelHeight = (int)Math.Round(screenHeight * 0.495f);
        //Ratio used to calculate width of board
        int boardPixelWidth = (int)Math.Round(boardRatio * boardPixelHeight);

        //dynamic Pixels per Cell based on Screensize
        int cellSize = boardPixelHeight / boardHeight;

        int opponentBoardY = 0;
        //position boards in the center of the screen
        int opponentBoardX = (screenWidth - boardPixelWidth) / 2;
        Vector2 opponentBoardPos = new(opponentBoardX, opponentBoardY);

        int playerBoardY = screenHeight - boardPixelHeight;
        int playerBoardX = opponentBoardX;
        Vector2 playerBoardPos = new(playerBoardX, playerBoardY);

        if (playerBoard == null || opponentBoard == null)
        {
            playerBoard = new GameBoard(boardWidth, boardHeight, cellSize, playerBoardPos);
            opponentBoard = new GameBoard(boardWidth, boardHeight, cellSize, opponentBoardPos);
        }
        else
        {
            playerBoard.Position = playerBoardPos;
            playerBoard.CellSize = cellSize;
            playerBoard.GridWidth = boardWidth;
            playerBoard.GridHeight = boardHeight;
            opponentBoard.Position = opponentBoardPos;
            opponentBoard.CellSize = cellSize;
            opponentBoard.GridWidth = boardWidth;
            opponentBoard.GridHeight = boardHeight;
        }
    }

    /// <summary>
    /// Draws the Boards, Hands (of cards), timer and graveyard based on the size of the screen.
    /// Also Draws other elements for navigation and handles inputs for these and for a rudimentary attack. 
    /// </summary>
    /// <param name="currentScreenWidth"></param>
    /// <param name="currentScreenHeight"></param>
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
        ZonePadding = currentScreenWidth * 0.02f;
        int boardPixelWidth = playerBoard.GridWidth * playerBoard.CellSize;
        int boardPixelHeight = playerBoard.GridHeight * playerBoard.CellSize;

        // Opponent Hand
        DrawHand(false);

        // Player Hand
        DrawHand(true);

        // Draw Timer
        float timerX = ZonePadding;
        float timerY = (currentScreenHeight / 2f) - 10;
        gameTimer.Draw((int)timerX, (int)timerY, 20, Color.Red);


        // Graveyard Area - Use current dimensions for calculation
        float outerBufferWidth = cardWidth * 0.1f;
        float outerBufferHeight = cardHeight * 0.1f;
        float graveyardWidth = cardWidth + outerBufferWidth * 2;
        float graveyardHeight = cardHeight + outerBufferHeight * 2;
        float graveyardX = ZonePadding * 2 + GameTimer.MaxTextWidth(20);
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
        Rectangle backButton = new Rectangle(ZonePadding, currentScreenHeight - backButtonHeight - ZonePadding, backButtonWidth, backButtonHeight);
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
    private void DrawHand(bool playerHand)
    {
        if (playerHand)
        {
            this.playerHand?.Draw(false);
        }
        else
        {
            opponentHand?.Draw(true);
        }
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

    public void UpdateScreenSize(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;
        Initialize();
    }

    public void Reset()
    {
        if (playerBoard is null || opponentBoard is null)
        {
            InitializeBoards();
        }
        else
        {
            playerBoard = new(playerBoard.GridWidth, playerBoard.GridHeight, playerBoard.CellSize, playerBoard.Position);
            opponentBoard = new(opponentBoard.GridWidth, opponentBoard.GridHeight, opponentBoard.CellSize, opponentBoard.Position);
        }
        InitializeHands();
    }
}