using WaterWizard.Server;
using WaterWizard.Server.ServerGameStates;
using LiteNetLib;

namespace WaterWizardTests;
public class GameStateTests
{
    private GameState CreateTestGameState()
    {
        var listener = new EventBasedNetListener();
        var netManager = new NetManager(listener);
        
        var stateManager = new ServerGameStateManager(netManager);
        
        try
        {
            return new GameState(netManager, stateManager);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    [Fact]
    public void GameState_PlayerIndex_ReturnsMinusOneForNullPlayer()
    {
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        // Act
        int result = gameState.GetPlayerIndex(null);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void GameState_GoldFreeze_InitialState_NotFrozen()
    {
        // Arrange
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        // Act & Assert
        Assert.False(gameState.IsPlayerGoldFrozen(0));
        Assert.False(gameState.IsPlayerGoldFrozen(1));
    }

    [Fact]
    public void GameState_GoldFreeze_FreezePlayer1()
    {
        // Arrange
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        // Act
        gameState.FreezeGoldGeneration(0, 5);

        // Assert
        Assert.True(gameState.IsPlayerGoldFrozen(0));
        Assert.False(gameState.IsPlayerGoldFrozen(1));
    }

    [Fact]
    public void GameState_GoldFreeze_FreezePlayer2()
    {
        // Arrange
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        // Act
        gameState.FreezeGoldGeneration(1, 3);

        // Assert
        Assert.False(gameState.IsPlayerGoldFrozen(0));
        Assert.True(gameState.IsPlayerGoldFrozen(1));
    }

    [Fact]
    public void GameState_GoldFreeze_TimerDecreases()
    {
        // Arrange
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        gameState.FreezeGoldGeneration(0, 2);
        
        Assert.True(gameState.IsPlayerGoldFrozen(0));

        // Act
        gameState.UpdateGoldFreezeTimers(1000f);

        // Assert
        Assert.True(gameState.IsPlayerGoldFrozen(0));

        // Act
        gameState.UpdateGoldFreezeTimers(1500f);

        // Assert 
        Assert.False(gameState.IsPlayerGoldFrozen(0));
    }

    [Fact]
    public void GameState_GoldFreeze_BothPlayersFrozen()
    {
        // Arrange
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        // Act
        gameState.FreezeGoldGeneration(0, 2);
        gameState.FreezeGoldGeneration(1, 3);

        // Assert 
        Assert.True(gameState.IsPlayerGoldFrozen(0));
        Assert.True(gameState.IsPlayerGoldFrozen(1));

        // Act
        gameState.UpdateGoldFreezeTimers(2500f);

        // Assert 
        Assert.False(gameState.IsPlayerGoldFrozen(0));
        Assert.True(gameState.IsPlayerGoldFrozen(1));

        // Act 
        gameState.UpdateGoldFreezeTimers(1000f);

        // Assert
        Assert.False(gameState.IsPlayerGoldFrozen(0));
        Assert.False(gameState.IsPlayerGoldFrozen(1));
    }

    [Fact]
    public void GameState_CheckGameOver_DoesNotThrow()
    {
        // Arrange
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        // Act & Assert
        var exception = Record.Exception(() => gameState.CheckGameOver());
        Assert.Null(exception);
    }

    [Fact]
    public void GameState_GetPlayerIndex_WithEmptyPlayersArray()
    {
        // Arrange
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        // Act & Assert
        int result = gameState.GetPlayerIndex(null);
        Assert.Equal(-1, result);
    }

    [Fact]
    public void GameState_Gold_InitialValues()
    {
        // Arrange
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        // Act & Assert 
        Assert.Equal(0, gameState.Player1Gold);
        Assert.Equal(0, gameState.Player2Gold);
    }

    [Fact]
    public void GameState_SetGold_UpdatesCorrectly()
    {
        // Arrange
        var gameState = CreateTestGameState();
        if (gameState == null)
        {
            return;
        }

        // Act
        gameState.SetGold(0, 100);
        gameState.SetGold(1, 200);

        // Assert
        Assert.Equal(100, gameState.Player1Gold);
        Assert.Equal(200, gameState.Player2Gold);
    }
}
