// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 140 Zeilen
// - erick: 22 Zeilen
// - jdewi001: 11 Zeilen
// - Erickk0: 3 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using Raylib_cs;
using WaterWizard.Shared;
using static WaterWizard.Client.gamescreen.cards.ActiveCards;

namespace WaterWizard.Client.gamescreen.cards;

public class GameHand(GameScreen gameScreen, int centralX, int cardY)
{
    /// <summary>
    /// Represents the Cards on the given Hand.
    /// </summary>
    public List<GameCard> Cards { get; private set; } = [];

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
    public virtual void Draw(bool front)
    {
        //space available for the cards to draw
        int availableHandWidth = (int)(ScreenWidth * 0.2f);
        //space the cards would take up if side by side
        int totalCardWidth = Cards.Count * CardWidth;
        //difference between these two spaces
        int excess = totalCardWidth - availableHandWidth;
        //calculate based on difference, how much cards need to be compressed
        int offset = excess > 0 ? excess / Cards.Count : 0;
        for (int i = 0; i < Cards.Count; i++)
        {
            int cardX;
            //actual width of rendered cards, after compression
            int effectiveCardWidth = CardWidth - offset;

            // calculate card position based on:
            // are cards even, how many cards, i, how much overlap
            bool areCardsEven = Cards.Count % 2 == 0;
            cardX = areCardsEven
                ? -(Cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth
                : -effectiveCardWidth / 2
                    - Cards.Count / 2 * effectiveCardWidth
                    + i * effectiveCardWidth;

            Cards[i].Draw(centralX + cardX, cardY, front);

            DrawCardPreview(front, i, cardX, effectiveCardWidth);
        }
    }

    /// <summary>
    /// Checks whether the given Card is being hovered. Handles the drawing of the preview of a
    /// hovered Card as well as handles the cast-process of the card:
    /// 1. Clicking on the hovered card 2. Confirming selection, by aiming if required
    /// 3. Making Cast-Request to the Server
    /// </summary>
    /// <param name="front">
    ///     Whether the current GameHand is facing the front, ie. is the players hand.
    ///     If false, doesn't draw card preview
    /// </param>
    /// <param name="i">Index of the Card to check</param>
    /// <param name="cardX">X coordinate of the Card to check, relative to the middle of the GameHand</param>
    /// <param name="effectiveCardWidth">
    ///     How wide the Card to check is on screen, ie. how much of it is not covered by another Card
    /// </param>
    private void DrawCardPreview(bool front, int i, int cardX, int effectiveCardWidth)
    {
        //Draw Card Preview if Mouse over card snippet
        Rectangle cardRec = new(centralX + cardX, cardY, effectiveCardWidth, CardHeight);
        if (front && GameScreen.IsHoveringRec(cardRec))
        {
            DrawPreview(Cards[i]);
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                HandleCast(Cards[i]);
            }
        }
    }

    internal virtual void HandleCast(GameCard gameCard)
    {
        CastingUI.Instance.StartDrawingCardAim(gameCard);
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
        int totalCardWidth = Cards.Count * CardWidth;
        //difference between these two spaces
        int excess = totalCardWidth - availableHandWidth;
        //calculate based on difference, how much cards need to be compressed
        int offset = excess > 0 ? excess / Cards.Count : 0;
        for (int i = 0; i < Cards.Count; i++)
        {
            int cardX;
            //actual width of rendered cards, after compression
            int effectiveCardWidth = CardWidth - offset;

            // calculate card position based on:
            // are cards even, how many cards, i, how much overlap
            bool areCardsEven = Cards.Count % 2 == 0;
            cardX = areCardsEven
                ? -(Cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth
                : -effectiveCardWidth / 2
                    - Cards.Count / 2 * effectiveCardWidth
                    + i * effectiveCardWidth;

            Cards[i].DrawRotation(centralX + cardX, cardY, front, rot);

            //Draw Card Preview if Mouse over card snippet
            Rectangle cardRec = new(centralX + cardX, cardY, effectiveCardWidth, CardHeight);
            if (front && GameScreen.IsHoveringRec(cardRec))
            {
                DrawPreview(Cards[i]);
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
        Cards.Add(new(gameScreen, card));
    }

    public void EmptyHand()
    {
        Cards.Clear();
    }

    /// <summary>
    /// Removes a Card from the Hand by its GameCard instance.
    /// </summary>
    /// <param name="cardToRemove">The Card that will be removed after Casting</param>
    public void RemoveCard(GameCard cardToRemove)
    {
        Cards.Remove(cardToRemove);
    }

    /// <summary>
    /// Removes a Card from the Hand by its Cards enum value.
    /// </summary>
    /// <param name="cardToRemove">The Card that will be removed after Casting</param>
    public void RemoveCard(Cards cardToRemove)
    {
        var gameCard = Cards.Find(gc => gc.card == cardToRemove);
        if (gameCard != null)
        {
            Cards.Remove(gameCard);
        }
    }
}
