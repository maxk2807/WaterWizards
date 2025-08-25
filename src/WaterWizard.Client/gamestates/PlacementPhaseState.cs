// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 38 Zeilen
// - maxk2807: 11 Zeilen
// - erick: 2 Zeilen
// - Erickk0: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using Raylib_cs;
using WaterWizard.Client.Assets.Sounds.Manager;
using WaterWizard.Client.gamescreen.handler;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamestates;

/// <summary>
/// Repräsentiert die Phase, in der Spieler ihre Schiffe platzieren,
/// bevor das eigentliche Spiel beginnt.
/// </summary>
public class PlacementPhaseState : IGameState
{
    private bool IsReady = false;

    /// <summary>
    /// Zeichnet die Platzierungsphase inklusive Spielfeld, Hinweistext und Ready-Button.
    /// Verarbeitet Eingaben für das Bestätigen der Platzierung oder den Wechsel ins Spiel.
    /// </summary>
    /// <param name="manager">Verwalter für Spielzustände und Bildschirmabmessungen.</param>
    public void UpdateAndDraw(GameStateManager manager)
    {
        manager.GameScreen.Draw(manager.screenWidth, manager.screenHeight);
        string infoText = "Platzierungsphase: Platziere alle Schiffe!";
        int textWidth = Raylib.MeasureText(infoText, 30);
        int textY = (int)(manager.screenHeight * 0.036f);
        Raylib.DrawText(infoText, (manager.screenWidth - textWidth) / 2, textY, 30, Color.Red);

        int buttonWidth = 200;
        int buttonHeight = 50;
        int buttonX = (manager.screenWidth - buttonWidth) / 8;
        int buttonY = textY + (int)(manager.screenHeight * 0.15f);
        Rectangle readyButton = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
        bool hoverReady = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), readyButton);
        Raylib.DrawRectangleRec(
            readyButton,
            IsReady ? Color.LightGray
                : hoverReady ? Color.DarkGreen
                : Color.Green
        );
        if (IsReady)
            Raylib.DrawRectangleLinesEx(readyButton, 3, Color.Black);
        string readyText = "Fertig" + (!IsReady ? "?" : "!");
        int readyTextWidth = Raylib.MeasureText(readyText, 24);
        Raylib.DrawText(
            readyText,
            buttonX + (buttonWidth - readyTextWidth) / 2,
            buttonY + (buttonHeight - 24) / 2,
            24,
            Color.White
        );
        if (hoverReady && Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            HandleShips.SendPlacementReady(NetworkManager.Instance);
            IsReady = true;
            Raylib.PlaySound(SoundManager.ButtonSound);
            //manager.SetStateToInGame();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.L))
        {
            manager.SetStateToInGame();
        }
    }
}
