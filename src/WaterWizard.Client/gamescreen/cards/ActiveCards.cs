using System.Numerics;
using Raylib_cs;
using WaterWizard.Shared;

namespace WaterWizard.Client.gamescreen.cards;

public class ActiveCards(GameScreen gameScreen)
{
    private ActiveCardsHand? _cards;

    private int X,
        Y;
    private int Width,
        Height;
    private int ScreenWidth => gameScreen._gameStateManager.screenWidth;
    private int ScreenHeight => gameScreen._gameStateManager.screenHeight;

    public void Initialize()
    {
        Width = (int)(ScreenWidth * 0.274f);
        Height = (int)(ScreenHeight * 0.25f);
        X = (int)(ScreenWidth * 0.725f);
        Y = (ScreenHeight - Height) / 2;
        _cards = new(gameScreen, X + Width / 2, Y + Height / 2 - gameScreen.cardHeight / 2);
        _cards.EmptyHand();
    }

    public void Draw()
    {
        Rectangle outerRec = new(X, Y, Width, Height);
        Raylib.DrawRectangleRec(outerRec, Color.White);
        Raylib.DrawRectangleLinesEx(outerRec, 2, Color.Black);
        Raylib.DrawText("Active Cards", (int)outerRec.X + 2, (int)outerRec.Y + 2, 15, Color.Black);

        _cards?.Draw(true);
    }

    internal void UpdateActiveCards(List<Cards> activeCards)
    {
        _cards!.EmptyHand();
        activeCards.ForEach(_cards.AddCard);
    }

    private class ActiveCardsHand(GameScreen gameScreen, int centralX, int cardY)
        : GameHand(gameScreen, centralX, cardY)
    {
        private readonly GameScreen gameScreen = gameScreen;
        private readonly int centralX = centralX;
        private readonly int cardY = cardY;

        internal override void HandleCast(GameCard gameCard) {/*Can't cast active cards*/}

        public override void Draw(bool front)
        {
            int availableHandWidth = (int)(gameScreen._gameStateManager.screenWidth * 0.2f);
            int totalCardWidth = Cards.Count * gameScreen.cardWidth;
            int excess = totalCardWidth - availableHandWidth;
            int offset = excess > 0 ? excess / Cards.Count : 0;
            for (int i = 0; i < Cards.Count; i++)
            {
                int cardX;
                int effectiveCardWidth = gameScreen.cardWidth - offset;
                bool areCardsEven = Cards.Count % 2 == 0;
                cardX = areCardsEven
                    ? -(Cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth
                    : -effectiveCardWidth / 2
                        - Cards.Count / 2 * effectiveCardWidth
                        + i * effectiveCardWidth;

                Cards[i].Draw(centralX + cardX, cardY, front);

                var card = Cards[i].card;
                int radius = 15;
                Raylib.DrawCircleLines(centralX + cardX + radius, cardY + radius, radius, Color.Black);
                if (int.TryParse(card.Duration, System.Globalization.NumberStyles.Integer, null, out int totalDuration))
                {
                    float relativePassed = card.remainingDuration / (totalDuration * 1000f);
                    int degrees = (int)(relativePassed * 360);
                    Raylib.DrawCircleSector(new(centralX + cardX + radius, cardY + radius), radius, 0, degrees, 100, Color.Black);
                }
                else
                {
                    Raylib.DrawCircleSector(new(centralX + cardX + radius, cardY + radius), radius, 0, 360, 100, Color.Black);
                }
            }
        }
    }
}
