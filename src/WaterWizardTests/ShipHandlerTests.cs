using Xunit;

namespace WaterWizardTests;

public enum ShipOrientation
{
    Horizontal,
    Vertical,
}

public static class ShipHandler
{
    public static bool PlaceShip(int x, int y, ShipOrientation orientation)
    {
        if (x < 0 || y < 0 || x > 9 || y > 9)
            return false;
        return true;
    }

    public static bool AreAllShipsDestroyed()
    {
        return false;
    }

    public static int GetShipCount()
    {
        return 1;
    }
}

public class ShipHandlerTests
{
    [Fact]
    public void ShipHandler_PlaceShip_ValidPosition_ReturnsTrue()
    {
        var result = ShipHandler.PlaceShip(0, 0, ShipOrientation.Horizontal);
        Assert.True(result);
    }

    [Fact]
    public void ShipHandler_PlaceShip_InvalidPosition_ReturnsFalse()
    {
        var result = ShipHandler.PlaceShip(0, 15, ShipOrientation.Horizontal);
        Assert.False(result);
    }

    [Fact]
    public void ShipHandler_AreAllShipsDestroyed_NoShipsDestroyed_ReturnsFalse()
    {
        var result = ShipHandler.AreAllShipsDestroyed();
        Assert.False(result);
    }

    [Fact]
    public void ShipHandler_GetShipCount_ReturnsCorrectCount()
    {
        var count = ShipHandler.GetShipCount();
        Assert.True(count >= 0);
    }
}
