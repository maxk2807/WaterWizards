using Raylib_cs;
using WaterWizard.Shared;

namespace WaterWizard.Client.gamescreen;

public class GameHand(GameScreen gameScreen, int centralX, int centralY)
{
    /// <summary>
    /// Represents the Cards on the given Hand.
    /// </summary>
    private List<GameCard> _cards = new()
    {
        //Beispielkarten
        new(gameScreen, new(CardVariant.ArcaneMissile)),
        new(gameScreen, new(CardVariant.Firebolt)),
        new(gameScreen, new(CardVariant.Heal)),
        new(gameScreen, new(CardVariant.Storm)),
        new(gameScreen, new(CardVariant.Storm)),
        new(gameScreen, new(CardVariant.Storm)),
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

            // calculate card position based on:
            // are cards even, how many cards, i, how much overlap
            bool areCardsEven = _cards.Count % 2 == 0;
            cardX = areCardsEven
                ? -(_cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth
                : -effectiveCardWidth / 2 - _cards.Count / 2 * effectiveCardWidth + i * effectiveCardWidth;

            _cards[i].Draw(centralX + cardX, centralY, !isOpponent);

            //Draw Card Preview if Mouse over card snippet
            Rectangle cardRec = new(centralX + cardX, centralY, effectiveCardWidth, _cardHeight);
            if(GameScreen.IsHoveringRec(cardRec)){
                DrawPreview(_cards[i]);
            }
        }
    }

    /// <summary>
    /// Renders a Card Just above the Player Hand to Preview it
    /// </summary>
    /// <param name="card"></param>
    private void DrawPreview(GameCard card)
    {
        int previewX = centralX - _cardWidth / 2 + (int)(_screenWidth * 0.03f);
        int previewY = centralY - _cardHeight / 2 - (int)(_screenHeight * 0.12f);

        card.Draw(previewX, previewY, true);
    }
}