using Raylib_cs;

namespace WaterWizard.Client.gamescreen;

public class CardStack(GameScreen gameScreen, int x, int y)
{
    private List<GameCard> cards = [
        
    ];

    public void InitDamage(){
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Firebolt)));
    }

    public void InitUtility(){
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Heal)));
    }

    public void InitEnvironment(){
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Thunder)));
    }

    public void Draw()
    {
        if (cards.Count == 0)
        {
            Raylib.DrawRectangle(x,y, gameScreen.cardWidth, gameScreen.cardHeight, Color.Brown);
        }
        else
        {
            cards.First().Draw(x,y,false);
        }
    }
}