using Raylib_cs;

namespace WaterWizard.Client.gamescreen;

public class CardStacksField(GameScreen gameScreen){
    private CardStack? utilityStack;
    private CardStack? damageStack;
    private CardStack? environmentStack;

    private int X;
    private int Y;
    private int Width;
    private int Height;

    public void Initialize(){
        Width = (int)(gameScreen.cardWidth * 2 + gameScreen.ZonePadding * 3);
        Height = (int)(gameScreen.cardHeight * 2 + gameScreen.ZonePadding * 3);
        X = (int)gameScreen.ZonePadding;
        Y = gameScreen._gameStateManager.screenHeight - Height - (int)gameScreen.ZonePadding;

        int utilityX = X + (int)gameScreen.ZonePadding;
        int utilityY = Y + Height / 2 - gameScreen.cardHeight/2;
        utilityStack = new(gameScreen, utilityX, utilityY);
        utilityStack.InitUtility();

        int damageX = X + (int)gameScreen.ZonePadding * 2 + gameScreen.cardWidth;
        int damageY = Y + (int)gameScreen.ZonePadding;
        damageStack = new(gameScreen, damageX, damageY);
        damageStack.InitDamage();

        int environmentX = X + (int)gameScreen.ZonePadding * 2 + gameScreen.cardWidth;
        int environmentY = Y + (int)gameScreen.ZonePadding * 2 + gameScreen.cardHeight;
        environmentStack = new(gameScreen, environmentX, environmentY);
        environmentStack.InitEnvironment();
    }

    public void Draw(){
        // Raylib.DrawRectangleLinesEx(new(X,Y,Width,Height), 1, Color.Black);

        utilityStack!.Draw();
        damageStack!.Draw();
        environmentStack!.Draw();
    }
}