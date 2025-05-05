using Raylib_cs;
using WaterWizard.Shared;

namespace WaterWizard.Client.gamescreen;

public class GameCard(GameScreen gameScreen, Cards card)
{
    public Cards card = card;

    private int Width => gameScreen.cardWidth;
    private int Height => gameScreen.cardHeight;

    /// <summary>
    /// Draw the individual Card at the given coordinates.
    /// The Card currently consists of the given color, and a black outline.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="front"></param>
    /// <param name="color"></param>
    public void Draw(int x, int y, bool front)
    {
        Rectangle card = new(x, y, Width, Height);
        Raylib.DrawRectangleRec(card, GetColorFromCardType(front));
        Raylib.DrawRectangleLinesEx(card, 2, Color.Black);
        if(!front){
            string cardBackText = this.card.Type.ToString();
            int textWidth = Raylib.MeasureText(cardBackText, 10);
            Raylib.DrawText(cardBackText, x+(Width-textWidth)/2, y+(Height-10)/2, 10, Color.Black);
        }
    }

    /// <summary>
    /// Draw the individual Card at the given coordinates with the given Rotation rot.
    /// The Card currently consists of the given color, and a black outline.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="front"></param>
    /// <param name="color"></param>
    /// <param name="rot"></param>
    public void DrawRotation(int x, int y, bool front, float rot)
    {
        Rectangle card = new(x, y, Width, Height);
        Raylib.DrawRectanglePro(card, new(Width, Height), rot, GetColorFromCardType(front));
        Raylib.DrawRectangleLinesEx(card, 2, Color.Black);
        if(!front){
            string cardBackText = this.card.Type.ToString();
            int textWidth = Raylib.MeasureText(cardBackText, 10);
            Raylib.DrawTextPro(Raylib.GetFontDefault(), cardBackText, new(x+(Width-textWidth)/2,y+(Height-10)/2), new(textWidth, 10), rot, 10, Raylib.GetFontDefault().GlyphPadding, Color.Black);
        }
    }



    private Color GetColorFromCardType(bool front){
        return front 
        ? (card.Type == CardType.Damage ? new(238, 156, 156) :
        card.Type == CardType.Environment ? new(210, 152, 255) : new(149, 251, 215))
        : (card.Type == CardType.Damage ? Color.Red :
        card.Type == CardType.Environment ? Color.Blue : Color.Green);
    }
}