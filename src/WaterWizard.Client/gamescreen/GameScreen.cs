using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.gamescreen.cards;
using WaterWizard.Client.gamescreen.ships;
using WaterWizard.Client.network;
using WaterWizard.Shared;

namespace WaterWizard.Client.gamescreen;

public class GameScreen(
    GameStateManager gameStateManager,
    int screenWidth,
    int screenHeight,
    GameTimer gameTimer
)
{
    public readonly GameStateManager _gameStateManager = gameStateManager;

    public GameBoard? playerBoard,
        opponentBoard;
    public GameHand? playerHand,
        opponentHand;
    public ActiveCards? activeCards;
    public CardStacksField? cardStacksField;
    public ShipField? shipField;
    public RessourceField? ressourceField;

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
        InitializeActiveCards();
        InitializeCardStacksField();
        InitializeShipField();
        InitializeRessourceField();
    }

    private void InitializeRessourceField()
    {
        ressourceField = new(this);
        ressourceField.Initialize();
    }

    private void InitializeShipField()
    {
        shipField = new(this);
        shipField.Initialize();
    }

    private HashSet<int> shipSizeLimitReached = new();

    public void MarkShipSizeLimitReached(int size)
    {
        shipSizeLimitReached.Add(size);
    }

    public bool IsShipSizeLimitReached(int size)
    {
        return shipSizeLimitReached.Contains(size);
    }

    private void InitializeCardStacksField()
    {
        cardStacksField = new(this);
        cardStacksField.Initialize();
    }

    /// <summary>
    /// Initializes the Hands of Cards of both the Player and the opponent,
    /// assigning the central position that the GameHand will be rendered around.
    /// </summary>
    private void InitializeHands()
    {
        float offsetX = screenWidth * 0.143f;

        int centralPlayerX = (int)(screenWidth - ZonePadding - offsetX);
        int playerCardY = (int)(screenHeight - ZonePadding * 2 - cardHeight);
        int centralOpponentX = (int)(screenWidth - ZonePadding - offsetX);
        int opponentCardY = (int)ZonePadding * 2;

        playerHand = new(this, centralPlayerX, playerCardY);
        opponentHand = new(this, centralOpponentX, opponentCardY);
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

    public void InitializeActiveCards()
    {
        activeCards = new(this);
        activeCards.Initialize();
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
        cardWidth = (int)Math.Round(currentScreenWidth * (1 / 12f));
        cardHeight = (int)Math.Round(currentScreenHeight * (7 / 45f));
        ZonePadding = currentScreenWidth * 0.02f;
        int boardPixelWidth = playerBoard.GridWidth * playerBoard.CellSize;
        int boardPixelHeight = playerBoard.GridHeight * playerBoard.CellSize;

        // Draw the (Card) Hands of both opponent and player
        DrawHands();

        // Draw Timer
        float timerX = ZonePadding;
        float timerY = ZonePadding;
        gameTimer.Draw((int)timerX, (int)timerY, 20, Color.Red);

        // Graveyard Area - Use current dimensions for calculation
        float outerBufferWidth = cardWidth * 0.1f;
        float outerBufferHeight = cardHeight * 0.1f;
        float graveyardWidth = cardWidth + outerBufferWidth * 2;
        float graveyardHeight = cardHeight + outerBufferHeight * 2;
        float graveyardX = playerBoard.Position.X - graveyardWidth - ZonePadding;
        float graveyardY = (currentScreenHeight - graveyardHeight) / 2f;
        DrawGraveyard(
            graveyardWidth,
            graveyardHeight,
            graveyardX,
            graveyardY,
            cardWidth,
            cardHeight,
            outerBufferWidth,
            outerBufferHeight
        );

        DrawActiveCards();

        DrawCardStacksField();

        DrawRessourceField();

        // Update and Draw Game Boards
        GameBoard.Point? clickedCell = opponentBoard.Update();
        if (clickedCell.HasValue)
        {
            NetworkManager.SendAttack(clickedCell.Value.X, clickedCell.Value.Y);
            Console.WriteLine(
                $"Attack initiated at ({clickedCell.Value.X}, {clickedCell.Value.Y})"
            );
            // TODO: Send attack command
        }

        // Draw board titles (rest of the code is mostly the same as previous fix)
        opponentBoard.Draw();
        Rectangle opponentBoardRectangle = new(
            opponentBoard.Position,
            boardPixelWidth,
            boardPixelHeight
        );
        bool hoverOpponentBoard = Raylib.CheckCollisionPointRec(
            Raylib.GetMousePosition(),
            opponentBoardRectangle
        );
        if (!hoverOpponentBoard)
        {
            int opponentTitleWidth = Raylib.MeasureText("Opponent's Board", 15);
            Raylib.DrawText(
                "Opponent's Board",
                (int)opponentBoard.Position.X + (boardPixelWidth - opponentTitleWidth) / 2,
                (int)opponentBoard.Position.Y + 15,
                15,
                Color.Black
            );
        }

        playerBoard.Draw();
        Rectangle playerBoardRectangle = new(
            playerBoard.Position,
            boardPixelWidth,
            boardPixelHeight
        );
        bool hoverPlayerBoard = Raylib.CheckCollisionPointRec(
            Raylib.GetMousePosition(),
            playerBoardRectangle
        );
        if (!hoverPlayerBoard)
        {
            int playerTitleWidth = Raylib.MeasureText("Your Board", 15);
            Raylib.DrawText(
                "Your Board",
                (int)playerBoard.Position.X + (boardPixelWidth - playerTitleWidth) / 2,
                (int)playerBoard.Position.Y + 15,
                15,
                Color.Black
            );
        }

        DrawShipField();

        // Draw Back Button
        int backButtonWidth = 100;
        int backButtonHeight = 30;
        Rectangle backButton = new Rectangle(
            ZonePadding,
            currentScreenHeight - backButtonHeight - ZonePadding,
            backButtonWidth,
            backButtonHeight
        );
        bool hoverBack = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);
        Raylib.DrawRectangleRec(backButton, hoverBack ? Color.DarkGray : Color.Gray);
        Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 5, 20, Color.White);

        if (hoverBack && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            NetworkManager.Instance.Shutdown();
            _gameStateManager.SetStateToMainMenu();
        }

        CastingUI.Instance.Draw();
    }

    private void DrawRessourceField()
    {
        ressourceField!.Draw();
    }

    private void DrawShipField()
    {
        shipField!.Draw();
    }

    private void DrawCardStacksField()
    {
        cardStacksField!.Draw();
    }

    private void DrawActiveCards()
    {
        activeCards!.Draw();
    }

    // Update helper methods to accept screen dimensions
    private void DrawHands()
    {
        playerHand!.Draw(true);
        opponentHand!.DrawRotation(false, 180);
    }

    // Update DrawGraveyard signature to accept local cardWidth/Height
    private static void DrawGraveyard(
        float graveyardWidth,
        float graveyardHeight,
        float graveyardX,
        float graveyardY,
        int cardWidth,
        int cardHeight,
        float outerBufferWidth,
        float outerBufferHeight
    )
    {
        Rectangle outerZone = new(graveyardX, graveyardY, graveyardWidth, graveyardHeight);
        // Use passed-in cardWidth/Height
        Rectangle cardZone = new(
            graveyardX + outerBufferWidth,
            graveyardY + outerBufferHeight,
            cardWidth,
            cardHeight
        );
        int lineThickness = 2;
        Raylib.DrawRectangleRec(outerZone, Color.DarkGray);
        Raylib.DrawRectangleLinesEx(outerZone, 2, Color.Black);
        Raylib.DrawRectangleRec(cardZone, Color.LightGray);
        Raylib.DrawText(
            "Graveyard",
            (int)outerZone.X + lineThickness,
            (int)outerZone.Y + lineThickness,
            10,
            Color.White
        );
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
            playerBoard = new(
                playerBoard.GridWidth,
                playerBoard.GridHeight,
                playerBoard.CellSize,
                playerBoard.Position
            );
            opponentBoard = new(
                opponentBoard.GridWidth,
                opponentBoard.GridHeight,
                opponentBoard.CellSize,
                opponentBoard.Position
            );
        }
        InitializeHands();
    }

    /// <summary>
    /// Whether Raylib.GetMousePos() is currently Hovering over this Rectangle
    /// </summary>
    /// <param name="rec"></param>
    /// <returns>True if Mouse is over Rectangle rec</returns>
    public static bool IsHoveringRec(Rectangle rec)
    {
        return Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rec);
    }

    public void HandleBoughtCard(string variant)
    {
        if (Enum.TryParse<CardVariant>(variant, true, out var cardVariant))
        {
            playerHand!.AddCard(new(cardVariant));
        }
        else
        {
            Console.WriteLine($"[Client] CardBuy Failed. Variant {variant} unknown");
        }
    }

    public void HandleOpponentBoughtCard(string type)
    {
        List<Cards> cards;
        if (
            Enum.TryParse<CardType>(type, true, out var cardType)
            && (cards = Cards.GetCardsOfType(cardType)).Count > 0
        )
        {
            opponentHand!.AddCard(cards[0]);
        }
        else
        {
            Console.WriteLine($"[Client] CardBuy Failed. Type {type} unknown");
        }
    }

    /// <summary>
    /// Resets the GameScreen for a new game.
    /// </summary>
    public void ResetForNewGame()
    {
        playerBoard?.ClearBoard();
        opponentBoard?.ClearBoard();

        playerHand?.EmptyHand();
        opponentHand?.EmptyHand();

        activeCards?.UpdateActiveCards([]);

        shipSizeLimitReached.Clear();

        Console.WriteLine("[Client][GameScreen] Reset for new game completed");
    }

    /// <summary>
    /// Resets the Boards of the GameScreen, clearing all ships and cell states.
    /// </summary>
    public void ResetBoards()
    {
        playerBoard?.ClearBoard();
        opponentBoard?.ClearBoard();

        Console.WriteLine("[Client][GameScreen] Boards reset");
    }
}
