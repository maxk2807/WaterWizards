// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 61 Zeilen
// - Paul: 23 Zeilen
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
/// Zustandsklasse für den Hosting-Bildschirm. Lädt das Menü-Hintergrundbild
/// und rendert die Hosting-UI (öffentliche IP/Port, Verbindungsstatus,
/// Zurück-Button) während der Host auf Spieler wartet.
/// </summary>
public class HostingMenuState : IGameState
{
    private Texture2D menuBackground;

    /// <summary>
    /// Lädt einmalig die für den Hosting-Bildschirm benötigten Texturen
    /// (Menü-Hintergrund). Führt bei bereits geladenen Assets keinen erneuten
    /// Ladevorgang aus.
    /// </summary>
    public void LoadAssets()
    {
        if (menuBackground.Id != 0)
            return; // Falls bereits geladen, nichts tun

        menuBackground = TextureManager.LoadTexture(
            "Background/WaterWizardsMenu1200x900.png"
        );
    }

    /// <summary>
    /// Aktualisiert und zeichnet den Hosting-Bildschirm für den aktuellen Frame,
    /// indem die UI-Darstellung delegiert wird.
    /// </summary>
    /// <param name="manager">Zustands- und Bildschirmgrößen-Verwalter.</param>
    public void UpdateAndDraw(GameStateManager manager)
    {
        DrawHostMenu(manager);
    }

    /// <summary>
    /// Rendert den Hosting-Bildschirm: skaliertes Hintergrundbild, Timer,
    /// öffentliche IP und Port, Verbindungsstatus sowie den „Back“-Button.
    /// Behandelt außerdem die Eingabe für den Zurück-Navi­gationsbutton.
    /// </summary>
    /// <param name="manager">Zustands- und Bildschirmgrößen-Verwalter.</param>
    private void DrawHostMenu(GameStateManager manager)
    {
        LoadAssets();

        //Raylib.DrawTexture(menuBackground, 0, 0, Color.White);

        Raylib.DrawTexturePro(
            menuBackground,
            new Rectangle(0, 0, menuBackground.Width, menuBackground.Height),
            new Rectangle(0, 0, manager.screenWidth, manager.screenHeight),
            Vector2.Zero,
            0f,
            Color.White
        );

        manager.GetGameTimer().Draw(10, 10, 20, Color.Red);
        string publicIp = WaterWizard.Shared.NetworkUtils.GetPublicIPAddress();
        int hostPort = NetworkManager.Instance.GetHostPort();
        bool isPlayerConnected = NetworkManager.Instance.IsPlayerConnected();
        int titleWidth = Raylib.MeasureText($"Hosting on: {publicIp}:{hostPort}", 20);
        Raylib.DrawText(
            $"Hosting on: {publicIp}:{hostPort}",
            (manager.screenWidth - titleWidth) / 2,
            manager.screenHeight / 3,
            20,
            Color.DarkGreen
        );
        string statusText = isPlayerConnected ? "Player Connected!" : "Waiting for players...";
        int statusWidth = Raylib.MeasureText(statusText, 20);
        Raylib.DrawText(
            statusText,
            (manager.screenWidth - statusWidth) / 2,
            manager.screenHeight / 3 + 40,
            20,
            isPlayerConnected ? Color.Green : Color.DarkBlue
        );
        int margin = 20;
        int buttonHeight = 40;
        int buttonWidth = 120;
        Rectangle backButton = new Rectangle(
            margin,
            manager.screenHeight - buttonHeight - margin,
            buttonWidth,
            buttonHeight
        );
        bool hoverBack = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), backButton);
        Raylib.DrawRectangleRec(backButton, hoverBack ? new Color(100, 100, 100, 255) : Color.Gray);
        string backText = "Back";
        int backTextWidth = Raylib.MeasureText(backText, 20);
        Raylib.DrawText(
            backText,
            (int)backButton.X + (buttonWidth - backTextWidth) / 2,
            (int)backButton.Y + (buttonHeight - 20) / 2,
            20,
            Color.White
        );
        if (hoverBack && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            NetworkManager.Instance.Shutdown();
            manager.SetStateToMainMenu();
        }
    }
}
