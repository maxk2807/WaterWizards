// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 113 Zeilen
// - jdewi001: 40 Zeilen
// - maxk2807: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - private readonly List<string> _messages = new List<string>();   (justinjd00: 107 Zeilen)
// ===============================================

using Raylib_cs;
using WaterWizard.Client.network;

namespace WaterWizard.Client;

/// <summary>
/// Verwaltet den Chatverlauf und die Chat-Eingabe im Spiel.
/// </summary>
public class ChatLogManager
{
    private readonly List<string> _messages = new List<string>();
    private readonly int _maxMessages = 100;
    private string _currentInput = "";
    private bool _isTyping = false;
    private Rectangle _chatArea;
    private Rectangle _inputArea;
    private float _scrollOffset = 0;
    private readonly int _lineHeight = 18;

    /// <summary>
    /// Fügt dem Chat eine neue Nachricht hinzu.
    /// </summary>
    /// <param name="message">Die Nachricht, die hinzugefügt werden soll.</param>
    public void AddMessage(string message)
    {
        _messages.Add(message);

        if (_messages.Count > _maxMessages)
        {
            _messages.RemoveAt(0);
        }
        _scrollOffset = Math.Max(0, (_messages.Count * (float)_lineHeight) - _chatArea.Height);
    }

    /// <summary>
    /// Verarbeitet die Benutzereingabe für den Chat (Tippen, Senden, Scrollen).
    /// </summary>
    public void HandleInput()
    {
        if (_isTyping)
        {
            int key = Raylib.GetCharPressed();
            while (key > 0)
            {
                if ((key >= 32 && key <= 126) && _currentInput.Length < 100)
                {
                    _currentInput += (char)key;
                }
                key = Raylib.GetCharPressed();
            }

            if (Raylib.IsKeyPressedRepeat(KeyboardKey.Backspace) && _currentInput.Length > 0)
            {
                _currentInput = _currentInput[..^1];
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                if (!string.IsNullOrWhiteSpace(_currentInput))
                {
                    NetworkManager.Instance.SendChatMessage(_currentInput);
                    _currentInput = "";
                }
                _isTyping = false;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                _isTyping = false;
            }
        }
        else
        {
            if (
                Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _inputArea)
                && Raylib.IsMouseButtonReleased(MouseButton.Left)
            )
            {
                _isTyping = true;
            }
        }

        float wheelMove = Raylib.GetMouseWheelMove();
        float epsilon = 0.0001f;
        if (
            Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _chatArea)
            && Math.Abs(wheelMove) > epsilon
        )
        {
            _scrollOffset -= wheelMove * _lineHeight * 3;
            _scrollOffset = Math.Clamp(
                _scrollOffset,
                0,
                Math.Max(0, (_messages.Count * (float)_lineHeight) - _chatArea.Height)
            );
        }
    }

    /// <summary>
    /// Zeichnet das Chatfenster und das Eingabefeld auf den Bildschirm.
    /// </summary>
    /// <param name="screenWidth">Bildschirmbreite</param>
    /// <param name="screenHeight">Bildschirmhöhe</param>
    public void Draw(int screenWidth, int screenHeight)
    {
        float chatHeight = screenHeight * 0.4f;
        float inputHeight = 30f;
        float chatWidth = screenWidth * 0.3f;
        float chatX = screenWidth - chatWidth - 20;
        float chatY = (screenHeight - chatHeight - inputHeight - 10) / 2;

        _chatArea = new Rectangle(chatX, chatY, chatWidth, chatHeight);
        _inputArea = new Rectangle(chatX, chatY + chatHeight + 10, chatWidth, inputHeight);

        Raylib.DrawRectangleRec(_chatArea, new Color(245, 245, 245, 220));
        Raylib.DrawRectangleLinesEx(_chatArea, 1, Color.DarkGray);

        Raylib.BeginScissorMode(
            (int)_chatArea.X,
            (int)_chatArea.Y,
            (int)_chatArea.Width,
            (int)_chatArea.Height
        );

        int startY = (int)(_chatArea.Y - _scrollOffset + 5);
        for (int i = 0; i < _messages.Count; i++)
        {
            int currentY = startY + i * _lineHeight;
            if (currentY + _lineHeight > _chatArea.Y && currentY < _chatArea.Y + _chatArea.Height)
            {
                Raylib.DrawText(_messages[i], (int)_chatArea.X + 5, currentY, 16, Color.Black);
            }
        }

        Raylib.EndScissorMode();

        Raylib.DrawRectangleRec(_inputArea, Color.White);
        Raylib.DrawRectangleLinesEx(_inputArea, 1, _isTyping ? Color.Blue : Color.DarkGray);
        Raylib.DrawText(
            _currentInput,
            (int)_inputArea.X + 5,
            (int)_inputArea.Y + 7,
            16,
            Color.Black
        );

        if (_isTyping && (int)(Raylib.GetTime() * 2) % 2 == 0)
        {
            int textWidth = Raylib.MeasureText(_currentInput, 16);
            Raylib.DrawText(
                "|",
                (int)_inputArea.X + 5 + textWidth,
                (int)_inputArea.Y + 7,
                16,
                Color.Black
            );
        }
        else if (!_isTyping && string.IsNullOrWhiteSpace(_currentInput))
        {
            Raylib.DrawText(
                "Click here to type...",
                (int)_inputArea.X + 5,
                (int)_inputArea.Y + 7,
                16,
                Color.Gray
            );
        }
    }
}
