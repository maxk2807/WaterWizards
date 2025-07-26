// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 61 Zeilen
// - jdewi001: 4 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Client.gamescreen.ships;

public class ShipField(GameScreen gameScreen)
{
    public Dictionary<DraggableShip, int> Ships = [];

    private int X;
    private int Y;
    private int Width;
    private int Height;
    private int CellSize => gameScreen.playerBoard!.CellSize;
    private int ZonePadding => (int)gameScreen.ZonePadding;

    public void Initialize()
    {
        Width = gameScreen.playerBoard!.CellSize * 3 + ZonePadding * 2;
        Height = gameScreen.playerBoard.CellSize * 5 + ZonePadding;
        X = (int)gameScreen.playerBoard.Position.X - Width;
        Y = gameScreen._gameStateManager.screenHeight - Height - ZonePadding * 2;

        int shipX = X;
        int shipY = Y + ZonePadding / 4;
        int width = CellSize;
        int height = CellSize * 5;
        DraggableShip ship = new(gameScreen, shipX, shipY, width, height, 1);
        Ships.Add(ship, 1);

        shipX = X + ZonePadding / 2 + CellSize;
        shipY = Y;
        width = CellSize;
        height = CellSize * 4;
        ship = new(gameScreen, shipX, shipY, width, height, 2);
        Ships.Add(ship, 2);

        shipX = X + ZonePadding + CellSize * 2;
        //same shipY
        width = CellSize;
        height = CellSize * 3;
        ship = new(gameScreen, shipX, shipY, width, height, 2);
        Ships.Add(ship, 2);

        //same shipX
        shipY = Y + CellSize * 3 + ZonePadding / 2;
        width = CellSize;
        height = CellSize * 2;
        ship = new(gameScreen, shipX, shipY, width, height, 4);
        Ships.Add(ship, 4);

        shipX = X + ZonePadding / 2 + CellSize;
        shipY = Y + CellSize * 4 + ZonePadding / 2;
        width = CellSize;
        height = CellSize;
        ship = new(gameScreen, shipX, shipY, width, height, 5);
        Ships.Add(ship, 5);
        Console.WriteLine(Ships.Count);
    }

    public void Draw()
    {
        foreach (var pair in Ships)
        {
            pair.Key.Draw();
        }
    }
}
