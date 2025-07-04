using System.Numerics;
using Raylib_cs;
using WaterWizard.Shared;

namespace WaterWizard.Client.gamescreen.cards;

public class GameCard(GameScreen gameScreen, Cards card)
{
    public Cards card = card;

    private int Width => gameScreen.cardWidth;
    private int Height => gameScreen.cardHeight;

    private static Texture2D cardTemplateAsset = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Cards/CardTemplate.png");
    private static Texture2D allyIconAsset = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Cards/Icons/ally.png");
    private static Texture2D enemyIconAsset = TextureManager.LoadTexture("src/WaterWizard.Client/Assets/Cards/Icons/enemy.png");

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
        if (front)
        {
            Rectangle cardTemplateRec = new(0, 0, cardTemplateAsset.Width, cardTemplateAsset.Height);
            Raylib.DrawTexturePro(
                cardTemplateAsset,
                cardTemplateRec,
                card,
                Vector2.Zero,
                0f,
                Color.White
            );

            string variantText = this.card.Variant.ToString();
            int variantTextWidth = Raylib.MeasureText(variantText, 10);
            Raylib.DrawText(
                variantText,
                x + (Width - variantTextWidth - 10) / 2,
                y + Height / 8 - 3,
                10,
                Color.White
            );

            string targetText = this.card.Target!.Target.ToString();
            int targetTextWidth = Raylib.MeasureText(variantText, 10);
            Raylib.DrawText(
                targetText,
                x + (Width - targetTextWidth - 10) / 2,
                y + Height * 3 / 4 - 10,
                10,
                Color.White
            );

            Rectangle allyIconRec = new(0, 0, allyIconAsset.Width, allyIconAsset.Height);
            Rectangle rec = new(x + (Width - targetTextWidth) / 2f + 30, y + Height * 3 / 4f - 10, 16, 16);
            Raylib.DrawTexturePro(
                allyIconAsset,
                allyIconRec,
                rec,
                Vector2.Zero,
                0f,
                Color.White
            );
        }
        else
        {
            Raylib.DrawRectangleRec(card, GetColorFromCardType(front));
            Raylib.DrawRectangleLinesEx(card, 2, Color.Black);

            string cardBackText = this.card.Type.ToString();
            int textWidth = Raylib.MeasureText(cardBackText, 10);
            Raylib.DrawText(
                cardBackText,
                x + (Width - textWidth) / 2,
                y + (Height - 10) / 2,
                10,
                Color.Black
            );
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
        if (!front)
        {
            string cardBackText = this.card.Type.ToString();
            int textWidth = Raylib.MeasureText(cardBackText, 10);
            Raylib.DrawTextPro(
                Raylib.GetFontDefault(),
                cardBackText,
                new(x + (Width - textWidth) / 2f, y + (Height - 10) / 2f),
                new(textWidth, 10),
                rot,
                10,
                Raylib.GetFontDefault().GlyphPadding,
                Color.Black
            );
        }
    }

    private Color GetColorFromCardType(bool front)
    {
        return front
            ? (
                card.Type == CardType.Damage ? new(238, 156, 156)
                : card.Type == CardType.Environment ? new(210, 152, 255)
                : card.Type == CardType.Utility ? new(255, 255, 153)
                : new(149, 251, 215)
            )
            : (
                card.Type == CardType.Damage ? Color.Red
                : card.Type == CardType.Environment ? Color.Blue
                : card.Type == CardType.Utility ? Color.Yellow
                : Color.Green
            );
    }
}
