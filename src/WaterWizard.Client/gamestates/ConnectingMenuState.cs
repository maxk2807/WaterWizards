// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 108 Zeilen
// - Erickk0: 29 Zeilen
// - Paul: 21 Zeilen
// - maxk2807: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamestates;

/// <summary>
/// Repräsentiert den Verbindungsmenü-Zustand, in dem der Spieler IP-Adresse und Port eingeben
/// und eine Verbindung zu einem Server herstellen kann.
/// </summary>
public class ConnectingMenuState : IGameState
{
    private Texture2D menuBackground;

    /// <summary>
    /// Lädt die Hintergrund-Assets für das Verbindungsmenü,
    /// falls diese noch nicht geladen wurden.
    /// </summary>
    private void LoadAssets()
    {
        if (menuBackground.Id != 0)
            return;
        menuBackground = TextureManager.LoadTexture(
            "Background/WaterWizardsMenu1200x900.png"
        );
    }

    /// <summary>
    /// Aktualisiert den Zustand des Menüs und zeichnet die Inhalte des Verbindungsmenüs.
    /// </summary>
    /// <param name="manager">Der GameStateManager, der den aktuellen Spielzustand verwaltet.</param>
    public void UpdateAndDraw(GameStateManager manager)
    {
        DrawConnectMenu(manager);
    }

    /// <summary>
    /// Zeichnet das Verbindungsmenü, inklusive IP-/Port-Eingabe, Connect- und Back-Button.
    /// </summary>
    /// <param name="manager">Der GameStateManager, der den aktuellen Spielzustand verwaltet.</param>
    private void DrawConnectMenu(GameStateManager manager)
    {
        LoadAssets();

        Raylib.DrawTexturePro(
            menuBackground,
            new Rectangle(0, 0, menuBackground.Width, menuBackground.Height),
            new Rectangle(0, 0, manager.screenWidth, manager.screenHeight),
            Vector2.Zero,
            0f,
            Color.White
        );

        Raylib.DrawText(
            "Enter IP Address:",
            manager.screenWidth / 3,
            manager.screenHeight / 2 - 40,
            20,
            Color.DarkBlue
        );
        Rectangle inputBox = new(
            (float)manager.screenWidth / 3,
            (float)manager.screenHeight / 2,
            250,
            40
        );
        Raylib.DrawRectangleRec(inputBox, manager.IsEditingIp() ? Color.White : Color.LightGray);
        Raylib.DrawRectangleLinesEx(inputBox, 1, Color.DarkBlue);
        Raylib.DrawText(
            manager.GetInputText(),
            (int)inputBox.X + 5,
            (int)inputBox.Y + 10,
            20,
            Color.Black
        );

        int portInputX = (int)inputBox.X + (int)inputBox.Width + 20;
        Raylib.DrawText(
            "Enter Port:",
            portInputX,
            manager.screenHeight / 2 - 40,
            20,
            Color.DarkBlue
        );
        Rectangle portInputBox = new(portInputX, (float)manager.screenHeight / 2, 100, 40);
        Raylib.DrawRectangleRec(
            portInputBox,
            manager.IsEditingPort() ? Color.White : Color.LightGray
        );
        Raylib.DrawRectangleLinesEx(portInputBox, 1, Color.DarkBlue);
        Raylib.DrawText(
            manager.GetInputPortText(),
            (int)portInputBox.X + 5,
            (int)portInputBox.Y + 10,
            20,
            Color.Black
        );

        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), inputBox))
            {
                manager.SetEditingIp(true);
                manager.SetEditingPort(false);
            }
            else if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), portInputBox))
            {
                manager.SetEditingPort(true);
                manager.SetEditingIp(false);
            }
            else
            {
                manager.SetEditingIp(false);
                manager.SetEditingPort(false);
            }
        }

        if (manager.IsEditingIp() || manager.IsEditingPort())
        {
            manager.HandleTextInput();
        }

        Rectangle connectButton = new Rectangle(
            manager.screenWidth / 2f - 80,
            manager.screenHeight / 2f + 60,
            160,
            40
        );
        if (
            Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), connectButton)
            && Raylib.IsMouseButtonReleased(MouseButton.Left)
        )
        {
            if (int.TryParse(manager.GetInputPortText(), out int port))
            {
                NetworkManager.Instance.ConnectToServer(manager.GetInputText(), port);
            }
            else
            {
                NetworkManager.Instance.ConnectToServer(manager.GetInputText(), 7777);
                Console.WriteLine("Invalid port, using default 7777");
            }
        }
        Raylib.DrawRectangleRec(connectButton, Color.Blue);
        Raylib.DrawText(
            "Connect",
            (int)connectButton.X + (160 - Raylib.MeasureText("Connect", 20)) / 2,
            (int)connectButton.Y + 10,
            20,
            Color.White
        );

        Rectangle backButton = new(
            manager.screenWidth / 2f - 80,
            manager.screenHeight / 2f + 120,
            160,
            40
        );
        if (
            Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton)
            && Raylib.IsMouseButtonReleased(MouseButton.Left)
        )
        {
            manager.SetStateToMainMenu();
            manager.SetEditingIp(false);
            manager.SetEditingPort(false);
        }
        Raylib.DrawRectangleRec(backButton, Color.Gray);
        Raylib.DrawText(
            "Back",
            (int)backButton.X + (160 - Raylib.MeasureText("Back", 20)) / 2,
            (int)backButton.Y + 10,
            20,
            Color.White
        );
    }
}
