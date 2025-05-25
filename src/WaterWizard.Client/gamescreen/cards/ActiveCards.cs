using System.Numerics;
using Raylib_cs;

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
        _cards.AddCard(new(Shared.CardVariant.FrostBolt));
        _cards.AddCard(new(Shared.CardVariant.FrostBolt));
        _cards.AddCard(new(Shared.CardVariant.FrostBolt));
        _cards.AddCard(new(Shared.CardVariant.HoveringEye));
    }

    public void Draw()
    {
        Rectangle outerRec = new(X, Y, Width, Height);
        Raylib.DrawRectangleRec(outerRec, Color.White);
        Raylib.DrawRectangleLinesEx(outerRec, 2, Color.Black);
        Raylib.DrawText("Active Cards", (int)outerRec.X + 2, (int)outerRec.Y + 2, 15, Color.Black);

        _cards?.Draw(true);
    }

    private class ActiveCardsHand(GameScreen gameScreen, int centralX, int cardY)
        : GameHand(gameScreen, centralX, cardY)
    {
        internal override void HandleCast(GameCard gameCard) {/*Can't cast active cards*/}
    }
}
