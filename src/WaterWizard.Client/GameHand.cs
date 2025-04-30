using System.Numerics;
using Raylib_cs;
using WaterWizard.Shared;

namespace WaterWizard.Client;

public class GameHand(GameScreen gameScreen)
{
    /// <summary>
    /// Represents the Cards on the given Hand.
    /// </summary>
    private List<Cards> _cards = new()
    {
        //Beispielkarten
        new(CardVariant.ArcaneMissile),
        new(CardVariant.Firebolt),
        new(CardVariant.Heal),
        new(CardVariant.Storm)
    };

    private int _screenWidth => gameScreen._gameStateManager.screenWidth;
    private int _screenHeight => gameScreen._gameStateManager.screenHeight;

    float HandWidth => _screenWidth * 0.25f;
    float HandHeight => _screenHeight * 0.15f;


    public void DrawAsOpponentHand(float zonePadding, int cardWidth, int cardHeight)
    {
        Rectangle opponentHandZone = new(_screenWidth - HandWidth - zonePadding, zonePadding, HandWidth, HandHeight);
        Raylib.DrawRectangleRec(opponentHandZone, Color.LightGray);
        Raylib.DrawRectangleLinesEx(opponentHandZone, 2, Color.DarkGray);
        Raylib.DrawText("Opponent Hand", (int)(opponentHandZone.X + 10), (int)(opponentHandZone.Y + 10), 10, Color.Black);
    }

    public void DrawAsPlayerHand(float zonePadding, int cardWidth, int cardHeight)
    {
        Rectangle playerHandZone = new(_screenWidth - HandWidth - zonePadding, _screenHeight - HandHeight - zonePadding, HandWidth, HandHeight);
        Raylib.DrawRectangleRec(playerHandZone, Color.LightGray);
        Raylib.DrawRectangleLinesEx(playerHandZone, 2, Color.DarkGray);
        Raylib.DrawText("Player Hand", (int)(playerHandZone.X + 10), (int)(playerHandZone.Y + 10), 10, Color.Black);
        int cardX = (int)Math.Round(_screenWidth - HandWidth - zonePadding);
        int cardY = (int)Math.Round(_screenHeight - HandHeight - zonePadding);
        DrawCard(cardX, cardY,cardWidth, cardHeight, true);
    }

    private static void DrawCard(int cardX, int cardY, int cardWidth, int cardHeight, bool front)
    {
        if (front)
        {
            Rectangle card = new(cardX, cardY, cardWidth, cardHeight);
            Raylib.DrawRectangleRec(card, Color.Black);
        }
    }
}