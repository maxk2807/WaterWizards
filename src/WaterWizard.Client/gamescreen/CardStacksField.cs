using System.Runtime.CompilerServices;
using Raylib_cs;

namespace WaterWizard.Client.gamescreen;

public class CardStacksField(GameScreen gameScreen){
    private CardStack? utilityStack;
    private CardStack? damageStack;
    private CardStack? environmentStack;

    private int _x;
    public int X => _x;
    private int _y;
    public int Y => _y;
    private int Width;
    private int Height;

    public void Initialize(){
        Width = gameScreen.cardWidth;
        Height = gameScreen.cardHeight * 3 + (int)gameScreen.ZonePadding * 2;
        _x = (int)gameScreen.ZonePadding;
        _y = (gameScreen._gameStateManager.screenHeight - Height) / 2;

        int damageX = _x;
        int damageY = _y;
        damageStack = new(gameScreen, damageX, damageY);
        damageStack.InitDamage();

        int utilityX = _x;
        int utilityY = _y + (int)gameScreen.ZonePadding + gameScreen.cardHeight;
        utilityStack = new(gameScreen, utilityX, utilityY);
        utilityStack.InitUtility();

        int environmentX = _x;
        int environmentY = _y + (int)gameScreen.ZonePadding * 2 + gameScreen.cardHeight * 2;
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