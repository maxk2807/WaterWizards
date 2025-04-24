using Raylib_cs;
using System;

namespace WaterWizard.Client
{
    public class GameStateManager
    {

        // Singleton-Instanz
        private static GameStateManager? instance;
        public static GameStateManager Instance => instance ?? throw new InvalidOperationException("GameStateManager wurde nicht initialisiert!");
        private string playerName = "Player";
        private bool isEditingName = false;
        private readonly GameTimer gameTimer;
        // Initialisierungsmethode
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
            HostingMenu,
            PreStartLobby,
            InGame
        }

        private GameState currentState = GameState.MainMenu;
        private readonly int screenWidth;
        private readonly int screenHeight;
        private string inputText = "localhost"; // Default IP for joining
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
            gameTimer = new GameTimer(this);
        }

        public void UpdateAndDraw()
        {
            NetworkManager.Instance.PollEvents();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Beige);

            // Handle the game timer in the hosting menu
            if(currentState == GameState.HostingMenu)
            {
                gameTimer.Update();
                if(gameTimer.IsTimeUp)
                {
                    NetworkManager.Instance.Shutdown();
                    SetStateToMainMenu();
                }
            }

            switch (currentState)
            {
                case GameState.MainMenu:
                    DrawMainMenu();
                    break;
                case GameState.ConnectingMenu:
                    DrawConnectMenu();
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

            Raylib.EndDrawing();
        }

        private void DrawMainMenu()
        {
            Raylib.DrawText("Welcome to WaterWizards!", screenWidth / 3, screenHeight / 3, 30, Color.DarkBlue);

            Rectangle joinButton = new Rectangle(screenWidth / 2 - 100, screenHeight / 2, 200, 40);
            Rectangle hostButton = new Rectangle(screenWidth / 2 - 100, screenHeight / 2 + 60, 200, 40);

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), joinButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.ConnectingMenu;
                isEditingIp = true; // Enable IP editing
            }

            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), hostButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.HostingMenu;
                NetworkManager.Instance.StartHosting();
            }

            Raylib.DrawRectangleRec(joinButton, Color.Blue);
            Raylib.DrawText("Join Lobby", (int)joinButton.X + 50, (int)joinButton.Y + 10, 20, Color.White);

            Raylib.DrawRectangleRec(hostButton, Color.Blue);
            Raylib.DrawText("Host Lobby", (int)hostButton.X + 50, (int)hostButton.Y + 10, 20, Color.White);
        }

        private void DrawConnectMenu()
        {

            Raylib.DrawText("Enter IP Address to Connect:", screenWidth / 3, screenHeight / 3, 20, Color.DarkBlue);

            Rectangle inputBox = new Rectangle(screenWidth / 3, screenHeight / 2, 300, 40);
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

            Rectangle connectButton = new Rectangle(screenWidth / 2 - 80, screenHeight / 2 + 60, 160, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), connectButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.ConnectToServer(inputText, 7777);
                //currentState = GameState.PreStartLobby;
            }

            Raylib.DrawRectangleRec(connectButton, Color.Blue);
            Raylib.DrawText("Connect", (int)connectButton.X + 40, (int)connectButton.Y + 10, 20, Color.White);

            // Back Button
            Rectangle backButton = new Rectangle(screenWidth / 3, screenHeight / 2 + 120, 100, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.MainMenu;
                isEditingIp = false; // Stop editing
            }

            Raylib.DrawRectangleRec(backButton, Color.Gray);
            Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 10, 20, Color.White);
        }

        private void DrawHostMenu()
        {
            // Draw the time in the host menu on the top left corner in red
            gameTimer.Draw(10, 10, 20, Color.Red);

            string publicIp = NetworkUtils.GetPublicIPAddress();
            int hostPort = NetworkManager.Instance.GetHostPort();
            bool isPlayerConnected = NetworkManager.Instance.IsPlayerConnected();

            Raylib.DrawText($"Hosting on: {publicIp}:{hostPort}", screenWidth / 3, screenHeight / 3, 20, Color.DarkGreen);
            Raylib.DrawText(isPlayerConnected ? "Player Connected!" : "Waiting for players...", screenWidth / 3, screenHeight / 3 + 40, 20, isPlayerConnected ? Color.Green : Color.DarkBlue);

            // Back Button
            Rectangle backButton = new Rectangle(screenWidth / 3, screenHeight / 2 + 120, 100, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.Shutdown(); // Stop hosting
                currentState = GameState.MainMenu;
            }

            Raylib.DrawRectangleRec(backButton, Color.Gray);
            Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 10, 20, Color.White);
        }


        private void DrawPreStartLobby()
        {
            Raylib.DrawText("Waiting for players...", screenWidth / 3, screenHeight / 3, 20, Color.DarkBlue);

            // Zeige die Liste der verbundenen Spieler an
            var players = NetworkManager.Instance.GetConnectedPlayers();
            Raylib.DrawText($"Connected Players: {players.Count}", screenWidth / 3, screenHeight / 3 + 40, 18, Color.DarkGreen);

            for (int i = 0; i < players.Count; i++)
            {
                // Convert Player object to string using ToString() or a specific property like Name
                Raylib.DrawText(players[i].ToString(), screenWidth / 3, screenHeight / 3 + 70 + (i * 25), 16, Color.Black);
            }

            // �berpr�fe, ob wir der Host sind (Server ist nicht null)
            bool isHost = NetworkManager.Instance.IsHost();

            if (isHost)
            {
                Rectangle startButton = new Rectangle(screenWidth / 2 - 80, screenHeight / 2 + 60, 160, 40);
                if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), startButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    NetworkManager.Instance.SendToAllClients("StartGame");
                    SetStateToInGame(); // Host wechselt in den Spielzustand
                }

                Raylib.DrawRectangleRec(startButton, Color.Blue);
                Raylib.DrawText("Start Game", (int)startButton.X + 40, (int)startButton.Y + 10, 20, Color.White);
            }

            if (!NetworkManager.Instance.IsHost())
            {
                bool isReady = NetworkManager.Instance.IsClientReady();
                Rectangle readyButton = new Rectangle(screenWidth / 2 - 80, screenHeight / 2 + 60, 160, 40);

                if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), readyButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    NetworkManager.Instance.ToggleReadyStatus();
                }

                Raylib.DrawRectangleRec(readyButton, isReady ? Color.Green : Color.Orange);
                Raylib.DrawText(isReady ? "Ready" : "Not Ready", (int)readyButton.X + 40, (int)readyButton.Y + 10, 20, Color.White);
            }

            // Back Button
            Rectangle backButton = new Rectangle(screenWidth / 3, screenHeight / 2 + 120, 100, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.Shutdown(); // Stop hosting or disconnect
                currentState = GameState.MainMenu;
            }

            Raylib.DrawRectangleRec(backButton, Color.Gray);
            Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 10, 20, Color.White);
        }

        private void HandleTextInput()
        {
            int key = Raylib.GetCharPressed();
            while (key > 0)
            {
                if ((key >= 32 && key <= 126)) // Printable ASCII characters
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
                isEditingIp = false; // Stop editing on Enter
            }
        }
        public void SetStateToLobby()
        {
            currentState = GameState.PreStartLobby;
        }

        public void SetStateToInGame()
        {
            currentState = GameState.InGame;
        }

        public void SetStateToMainMenu()
        {
            currentState = GameState.MainMenu;
        }

        private void DrawGameScreen()
        {
            // Placeholder implementation for the InGame state rendering.
            Raylib.DrawText("Game is running...", screenWidth / 3, screenHeight / 3, 20, Color.DarkGreen);

            // Back Button
            Rectangle backButton = new Rectangle(screenWidth / 3, screenHeight / 2 + 120, 100, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                NetworkManager.Instance.Shutdown(); // Disconnect or stop hosting
                currentState = GameState.MainMenu;
            }

            Raylib.DrawRectangleRec(backButton, Color.Gray);
            Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 10, 20, Color.White);
        }
    }
}
