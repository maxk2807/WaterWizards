using System.Numerics;
using Raylib_cs;
using WaterWizard.Shared;

namespace WaterWizard.Client;

public class GameHand(GameScreen gameScreen, int centralX, int centralY)
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

    private float _zonePadding => gameScreen.ZonePadding;
    private int _cardWidth => gameScreen.cardWidth;
    private int _cardHeight => gameScreen.cardHeight;

    public void DrawAsOpponentHand()
    {
        Rectangle opponentHandZone = new(centralX, centralY, HandWidth, HandHeight);
        Raylib.DrawRectangleRec(opponentHandZone, Color.LightGray);
        Raylib.DrawRectangleLinesEx(opponentHandZone, 2, Color.DarkGray);
        Raylib.DrawText("Opponent Hand", (int)(opponentHandZone.X + 10), (int)(opponentHandZone.Y + 10), 10, Color.Black);
    }

    public void DrawAsPlayerHand()
    {
        Rectangle playerHandZone = new(centralX, centralY, HandWidth, HandHeight);
        int cardY = (int)Math.Round(_screenHeight - HandHeight - _zonePadding);
        for (int i = 0; i < _cards.Count; i++)
        {
            int offsetX;
            if (_cards.Count % 2 == 0)
            {
                offsetX = -(_cards.Count / 2 * _cardWidth) + i * _cardWidth;
            }
            else
            {
                offsetX = -_cardWidth / 2 - (_cards.Count / 2 * _cardWidth) + i * _cardWidth;
            }
            DrawCard(centralX + offsetX, cardY, true,
            _cards[i].Type == CardType.Damage ? Color.Red :
            _cards[i].Type == CardType.Environment ? Color.Blue : Color.Green);
        }
    }

    private void DrawCard(int cardX, int cardY, bool front, Color color)
    {
        if (front)
        {
            Rectangle card = new(cardX, cardY, _cardWidth, _cardHeight);
            //possible rotation
            // float rotationOriginX = _cardWidth / 2;
            // float rotationOriginY = _cardHeight;
            // Raylib.DrawRectanglePro(card, new(rotationOriginX, rotationOriginY), 2.3f, color);
            Raylib.DrawRectangleRec(card, color);
            Raylib.DrawRectangleLinesEx(card, 2, Color.Black);
        }
    }
}