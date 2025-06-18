using System.Numerics;
using Raylib_cs;

namespace WaterWizard.Client.gamescreen.cards;

public class CardStack(GameScreen gameScreen, int x, int y)
{
    public int X { get; private set; } = x;
    public int Y { get; private set; } = y;
    private List<GameCard> cards = [];

    public void InitDamage()
    {
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Firebolt)));
        cards.Add(new(gameScreen, new(Shared.CardVariant.GreedHit)));
    }

    public void InitUtility()
    {
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Heal)));
    }

    public void InitEnvironment()
    {
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Thunder)));
    }

    public void Draw()
    {
        if (cards.Count == 0)
        {
            Raylib.DrawRectangle(X, Y, gameScreen.cardWidth, gameScreen.cardHeight, Color.Brown);
        }
        else
        {
            cards.First().Draw(X, Y, false);
        }
    }
}
