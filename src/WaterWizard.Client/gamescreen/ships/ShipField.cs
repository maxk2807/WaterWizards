namespace WaterWizard.Client.gamescreen;

public class ShipField(GameScreen gameScreen)
{
    private Dictionary<DraggingShipType, int> ships = [];

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
        DraggingShipType ship = new(gameScreen, shipX, shipY, width, height, 1);
        ships.Add(ship, 1);

        shipX = X + ZonePadding / 2 + CellSize;
        shipY = Y;
        width = CellSize;
        height = CellSize * 4;
        ship = new(gameScreen, shipX, shipY, width, height, 2);
        ships.Add(ship, 2);
        
        shipX = X + ZonePadding + CellSize * 2;
        //same shipY
        width = CellSize;
        height = CellSize * 3;
        ship = new(gameScreen, shipX, shipY, width, height, 2);
        ships.Add(ship, 2);
        
        //same shipX
        shipY = Y + CellSize * 3 + ZonePadding / 2;
        width = CellSize;
        height = CellSize * 2;
        ship = new(gameScreen, shipX, shipY, width, height, 4);
        ships.Add(ship, 4);
        
        shipX = X + ZonePadding / 2 + CellSize;
        shipY = Y + CellSize * 4 + ZonePadding / 2;
        width = CellSize;
        height = CellSize;
        ship = new(gameScreen, shipX, shipY, width, height, 5);
        ships.Add(ship, 5);
        Console.WriteLine(ships.Count);
    }

    public void Draw()
    {
        foreach (var pair in ships)
        {
            pair.Key.Draw();
        }
    }
}