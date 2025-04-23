using Raylib_cs;

namespace WaterWizard.Client
{
    public class GameStateManager
    {
        private enum GameState
        {
            MainMenu,
            ConnectingMenu,
            HostingMenu,
            InGame
        }

        private GameState currentState = GameState.MainMenu;
        private readonly int screenWidth;
        private readonly int screenHeight;
        private string inputText = "localhost"; // Default IP for joining
        private bool isEditingIp = false;

        public GameStateManager(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
        }

        public void UpdateAndDraw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Beige);

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
                NetworkManager.Instance.ConnectToServer(inputText, 9050);
                currentState = GameState.InGame;
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


        private void DrawGameScreen()
        {
            Raylib.DrawText("Game In Progress", screenWidth / 3, screenHeight / 2, 30, Color.DarkBlue);

            // Back Button
            Rectangle backButton = new Rectangle(screenWidth / 3, screenHeight / 2 + 120, 100, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton) && Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
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
    }
}
