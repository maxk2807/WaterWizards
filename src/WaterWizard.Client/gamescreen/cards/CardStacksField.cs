// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 54 Zeilen
// - justinjd00: 40 Zeilen
// - jdewi001: 14 Zeilen
// - erick: 2 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Runtime.CompilerServices;
using Raylib_cs;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamescreen.cards;

public class CardStacksField(GameScreen gameScreen)
{
    private CardStack? damageStack;
    private CardStack? environmentStack;
    private CardStack? utilityStack;
    private CardStack? healingStack;

    private int _x;
    public int X => _x;
    private int _y;
    public int Y => _y;
    private int Width;
    private int Height;

    public void Initialize()
    {
        Width = gameScreen.cardWidth;
        Height = gameScreen.cardHeight * 4 + (int)gameScreen.ZonePadding * 3; // 4 Stacks + 3 Paddings
        _x = (int)gameScreen.ZonePadding;
        _y = (gameScreen._gameStateManager.screenHeight - Height) / 2;

        // Position 1: Damage (Rot)
        int damageX = _x;
        int damageY = _y;
        damageStack = new(gameScreen, damageX, damageY);
        damageStack.InitDamage();

        // Position 2: Environment (Blau)
        int environmentX = _x;
        int environmentY = _y + (int)gameScreen.ZonePadding + gameScreen.cardHeight;
        environmentStack = new(gameScreen, environmentX, environmentY);
        environmentStack.InitEnvironment();

        // Position 3: Utility (Gelb) - unter Environment
        int utilityX = _x;
        int utilityY = _y + (int)gameScreen.ZonePadding * 2 + gameScreen.cardHeight * 2;
        utilityStack = new(gameScreen, utilityX, utilityY);
        utilityStack.InitUtility();

        // Position 4: Healing (Grün)
        int healingX = _x;
        int healingY = _y + (int)gameScreen.ZonePadding * 3 + gameScreen.cardHeight * 3;
        healingStack = new(gameScreen, healingX, healingY);
        healingStack.InitHealing();
    }

    public void Draw()
    {
        // Raylib.DrawRectangleLinesEx(new(X,Y,Width,Height), 1, Color.Black);

        damageStack!.Draw();
        environmentStack!.Draw();
        utilityStack!.Draw();
        healingStack!.Draw();

        HandleBuyingCardFromStack();
    }

    private void HandleBuyingCardFromStack()
    {
        var mousePos = Raylib.GetMousePosition();
        Rectangle damageRec = new(
            damageStack!.X,
            damageStack.Y,
            gameScreen.cardWidth,
            gameScreen.cardHeight
        );
        Rectangle environmentRec = new(
            environmentStack!.X,
            environmentStack.Y,
            gameScreen.cardWidth,
            gameScreen.cardHeight
        );
        Rectangle utilityRec = new(
            utilityStack!.X,
            utilityStack.Y,
            gameScreen.cardWidth,
            gameScreen.cardHeight
        );
        Rectangle healingRec = new(
            healingStack!.X,
            healingStack.Y,
            gameScreen.cardWidth,
            gameScreen.cardHeight
        );
        var clicked = Raylib.IsMouseButtonPressed(MouseButton.Left);
        if (Raylib.CheckCollisionPointRec(mousePos, damageRec) && clicked)
        {
            NetworkManager.RequestCardBuy("Damage");
        }
        else if (Raylib.CheckCollisionPointRec(mousePos, environmentRec) && clicked)
        {
            NetworkManager.RequestCardBuy("Environment");
        }
        else if (Raylib.CheckCollisionPointRec(mousePos, utilityRec) && clicked)
        {
            NetworkManager.RequestCardBuy("Utility");
        }
        else if (Raylib.CheckCollisionPointRec(mousePos, healingRec) && clicked)
        {
            NetworkManager.RequestCardBuy("Healing");
        }
    }
}
