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
        new(CardVariant.Storm)
    };

    private int _screenWidth => gameScreen._gameStateManager.screenWidth;
    private int _screenHeight => gameScreen._gameStateManager.screenHeight;
    private int _cardWidth => gameScreen.cardWidth;
    private int _cardHeight => gameScreen.cardHeight;

    /// <summary>
    /// Render the Cards on this GameHand instance. If calling this with isOpponent true, 
    /// the cards take on a different color to signify the back of the cards, and get rendered 
    /// on the side of the opponent.
    /// </summary>
    /// <param name="isOpponent"></param>
    public void Draw(bool isOpponent)
    {
        //space available for the cards to draw
        int availableHandWidth = (int)(_screenWidth * 0.2f);
        //space the cards would take up if side by side
        int totalCardWidth = _cards.Count * _cardWidth;
        //difference between these two spaces
        int excess = totalCardWidth - availableHandWidth;
        //calculate based on difference, how much cards need to be compressed
        int offset = excess > 0 ? excess / _cards.Count : 0;
        for (int i = 0; i < _cards.Count; i++)
        {
            int cardX;
            //actual width of rendered cards, after compression
            int effectiveCardWidth = _cardWidth - offset;

            // checks whether cards are even
            bool areCardsEven = _cards.Count % 2 == 0;
            cardX = areCardsEven
                ? -(_cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth
                : -effectiveCardWidth / 2 - (_cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth;

            if (isOpponent)
            {
                DrawCard(centralX + cardX, centralY, true,
                _cards[i].Type == CardType.Damage ? Color.Red :
                _cards[i].Type == CardType.Environment ? Color.Blue : Color.Green);
            }
            else
            {
                DrawCard(centralX + cardX, centralY, true,
                _cards[i].Type == CardType.Damage ? new(238, 156, 156) :
                _cards[i].Type == CardType.Environment ? new(210, 152, 255) : new(149, 251, 215));

                //Draw Card Preview if Mouse over card snippet
                Rectangle cardRec = new(centralX + cardX, centralY, effectiveCardWidth, _cardHeight);
                if(IsHoveringRec(cardRec)){
                    DrawPreview(_cards[i]);
                }
            }
        }

    }

    /// <summary>
    /// Draw the individual Card at the given coordinates.
    /// The Card currently consists of the given color, and a black outline.
    /// </summary>
    /// <param name="cardX"></param>
    /// <param name="cardY"></param>
    /// <param name="front"></param>
    /// <param name="color"></param>
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

    private static bool IsHoveringRec(Rectangle rec){
        return Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rec);
    }

    private void DrawPreview(Cards card)
    {
        int previewX = centralX - _cardWidth / 2 + (int)(_screenWidth * 0.03f);
        int previewY = centralY - _cardHeight / 2 - (int)(_screenHeight * 0.12f);

        DrawCard(previewX, previewY, true, card.Type == CardType.Damage ? new(238, 156, 156) 
        : card.Type == CardType.Environment ? new(210, 152, 255) : new(149, 251, 215));
    }
}