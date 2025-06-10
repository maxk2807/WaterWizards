using Raylib_cs;

namespace WaterWizard.Client.gamescreen;

public class RessourceField(GameScreen gameScreen)
{
    private float Mana;
    private string manaString = "";
    private float Gold;
    private string goldString = "";

    private int X;
    private int Y;
    private int Width;
    private int Height;
    private int CellSize => gameScreen.playerBoard!.CellSize;
    private int ZonePadding => (int)gameScreen.ZonePadding;

    private Rectangle GoldRec;
    private Rectangle ManaRec;

    public void Initialize()
    {
        Mana = 0f;
        manaString = Mana.ToString() + " Mana";
        Gold = 0f;
        goldString = Gold.ToString() + " $";

        X = ZonePadding;
        Y = ZonePadding * 2 + 20; //20 is fontsize of GameTimer in GameScreen
        Width = (int)gameScreen.opponentBoard!.Position.X - ZonePadding * 2;
        Height = gameScreen.cardStacksField!.Y - 20 - ZonePadding * 3;

        int goldFontSize = 30;
        int goldWidth = Raylib.MeasureText(goldString, goldFontSize);
        int goldX = X + (int)(Width / 4f - goldWidth / 2f);
        int goldY = (int)(Y + (Height - goldFontSize) / 2f);

        GoldRec = new(goldX, goldY, goldWidth, goldFontSize);

        int manaFontSize = 30;
        int manaWidth = Raylib.MeasureText(manaString, manaFontSize);
        int manaX = X + (int)(3f * Width / 4f - manaWidth / 2f);
        int manaY = (int)(Y + (Height - manaFontSize) / 2f);

        ManaRec = new(manaX, manaY, manaWidth, manaFontSize);
    }
    
    public void SetMana(int mana)
    {
        Mana = mana;
    }

    public void ManaFieldUpdate()
    {
        manaString = Mana + " Mana";

        int manaFontSize = 30;
        int manaWidth = Raylib.MeasureText(manaString, manaFontSize);
        int manaX = X + (int)(3f * Width / 4f - manaWidth / 2f);
        int manaY = (int)(Y + (Height - manaFontSize) / 2f);

        ManaRec = new(manaX, manaY, manaWidth, manaFontSize);
    }

    public void SetGold(int gold)
    {
        Gold = gold;
    }

    public void GoldFieldUpdate()
    {
        goldString = Gold + " $";

        int goldFontSize = 30;
        int goldWidth = Raylib.MeasureText(goldString, goldFontSize);
        int goldX = X + (int)(Width / 4f - goldWidth / 2f);
        int goldY = (int)(Y + (Height - goldFontSize) / 2f);

        GoldRec = new(goldX, goldY, goldWidth, goldFontSize);
    }

    public void Draw()
    {
        Raylib.DrawText(
            goldString,
            (int)GoldRec.X,
            (int)GoldRec.Y,
            (int)GoldRec.Height,
            Color.Black
        );
        Raylib.DrawText(
            manaString,
            (int)ManaRec.X,
            (int)ManaRec.Y,
            (int)ManaRec.Height,
            Color.Black
        );
    }
}
