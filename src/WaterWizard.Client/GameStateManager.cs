using Raylib_cs;
using System;
using System.Numerics; 
using WaterWizard.Shared;

namespace WaterWizard.Client
{
    public class GameStateManager
    {
        private static GameStateManager? instance;
        public static GameStateManager Instance => instance ?? throw new InvalidOperationException("GameStateManager wurde nicht initialisiert!");
        private readonly GameTimer gameTimer;

        // Add GameBoard instances
        private GameBoard? playerBoard;
        private GameBoard? opponentBoard;


        private float titleAnimTime = 0;
        private float titleVerticalPosition = 0;
        private const float TITLE_ANIM_SPEED = 1.5f;
        private const float TITLE_FLOAT_AMPLITUDE = 10.0f;
      
        public static void Initialize(int screenWidth, int screenHeight)
        {
            if (instance == null)
            {
                instance = new GameStateManager(screenWidth, screenHeight);
            }
        }

        private enum GameState
        {
            MainMenu,
            ConnectingMenu,
            LobbyListMenu,
            HostingMenu,
            PreStartLobby,
            InGame
        }

        private GameState currentState = GameState.MainMenu;
        private int screenWidth;
        private int screenHeight;
        private string inputText = "localhost"; // Default
        private bool isEditingIp = false;

        /// <summary>
        /// Constructor for GameStateManager.
        /// </summary>
        /// <param name="screenWidth">The width of the game screen in pixels.</param>
        /// <param name="screenHeight">The height of the game screen in pixels.</param>
        public GameStateManager(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            // Pass 'this' instance to GameTimer
            gameTimer = new GameTimer(this);
            // Initialize boards - adjust position and size as needed
            InitializeBoards();
        }

        private void InitializeBoards()
        {
            int boardSize = 10;
            int cellSize = 25; 
            int boardPixelSize = boardSize * cellSize;

            float gapBetweenBoards = screenHeight * 0.05f;
            float totalBoardBlockHeight = boardPixelSize * 2f + gapBetweenBoards; 

            int opponentBoardY = Math.Max((int)((screenHeight - totalBoardBlockHeight) / 2f), (int)(screenHeight * 0.1f));
            int opponentBoardX = (screenWidth - boardPixelSize) / 2;

            Vector2 opponentBoardPos = new(opponentBoardX, opponentBoardY);
            Vector2 playerBoardPos = new(opponentBoardX, opponentBoardY + boardPixelSize + (int)gapBetweenBoards);

            if (playerBoard == null || opponentBoard == null)
            {
                // First time initialization
                playerBoard = new GameBoard(boardSize, boardSize, cellSize, playerBoardPos);
                opponentBoard = new GameBoard(boardSize, boardSize, cellSize, opponentBoardPos);
            }
            else
            {
                playerBoard.Position = playerBoardPos;
                opponentBoard.Position = opponentBoardPos;
            }
        }

        /// <summary>
        /// Updates the screen dimensions when the window size changes.
        /// </summary>
        /// <param name="width">New screen width.</param>
        /// <param name="height">New screen height.</param>
        public void UpdateScreenSize(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
            Console.WriteLine($"Screen size updated to {width}x{height}");
            // Re-initialize boards to potentially reposition them based on new screen size
            InitializeBoards();
        }

        /// <summary>
        /// Aktualisiert den Spielzustand und zeichnet die entsprechende Benutzeroberfläche.
        /// Diese Methode muss in jedem Frame aufgerufen werden.
        /// </summary>
        public void UpdateAndDraw()
        {
            NetworkManager.Instance.PollEvents();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Beige);
            // Handle the game timer in the hosting menu
            if (currentState == GameState.HostingMenu || currentState == GameState.InGame)
            {
                gameTimer.Update();
                if (gameTimer.IsTimeUp)
                {
                    NetworkManager.Instance.Shutdown();
                    SetStateToMainMenu();
                }
            }
            else
            {
                gameTimer.Reset();
            }

            switch (currentState)
            {
                case GameState.MainMenu:
                    DrawMainMenu();
                    break;
                case GameState.ConnectingMenu:
                    DrawConnectMenu();
                    break;
                case GameState.LobbyListMenu:
                    DrawLobbyListMenu();
                    break;
                case GameState.HostingMenu:
                    DrawHostMenu();
                    break;
                case GameState.PreStartLobby:
                    DrawPreStartLobby();
                    break;
                case GameState.InGame:
                    DrawGameScreen();
                    break;
            }
            string screenInfo = $"Auflösung: {screenWidth}x{screenHeight} | F11: Vollbild umschalten";
            int infoWidth = Raylib.MeasureText(screenInfo, 12);
            Raylib.DrawText(screenInfo, screenWidth - infoWidth - 10, screenHeight - 20, 12, Color.DarkGray);

            Raylib.EndDrawing();
        }

        private void DrawMainMenu()
        {
            string title = "Welcome to WaterWizards!";
            float letterSpacing = 3;

            float totalTitleWidth = 0;
            for (int i = 0; i < title.Length; i++)
            {
                totalTitleWidth += Raylib.MeasureText(title[i].ToString(), 30) + letterSpacing;
            }

            totalTitleWidth -= letterSpacing;

            titleAnimTime += Raylib.GetFrameTime() * TITLE_ANIM_SPEED;

            titleVerticalPosition = (float)Math.Sin(titleAnimTime) * TITLE_FLOAT_AMPLITUDE;

            float titleX = (screenWidth - totalTitleWidth) / 2;
            int titleY = screenHeight / 4 + (int)titleVerticalPosition;

            for (int i = 0; i < title.Length; i++)
            {
                float hue = (titleAnimTime * 0.3f + i * 0.05f) % 1.0f;
                Color charColor = ColorFromHSV(hue * 360, 0.7f, 0.9f);

                int charWidth = Raylib.MeasureText(title[i].ToString(), 30);

                Raylib.DrawText(
                    title[i].ToString(),
                    (int)titleX,
                    titleY,
                    30,
                    charColor);

                titleX += charWidth + letterSpacing;
            }

            Rectangle joinButton = new((float)screenWidth / 2 - 100, (float)screenHeight / 2, 200, 40);
            Rectangle hostButton = new((float)screenWidth / 2 - 100, (float)screenHeight / 2 + 60, 200, 40);
            Rectangle mapButton = new((float)screenWidth / 2 - 100, (float)screenHeight / 2 + 120, 200, 40);

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), joinButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.LobbyListMenu;
                NetworkManager.Instance.DiscoverLobbies();
            }

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), hostButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.HostingMenu;
                NetworkManager.Instance.StartHosting();
            }

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), mapButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.InGame;
            }

            Raylib.DrawRectangleRec(joinButton, Color.Blue);
            Raylib.DrawText("Join Lobby", (int)joinButton.X + 50, (int)joinButton.Y + 10, 20, Color.White);

            Raylib.DrawRectangleRec(hostButton, Color.Blue);
            Raylib.DrawText("Host Lobby", (int)hostButton.X + 50, (int)hostButton.Y + 10, 20, Color.White);
            
            Raylib.DrawRectangleRec(mapButton, Color.Blue);
            Raylib.DrawText("Map Test", (int)mapButton.X + 50, (int)mapButton.Y + 10, 20, Color.White);
        }

        private Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = (int)(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            value = value * 255;
            int v = (int)value;
            int p = (int)(value * (1 - saturation));
            int q = (int)(value * (1 - f * saturation));
            int t = (int)(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new Color(v, t, p, 255);
            else if (hi == 1)
                return new Color(q, v, p, 255);
            else if (hi == 2)
                return new Color(p, v, t, 255);
            else if (hi == 3)
                return new Color(p, q, v, 255);
            else if (hi == 4)
                return new Color(t, p, v, 255);
            else
                return new Color(v, p, q, 255);
        }

        private void DrawLobbyListMenu()
        {
            int titleWidth = Raylib.MeasureText("Verfügbare Lobbies", 30);
            Raylib.DrawText("Verfügbare Lobbies", (screenWidth - titleWidth) / 2, screenHeight / 10, 30, Color.DarkBlue);

            var lobbies = NetworkManager.Instance.GetDiscoveredLobbies();

            if (lobbies.Count == 0)
            {
                int noLobbiesWidth = Raylib.MeasureText("Suche nach verfügbaren Lobbies...", 20);
                Raylib.DrawText(
                    "Suche nach verfügbaren Lobbies...",
                    (screenWidth - noLobbiesWidth) / 2,
                    screenHeight / 3,
                    20,
                    Color.DarkGray);
            }
            else
            {
                int yPos = screenHeight / 4;

                int headerSpacing = 300;
                int nameWidth = Raylib.MeasureText("Lobby Name", 20);
                int spielerWidth = Raylib.MeasureText("Spieler", 20);
                int tableWidth = nameWidth + headerSpacing + spielerWidth;
                int tableX = (screenWidth - tableWidth) / 2;

                Raylib.DrawText("Lobby Name", tableX, yPos, 20, Color.DarkBlue);
                Raylib.DrawText("Spieler", tableX + headerSpacing, yPos, 20, Color.DarkBlue);
                yPos += 30;

                for (int i = 0; i < lobbies.Count; i++)
                {
                    var lobby = lobbies[i];
                    Rectangle lobbyRect = new Rectangle(tableX - 20, yPos - 5, 400, 35);

                    if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), lobbyRect))
                    {
                        Raylib.DrawRectangleRec(lobbyRect, new Color(200, 200, 255, 100));

                        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                        {
                            string ip = lobby.IP.Split(':')[0];
                            NetworkManager.Instance.ConnectToServer(ip, 7777);
                        }
                    }

                    Raylib.DrawText(lobby.Name, tableX, yPos, 18, Color.Black);
                    Raylib.DrawText($"{lobby.PlayerCount}", tableX + headerSpacing, yPos, 18, Color.Black);

                    Rectangle joinBtn = new Rectangle(tableX + tableWidth - 100, yPos - 5, 100, 30);
                    bool hoverJoin = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), joinBtn);

                    Raylib.DrawRectangleRec(joinBtn, hoverJoin ? Color.DarkGreen : Color.Green);
                    Raylib.DrawText("Join", (int)joinBtn.X + 35, (int)joinBtn.Y + 5, 18, Color.White);

                    if (hoverJoin && Raylib.IsMouseButtonReleased(MouseButton.Left))
                    {
                        string ip = lobby.IP.Split(':')[0];
                        NetworkManager.Instance.ConnectToServer(ip, 7777);
                    }

                    yPos += 40;
                }
            }

            int margin = 20;
            int buttonHeight = 40;
            int buttonWidth = 120;

            // -----------------------
            // BACK BUTTON
            // -----------------------
            Rectangle backButton = new Rectangle(
                margin,
                screenHeight - buttonHeight - margin,
                buttonWidth,
                buttonHeight);

            bool hoverBack = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);
            Raylib.DrawRectangleRec(
                backButton,
                hoverBack ? new Color(100, 100, 100, 255) : Color.Gray);

            string backText = "Zurück";
            int backTextWidth = Raylib.MeasureText(backText, 20);
            Raylib.DrawText(
                backText,
                (int)backButton.X + (buttonWidth - backTextWidth) / 2,
                (int)backButton.Y + (buttonHeight - 20) / 2,
                20,
                Color.White);

            if (hoverBack && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.Shutdown();
                currentState = GameState.MainMenu;
            }

            // -----------------------
            // REFRESH BUTTON
            // -----------------------
            string refreshText = "Aktualisieren";
            int refreshTextWidth = Raylib.MeasureText(refreshText, 20);
            int refreshButtonWidth = refreshTextWidth + 40;

            Rectangle refreshButton = new Rectangle(
                screenWidth - refreshButtonWidth - margin,
                screenHeight - buttonHeight - margin,
                refreshButtonWidth,
                buttonHeight);

            bool hoverRefresh = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), refreshButton);

            Raylib.DrawRectangleRec(
                refreshButton,
                hoverRefresh ? new Color(70, 120, 70, 255) : new Color(60, 160, 60, 255));

            Raylib.DrawText(
                refreshText,
                (int)refreshButton.X + (refreshButtonWidth - refreshTextWidth) / 2,
                (int)refreshButton.Y + (buttonHeight - 20) / 2,
                20,
                Color.White);

            if (hoverRefresh && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.RefreshLobbies();
            }

            // -----------------------
            // MANUAL IP BUTTON
            // -----------------------
            int manualBtnWidth = 300;
            Rectangle manualIpButton = new Rectangle(
                (float)(screenWidth - manualBtnWidth) / 2,
                screenHeight - buttonHeight - margin,
                manualBtnWidth,
                buttonHeight);

            bool hoverManualIp = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), manualIpButton);
            Raylib.DrawRectangleRec(manualIpButton, hoverManualIp ? new Color(70, 70, 200, 255) : Color.Blue);

            string manualText = "Manuell verbinden";
            int manualTextWidth = Raylib.MeasureText(manualText, 20);
            Raylib.DrawText(
                manualText,
                (int)manualIpButton.X + (manualBtnWidth - manualTextWidth) / 2,
                (int)manualIpButton.Y + (buttonHeight - 20) / 2,
                20,
                Color.White);

            if (hoverManualIp && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.ConnectingMenu;
                isEditingIp = true;
            }
        }

        private void DrawConnectMenu()
        {
            Raylib.DrawText("Enter IP Address to Connect:", screenWidth / 3, screenHeight / 3, 20, Color.DarkBlue);

            Rectangle inputBox = new((float)screenWidth / 3, (float)screenHeight / 2, 300, 40);
            Raylib.DrawRectangleRec(inputBox, isEditingIp ? Color.White : Color.LightGray);
            Raylib.DrawRectangleLines((int)inputBox.X, (int)inputBox.Y, (int)inputBox.Width, (int)inputBox.Height, Color.DarkBlue);
            Raylib.DrawText(inputText, (int)inputBox.X + 5, (int)inputBox.Y + 10, 20, Color.Black);

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), inputBox) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                isEditingIp = true;
            }

            if (isEditingIp)
            {
                HandleTextInput();
            }

            Rectangle connectButton = new Rectangle((float)screenWidth / 2 - 80, (float)screenHeight / 2 + 60, 160, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), connectButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.ConnectToServer(inputText, 7777);
            }

            Raylib.DrawRectangleRec(connectButton, Color.Blue);
            Raylib.DrawText("Connect", (int)connectButton.X + 40, (int)connectButton.Y + 10, 20, Color.White);

            // Back Button
            Rectangle backButton = new((float)screenWidth / 3, (float)screenHeight / 2 + 120, 100, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.MainMenu;
                isEditingIp = false;
            }

            Raylib.DrawRectangleRec(backButton, Color.Gray);
            Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 10, 20, Color.White);
        }

        private void DrawHostMenu()
        {
            gameTimer.Draw(10, 10, 20, Color.Red);

            string publicIp = NetworkUtils.GetPublicIPAddress();
            int hostPort = NetworkManager.Instance.GetHostPort();
            bool isPlayerConnected = NetworkManager.Instance.IsPlayerConnected();

            int titleWidth = Raylib.MeasureText($"Hosting on: {publicIp}:{hostPort}", 20);
            Raylib.DrawText($"Hosting on: {publicIp}:{hostPort}", (screenWidth - titleWidth) / 2, screenHeight / 3, 20, Color.DarkGreen);

            string statusText = isPlayerConnected ? "Player Connected!" : "Waiting for players...";
            int statusWidth = Raylib.MeasureText(statusText, 20);
            Raylib.DrawText(statusText, (screenWidth - statusWidth) / 2, screenHeight / 3 + 40, 20, isPlayerConnected ? Color.Green : Color.DarkBlue);

            int margin = 20;
            int buttonHeight = 40;
            int buttonWidth = 120;

            Rectangle backButton = new Rectangle(
                margin,
                screenHeight - buttonHeight - margin,
                buttonWidth,
                buttonHeight);

            bool hoverBack = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);
            Raylib.DrawRectangleRec(backButton, hoverBack ? new Color(100, 100, 100, 255) : Color.Gray);

            string backText = "Back";
            int backTextWidth = Raylib.MeasureText(backText, 20);
            Raylib.DrawText(
                backText,
                (int)backButton.X + (buttonWidth - backTextWidth) / 2,
                (int)backButton.Y + (buttonHeight - 20) / 2,
                20,
                Color.White);

            if (hoverBack && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.Shutdown();
                currentState = GameState.MainMenu;
            }
        }

        private void DrawPreStartLobby()
        {
            int titleWidth = Raylib.MeasureText("Waiting for players...", 30);
            Raylib.DrawText("Waiting for players...", (screenWidth - titleWidth) / 2, screenHeight / 10, 30, Color.DarkBlue);

            var players = NetworkManager.Instance.GetConnectedPlayers();
            string playerCountText = $"Connected Players: {players.Count}";
            int playerCountWidth = Raylib.MeasureText(playerCountText, 20);
            Raylib.DrawText(playerCountText, (screenWidth - playerCountWidth) / 2, screenHeight / 4, 20, Color.DarkGreen);

            int playerListY = screenHeight / 4 + 40;
            int maxListHeight = screenHeight / 2 - 80;

            for (int i = 0; i < players.Count; i++)
            {
                string playerText = players[i].ToString();
                int textWidth = Raylib.MeasureText(playerText, 18);
                if (playerListY + i * 30 < playerListY + maxListHeight)
                {
                    Raylib.DrawText(playerText, (screenWidth - textWidth) / 2, playerListY + i * 30, 18, Color.Black);
                }
            }

            int actionButtonY = screenHeight * 2 / 3;

            bool isHost = NetworkManager.Instance.IsHost();

            if (isHost)
            {
                int buttonWidth = 200;
                int buttonHeight = 50;
                Rectangle startButton = new Rectangle((float)(screenWidth - buttonWidth) / 2, actionButtonY, buttonWidth, buttonHeight);
                bool hoverStart = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), startButton);

                Raylib.DrawRectangleRec(startButton, hoverStart ? Color.DarkBlue : Color.Blue);

                string startText = "Start Game";
                int textWidth = Raylib.MeasureText(startText, 20);
                Raylib.DrawText(startText, (int)startButton.X + (buttonWidth - textWidth) / 2,
                                (int)startButton.Y + (buttonHeight - 20) / 2, 20, Color.White);

                if (hoverStart && Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    NetworkManager.Instance.SendToAllClients("StartGame");
                    SetStateToInGame();
                }
            }
            else
            {
                int buttonWidth = 200;
                int buttonHeight = 50;
                bool isReady = NetworkManager.Instance.IsClientReady();
                Rectangle readyButton = new Rectangle((float)(screenWidth - buttonWidth) / 2, actionButtonY, buttonWidth, buttonHeight);
                bool hoverReady = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), readyButton);

                Raylib.DrawRectangleRec(readyButton, isReady ? Color.Green : (hoverReady ? Color.DarkGray : Color.Pink));

                string readyText = isReady ? "Ready" : "Get Ready";
                int textWidth = Raylib.MeasureText(readyText, 20);
                Raylib.DrawText(readyText, (int)readyButton.X + (buttonWidth - textWidth) / 2,
                               (int)readyButton.Y + (buttonHeight - 20) / 2, 20, Color.White);

                if (hoverReady && Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    NetworkManager.Instance.ToggleReadyStatus();
                }
            }

            int backButtonWidth = 120;
            int backButtonHeight = 40;
            int backButtonMargin = 20;
            Rectangle backButton = new Rectangle(backButtonMargin, screenHeight - backButtonHeight - backButtonMargin,
                                                backButtonWidth, backButtonHeight);
            bool hoverBack = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);

            Raylib.DrawRectangleRec(backButton, hoverBack ? new Color(100, 100, 100, 255) : Color.Gray);

            string backText = "Back";
            int backTextWidth = Raylib.MeasureText(backText, 20);
            Raylib.DrawText(backText, (int)backButton.X + (backButtonWidth - backTextWidth) / 2,
                           (int)backButton.Y + (backButtonHeight - 20) / 2, 20, Color.White);

            if (hoverBack && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.Shutdown();
                currentState = GameState.MainMenu;
            }
        }

        private void HandleTextInput()
        {
            int key = Raylib.GetCharPressed();
            while (key > 0)
            {
                if ((key >= 32 && key <= 126))
                {
                    inputText += (char)key;
                }
                key = Raylib.GetCharPressed();
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && inputText.Length > 0)
            {
                inputText = inputText.Substring(0, inputText.Length - 1);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                isEditingIp = false;
            }
        }

        public void SetStateToLobby()
        {
            Console.WriteLine("[GameStateManager] Wechsel in den Zustand: PreStartLobby");
            currentState = GameState.PreStartLobby;
        }

        public void SetStateToInGame()
        {
            Console.WriteLine("[GameStateManager] Wechsel in den Zustand: InGame");
            currentState = GameState.InGame;
            gameTimer.Reset(); 
            if (playerBoard == null || opponentBoard == null)
            {
                InitializeBoards();
            }
        }

        public void SetStateToMainMenu()
        {
            Console.WriteLine("[GameStateManager] Wechsel in den Zustand: MainMenu");
            currentState = GameState.MainMenu;
        }

        private void DrawGameScreen()
        {
            if (playerBoard == null || opponentBoard == null)
            {
                Raylib.DrawText("Initializing game boards...", 10, 50, 20, Color.Gray);
                return;
            }

            float zonePadding = screenWidth * 0.02f;
            int boardPixelSize = playerBoard.GridWidth * playerBoard.CellSize;


            // Opponent Hand
            float handWidth = screenWidth * 0.25f;
            float handHeight = screenHeight * 0.15f;
            Rectangle opponentHandZone = new(screenWidth - handWidth - zonePadding, zonePadding, handWidth, handHeight);
            Raylib.DrawRectangleRec(opponentHandZone, Color.LightGray);
            Raylib.DrawRectangleLinesEx(opponentHandZone, 2, Color.DarkGray);
            Raylib.DrawText("Opponent Hand", (int)(opponentHandZone.X + 10), (int)(opponentHandZone.Y + 10), 10, Color.Black);

            // Player Hand 
            Rectangle playerHandZone = new(screenWidth - handWidth - zonePadding, screenHeight - handHeight - zonePadding, handWidth, handHeight);
            Raylib.DrawRectangleRec(playerHandZone, Color.LightGray);
            Raylib.DrawRectangleLinesEx(playerHandZone, 2, Color.DarkGray);
            Raylib.DrawText("Player Hand", (int)(playerHandZone.X + 10), (int)(playerHandZone.Y + 10), 10, Color.Black);

            // Utility Area 
            float utilityWidth = screenWidth * 0.1f;
            float utilityHeight = screenHeight * 0.3f;
            float utilityX = zonePadding;
            float utilityY = (screenHeight - utilityHeight) / 2f; 
            Rectangle utilityZone = new(utilityX, utilityY, utilityWidth, utilityHeight);
            Raylib.DrawRectangleRec(utilityZone, Color.LightGray);
            Raylib.DrawRectangleLinesEx(utilityZone, 2, Color.DarkGray);
            Raylib.DrawText("Utility", (int)(utilityZone.X + 10), (int)(utilityZone.Y + 10), 10, Color.Black);

            // Update and Draw Game Boards
            GameBoard.Point? clickedCell = opponentBoard.Update();
            if (clickedCell.HasValue)
            {
                Console.WriteLine($"Attack initiated at ({clickedCell.Value.X}, {clickedCell.Value.Y})");
                // TODO: Send attack command
            }

            // Draw board titles centered above boards
            int playerTitleWidth = Raylib.MeasureText("Your Board", 15);
            Raylib.DrawText("Your Board", (int)playerBoard.Position.X + (boardPixelSize - playerTitleWidth)/2, (int)playerBoard.Position.Y - 20, 15, Color.Black);
            playerBoard.Draw();

            int opponentTitleWidth = Raylib.MeasureText("Opponent's Board", 15);
            Raylib.DrawText("Opponent's Board", (int)opponentBoard.Position.X + (boardPixelSize - opponentTitleWidth)/2, (int)opponentBoard.Position.Y - 20, 15, Color.Black);
            opponentBoard.Draw();

            // Draw Timer 
            float timerX = opponentBoard.Position.X + boardPixelSize + zonePadding * 2;
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
                SetStateToMainMenu();
            }
        }
    }
}
