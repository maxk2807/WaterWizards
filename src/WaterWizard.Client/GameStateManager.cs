using Raylib_cs;
using System;
using System.Numerics;
using WaterWizard.Client.gamescreen;
using WaterWizard.Client.gamestates;
using WaterWizard.Shared;

namespace WaterWizard.Client;

public class GameStateManager
{
    private static GameStateManager? instance;
    public static GameStateManager Instance =>
        instance ?? throw new InvalidOperationException(
                        "GameStateManager wurde nicht initialisiert!");
    private readonly GameTimer gameTimer;
    public GameTimer GetGameTimer() => gameTimer;
    private readonly GamePauseManager _gamePauseManager;
    public GamePauseManager GetGamePauseManager() => _gamePauseManager;
    private GameScreen? gameScreen;
    public GameScreen GameScreen =>
        gameScreen ?? throw new InvalidOperationException(
                          "Game Screen wurde nicht initialisiert!");

    private readonly ChatLogManager _chatLogManager;
    public ChatLogManager ChatLog => _chatLogManager;

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


    private IGameState currentState;

    public int screenWidth;
    public int screenHeight;
    private string inputText = "localhost"; // Default
    private string inputPortText = "7777"; // Default
    private bool isEditingIp = false;
    private bool isEditingPort = false;

    public string GetInputText() => inputText;
    public string GetInputPortText() => inputPortText;
    public bool IsEditingIp() => isEditingIp;
    public bool IsEditingPort() => isEditingPort;
    public void SetEditingIp(bool value) => isEditingIp = value;
    public void SetEditingPort(bool value) => isEditingPort = value;
    public void SetInputText(string value) => inputText = value;
    public void SetInputPortText(string value) => inputPortText = value;

    /// <summary>
    /// Constructor for GameStateManager.
    /// </summary>
    /// <param name="screenWidth">The width of the game screen in
    /// pixels.</param> <param name="screenHeight">The height of the game screen
    /// in pixels.</param>
    public GameStateManager(int screenWidth, int screenHeight)
    {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        gameTimer = new GameTimer(this);
        _chatLogManager = new ChatLogManager();
        _gamePauseManager = new GamePauseManager(gameTimer);
        InitializeGameScreen();
        currentState = new MainMenuState();
    }

    private void InitializeGameScreen()
    {
        gameScreen ??= new GameScreen(this, screenWidth, screenHeight, gameTimer);
        gameScreen.Initialize();
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
        GameScreen.UpdateScreenSize(width, height);
    }

    /// <summary>
    /// Aktualisiert den Spielzustand und zeichnet die entsprechende
    /// Benutzeroberfläche. Diese Methode muss in jedem Frame aufgerufen werden.
    /// </summary>
    public void UpdateAndDraw()
    {
        NetworkManager.Instance.PollEvents();

        if (Raylib.IsKeyPressed(KeyboardKey.P))
        {
            if (currentState is InGameState)
            {
                _gamePauseManager.TogglePause();
            }
        }
        if (currentState is PreStartLobbyState)
        {
            _chatLogManager.HandleInput();
        }

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Beige);
        if (currentState is HostingMenuState || currentState is InGameState)
        {
            if (!_gamePauseManager.IsGamePaused)
            {
                gameTimer.Update();
            }
            if (gameTimer.IsTimeUp)
            {
                NetworkManager.Instance.Shutdown();
                SetStateToMainMenu();
            }
        }
        else
        {
            Raylib.DrawText("Enter IP Address:", screenWidth / 3,
                            screenHeight / 2 - 40, 20, Color.DarkBlue);

            Rectangle inputBox =
                new((float)screenWidth / 3, (float)screenHeight / 2, 300, 40);
            Raylib.DrawRectangleRec(inputBox,
                                    isEditingIp ? Color.White : Color.LightGray);
            Raylib.DrawRectangleLines((int)inputBox.X, (int)inputBox.Y,
                                      (int)inputBox.Width, (int)inputBox.Height,
                                      Color.DarkBlue);
            Raylib.DrawText(inputText, (int)inputBox.X + 5, (int)inputBox.Y + 10, 20,
                            Color.Black);

            Raylib.DrawText("Enter Port:", screenWidth / 2 + 40, screenHeight / 2 - 40, 20, Color.DarkBlue); 
            Rectangle portInputBox = new((float)screenWidth / 3 + 300, (float)screenHeight / 2, 100, 40); 
            Raylib.DrawRectangleRec(portInputBox, isEditingPort ? Color.White : Color.LightGray);
            Raylib.DrawRectangleLines((int)portInputBox.X, (int)portInputBox.Y, (int)portInputBox.Width, (int)portInputBox.Height, Color.DarkBlue);
            Raylib.DrawText(inputPortText, (int)portInputBox.X + 5, (int)portInputBox.Y + 10, 20, Color.Black);


            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), inputBox))
                {
                    isEditingIp = true;
                    isEditingPort = false;
                }
                else if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), portInputBox))
                {
                    isEditingIp = false;
                    isEditingPort = true;
                }
                else
                {
                    isEditingIp = false;
                    isEditingPort = false;
                }
            }

            if (isEditingIp || isEditingPort) 
            {
                HandleTextInput();
            }

            Rectangle connectButton = new(
                (float)screenWidth / 2 - 80, (float)screenHeight / 2 + 60, 160, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(),
                                              connectButton) &&
                Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                if (int.TryParse(inputPortText, out int portValue) && portValue > 0 && portValue <= 65535)
                {
                    NetworkManager.Instance.ConnectToServer(inputText, portValue);
                }
                else
                {
                    Console.WriteLine("Invalid port number. Please enter a number between 1 and 65535.");
                }
            }

            Raylib.DrawRectangleRec(connectButton, Color.Blue);
            Raylib.DrawText("Connect", (int)connectButton.X + 40,
                            (int)connectButton.Y + 10, 20, Color.White);

            Rectangle backButton =
                new((float)screenWidth / 3, (float)screenHeight / 2 + 120, 100, 40);
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(),
                                              backButton) &&
                Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                currentState = GameState.MainMenu;
                isEditingIp = false;
            }

            Raylib.DrawRectangleRec(backButton, Color.Gray);
            Raylib.DrawText("Back", (int)backButton.X + 30, (int)backButton.Y + 10,
                            20, Color.White);
            gameTimer.Reset();
        }

        currentState.UpdateAndDraw(this);

        string screenInfo =
            $"Auflösung: {screenWidth}x{screenHeight} | F11: Vollbild umschalten";
        int infoWidth = Raylib.MeasureText(screenInfo, 12);
        Raylib.DrawText(screenInfo, screenWidth - infoWidth - 10,
                        screenHeight - 20, 12, Color.DarkGray);

        Raylib.EndDrawing();
    }

    public void SetStateToMainMenu() => currentState = new MainMenuState();
    public void SetStateToConnectingMenu() => currentState = new ConnectingMenuState();
    public void SetStateToLobbyList()
    {
        currentState = new LobbyListMenuState();
        NetworkManager.Instance.DiscoverLobbies();
    }
    public void SetStateToHostingMenu()
    {
        currentState = new HostingMenuState();
        NetworkManager.Instance.StartHosting();
    }
    public void SetStateToLobby() => currentState = new PreStartLobbyState();
    public void SetStateToPlacementPhase() => currentState = new PlacementPhaseState();
    public void SetStateToInGame()
    {
        currentState = new InGameState();
        gameTimer.Reset();
        GameScreen.Reset();
    }

    public void HandleTextInput()
    {
        int key = Raylib.GetCharPressed();
        while (key > 0)
        {
            if ((key >= 32 && key <= 126))
            {
                if (isEditingIp)
                {
                    if (inputText.Length < 45)
                        inputText += (char)key;
                }
                else if (isEditingPort)
                {
                    if ((key >= '0' && key <= '9') && inputPortText.Length < 5)
                        inputPortText += (char)key;
                }
            }
            key = Raylib.GetCharPressed();
        }

        if ((Raylib.IsKeyPressedRepeat(KeyboardKey.Backspace) || Raylib.IsKeyPressed(KeyboardKey.Backspace)))
        {
            int key = Raylib.GetCharPressed();
            while (key > 0)
            {
                if (isEditingIp)
                {
                    if ((key >= 32 && key <= 126) && inputText.Length < 45) 
                    {
                        inputText += (char)key;
                    }
                }
                else if (isEditingPort)
                {
                    if ((key >= '0' && key <= '9') && inputPortText.Length < 5) 
                    {
                        inputPortText += (char)key;
                    }
                }
                key = Raylib.GetCharPressed();
            }

            if (Raylib.IsKeyPressedRepeat(KeyboardKey.Backspace) || Raylib.IsKeyPressed(KeyboardKey.Backspace))

            {
                if (isEditingIp && inputText.Length > 0)
                {
                    inputText = inputText.Substring(0, inputText.Length - 1);
                }
                else if (isEditingPort && inputPortText.Length > 0)
                {
                    inputPortText = inputPortText.Substring(0, inputPortText.Length - 1);
                }
            }
            else if (isEditingPort && inputPortText.Length > 0)
            {
                isEditingIp = false;
                isEditingPort = false; 
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                 isEditingIp = false;
                 isEditingPort = false;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            isEditingIp = false;
            isEditingPort = false;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            isEditingIp = false;
            isEditingPort = false;
        }
    }

    public Color ColorFromHSV(float hue, float saturation, float value)
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
}

