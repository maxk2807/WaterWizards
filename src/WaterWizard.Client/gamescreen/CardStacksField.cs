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
        Width = gameScreen.cardWidth;
        Height = (int)(gameScreen.cardHeight * 3 + gameScreen.ZonePadding * 2);
        X = (int)gameScreen.ZonePadding;
        Y = (gameScreen._gameStateManager.screenHeight - Height) / 2;

        int damageX = X;
        int damageY = Y;
        damageStack = new(gameScreen, damageX, damageY);
        damageStack.InitDamage();

        int utilityX = X;
        int utilityY = Y + (int)gameScreen.ZonePadding + gameScreen.cardHeight;
        utilityStack = new(gameScreen, utilityX, utilityY);
        utilityStack.InitUtility();

        int environmentX = X;
        int environmentY = Y + (int)gameScreen.ZonePadding * 2 + gameScreen.cardHeight * 2;
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