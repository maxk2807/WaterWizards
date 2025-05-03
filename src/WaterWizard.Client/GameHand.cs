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
        new(CardVariant.Storm),
        new(CardVariant.Storm),
        new(CardVariant.Storm),
        new(CardVariant.Storm)
    };

    private int _screenWidth => gameScreen._gameStateManager.screenWidth;
    private int _screenHeight => gameScreen._gameStateManager.screenHeight;

    float HandWidth => _screenWidth * 0.25f;
    float HandHeight => _screenHeight * 0.15f;

    private float _zonePadding => gameScreen.ZonePadding;
    private int _cardWidth => gameScreen.cardWidth;
    private int _cardHeight => gameScreen.cardHeight;

    public void Draw(bool isOpponent)
    {
        //space available for the cards to draw
        int availableHandWidth = (int)(_screenWidth * 0.23f);
        //space the cards would take up if side by side
        int totalCardWidth = _cards.Count * _cardWidth;
        //difference between these two spaces
        int excess =  totalCardWidth - availableHandWidth;
        //calculate based on difference, how much cards need to be compressed
        int offset = excess > 0 ? excess / _cards.Count : 0;
        for (int i = 0; i < _cards.Count; i++)
        {
            int cardX;
            //actual width of rendered cards, after compression
            int effectiveCardWidth = _cardWidth - offset;
            if (_cards.Count % 2 == 0)
            {
                cardX = -(_cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth ;
            }
            else
            {
                cardX = -effectiveCardWidth / 2 - (_cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth;
            }
            if(isOpponent){
                DrawCard(centralX + cardX, centralY, true,
                _cards[i].Type == CardType.Damage ? Color.Red :
                _cards[i].Type == CardType.Environment ? Color.Blue : Color.Green);
            } else {
                DrawCard(centralX + cardX, centralY, true,
                _cards[i].Type == CardType.Damage ? new(238, 156, 156) :
                _cards[i].Type == CardType.Environment ? new(210,152,255) : new(149, 251, 215));

            }
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