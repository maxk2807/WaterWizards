using Raylib_cs;
using WaterWizard.Shared;

namespace WaterWizard.Client.gamescreen.cards;

public class GameHand(GameScreen gameScreen, int centralX, int cardY)
{
    /// <summary>
    /// Represents the Cards on the given Hand.
    /// </summary>
    private List<GameCard> _cards = [];

    private int ScreenWidth => gameScreen._gameStateManager.screenWidth;
    private int ScreenHeight => gameScreen._gameStateManager.screenHeight;
    private int CardWidth => gameScreen.cardWidth;
    private int CardHeight => gameScreen.cardHeight;

    /// <summary>
    /// Render the Cards on this GameHand instance. If calling this with isOpponent true,
    /// the cards take on a different color to signify the back of the cards, and get rendered
    /// on the side of the opponent.
    /// </summary>
    /// <param name="front"></param>
    public void Draw(bool front)
    {
        //space available for the cards to draw
        int availableHandWidth = (int)(ScreenWidth * 0.2f);
        //space the cards would take up if side by side
        int totalCardWidth = _cards.Count * CardWidth;
        //difference between these two spaces
        int excess = totalCardWidth - availableHandWidth;
        //calculate based on difference, how much cards need to be compressed
        int offset = excess > 0 ? excess / _cards.Count : 0;
        for (int i = 0; i < _cards.Count; i++)
        {
            int cardX;
            //actual width of rendered cards, after compression
            int effectiveCardWidth = CardWidth - offset;

            // calculate card position based on:
            // are cards even, how many cards, i, how much overlap
            bool areCardsEven = _cards.Count % 2 == 0;
            cardX = areCardsEven
                ? -(_cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth
                : -effectiveCardWidth / 2
                    - _cards.Count / 2 * effectiveCardWidth
                    + i * effectiveCardWidth;

            _cards[i].Draw(centralX + cardX, cardY, front);

            //Draw Card Preview if Mouse over card snippet
            Rectangle cardRec = new(centralX + cardX, cardY, effectiveCardWidth, CardHeight);
            if (front && GameScreen.IsHoveringRec(cardRec))
            {
                DrawPreview(_cards[i]);
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    HandleCast(_cards[i]);
                }
            }
        }
    }

    private void HandleCast(GameCard gameCard)
    {
        HandleConfirmCast(gameCard);
    }

    private void HandleConfirmCast(GameCard gameCard)
    {
        gameScreen.opponentBoard!.StartDrawingCardAim(gameCard);
    }

    /// <summary>
    /// Render the Cards on this GameHand instance with the given rotation. If calling this with isOpponent true,
    /// the cards take on a different color to signify the back of the cards, and get rendered
    /// on the side of the opponent.
    /// </summary>
    /// <param name="front"></param>
    /// <param name="rot"></param>
    public void DrawRotation(bool front, float rot)
    {
        //space available for the cards to draw
        int availableHandWidth = (int)(ScreenWidth * 0.2f);
        //space the cards would take up if side by side
        int totalCardWidth = _cards.Count * CardWidth;
        //difference between these two spaces
        int excess = totalCardWidth - availableHandWidth;
        //calculate based on difference, how much cards need to be compressed
        int offset = excess > 0 ? excess / _cards.Count : 0;
        for (int i = 0; i < _cards.Count; i++)
        {
            int cardX;
            //actual width of rendered cards, after compression
            int effectiveCardWidth = CardWidth - offset;

            // calculate card position based on:
            // are cards even, how many cards, i, how much overlap
            bool areCardsEven = _cards.Count % 2 == 0;
            cardX = areCardsEven
                ? -(_cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth
                : -effectiveCardWidth / 2
                    - _cards.Count / 2 * effectiveCardWidth
                    + i * effectiveCardWidth;

            _cards[i].DrawRotation(centralX + cardX, cardY, front, rot);

            //Draw Card Preview if Mouse over card snippet
            Rectangle cardRec = new(centralX + cardX, cardY, effectiveCardWidth, CardHeight);
            if (front && GameScreen.IsHoveringRec(cardRec))
            {
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
        int previewX = centralX - CardWidth / 2 + (int)(ScreenWidth * 0.03f);
        int previewY = cardY - CardHeight / 2 - (int)(ScreenHeight * 0.10f);

        card.Draw(previewX, previewY, true);
    }

    public void AddCard(Cards card)
    {
        _cards.Add(new(gameScreen, card));
    }

    public void EmptyHand()
    {
        _cards.Clear();
    }
}
