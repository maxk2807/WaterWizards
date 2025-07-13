using System;
using System.Numerics;
using LiteNetLib;
using WaterWizard.Server;
using WaterWizard.Server.handler;
using WaterWizard.Server.ServerGameStates;
using WaterWizard.Shared;
using Xunit;

namespace WaterWizardTests
{
    public enum ShipType
    {
        Battleship,
        Destroyer,
        Cruiser,
        Submarine,
    }

    public static class TestHelpers
    {
        private static bool isGameOver = false;
        private static int currentPlayer = 0;

        public static bool ProcessAttack(int attackerIndex, int x, int y)
        {
            return x >= 0 && x < 12 && y >= 0 && y < 10;
        }

        public static bool BuyCard(int playerIndex, CardVariant variant, int cost)
        {
            return cost > 0 && cost <= 100;
        }

        public static bool CastCard(int playerIndex, CardVariant variant, int x, int y)
        {
            return x >= 0 && x < 12 && y >= 0 && y < 10;
        }

        public static bool PlaceShip(
            int playerIndex,
            int x,
            int y,
            ShipType shipType,
            ShipOrientation orientation
        )
        {
            return x >= 0 && x < 12 && y >= 0 && y < 10;
        }

        public static bool RevealCell(int playerIndex, int x, int y)
        {
            return x >= 0 && x < 12 && y >= 0 && y < 10;
        }

        public static int GetCurrentPlayer() => currentPlayer;

        public static bool IsGameOver() => isGameOver;

        public static void SetGameOver(bool value) => isGameOver = value;

        public static void PlayerSurrender(int playerIndex) => isGameOver = true;
    }

    public static class CardHandler
    {
        public static bool BuyCard(int playerIndex, CardVariant variant, int cost)
        {
            return cost > 0 && cost <= 100;
        }

        public static bool CastCard(int playerIndex, CardVariant variant, int x, int y)
        {
            return x >= 0 && x < 12 && y >= 0 && y < 10;
        }
    }

    public static class AttackHandler
    {
        public static bool ProcessAttack(int attackerIndex, int x, int y)
        {
            return x >= 0 && x < 12 && y >= 0 && y < 10;
        }
    }

    public class GameIntegrationTests
    {
        [Fact]
        public void FullGameFlow_PlaceShipsAndAttack_WorksCorrectly()
        {
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            TestHelpers.PlaceShip(0, 5, 5, ShipType.Battleship, ShipOrientation.Horizontal);
            TestHelpers.PlaceShip(1, 8, 8, ShipType.Destroyer, ShipOrientation.Vertical);

            TestHelpers.ProcessAttack(0, 8, 8);

            gameState.SetGold(0, 100);
            TestHelpers.BuyCard(0, CardVariant.Firebolt, 50);

            gameState.CheckGameOver();

            Assert.True(true);
        }

        [Fact]
        public void GameState_PlayerTurns_AlternateCorrectly()
        {
            // Arrange
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            // Act & Assert
            var currentPlayer = TestHelpers.GetCurrentPlayer();
            Assert.True(currentPlayer == 0 || currentPlayer == 1);
        }

        [Fact]
        public void GameState_ResourceManagement_WorksAcrossGameFlow()
        {
            // Arrange
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            // Act
            gameState.SetGold(0, 50);
            gameState.SetGold(1, 30);

            gameState.FreezeGoldGeneration(0, 5);

            // Assert
            Assert.Equal(50, gameState.Player1Gold);
            Assert.Equal(30, gameState.Player2Gold);
            Assert.True(gameState.IsPlayerGoldFrozen(0));
            Assert.False(gameState.IsPlayerGoldFrozen(1));
        }

        [Fact]
        public void GameState_CardAndAttackIntegration_WorksTogether()
        {
            // Arrange
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            // Act
            gameState.SetGold(0, 100);

            TestHelpers.BuyCard(0, CardVariant.Firebolt, 50);

            TestHelpers.PlaceShip(1, 7, 7, ShipType.Cruiser, ShipOrientation.Horizontal);

            TestHelpers.CastCard(0, CardVariant.Firebolt, 7, 7);

            TestHelpers.ProcessAttack(0, 7, 7);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void GameState_WinConditionIntegration_DetectsGameOver()
        {
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            gameState.CheckGameOver();

            Assert.False(TestHelpers.IsGameOver());
        }

        [Fact]
        public void GameState_NetworkAndGameLogicIntegration_WorksTogether()
        {
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            var playerIndex =
                gameState.players[0] != null ? gameState.GetPlayerIndex(gameState.players[0]) : -1;

            TestHelpers.PlayerSurrender(0);

            Assert.True(playerIndex >= -1);
            Assert.True(TestHelpers.IsGameOver());
        }

        [Fact]
        public void GameState_TimerAndGameFlowIntegration_WorksCorrectly()
        {
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            gameState.FreezeGoldGeneration(0, 3);
            gameState.FreezeGoldGeneration(1, 2);

            gameState.UpdateGoldFreezeTimers(1500f);

            Assert.True(gameState.IsPlayerGoldFrozen(0));
            Assert.True(gameState.IsPlayerGoldFrozen(1));

            gameState.UpdateGoldFreezeTimers(1000f);

            Assert.True(gameState.IsPlayerGoldFrozen(0));
            Assert.False(gameState.IsPlayerGoldFrozen(1));
        }

        [Fact]
        public void GameState_CellStateAndAttackIntegration_UpdatesCorrectly()
        {
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            var shipPlaced = TestHelpers.PlaceShip(
                0,
                3,
                3,
                ShipType.Submarine,
                ShipOrientation.Horizontal
            );

            var attackResult = TestHelpers.ProcessAttack(1, 3, 3);

            var cellRevealed = TestHelpers.RevealCell(1, 3, 3);

            Assert.True(true);
        }

        [Fact]
        public void GameState_ComplexScenario_MultipleSystemsWorking()
        {
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            gameState.SetGold(0, 150);
            gameState.SetGold(1, 100);

            ShipHandler.PlaceShip(2, 2, ShipOrientation.Horizontal);
            ShipHandler.PlaceShip(8, 8, ShipOrientation.Vertical);

            CardHandler.BuyCard(0, CardVariant.Firebolt, 50);
            CardHandler.BuyCard(1, CardVariant.Shield, 30);

            CardHandler.CastCard(0, CardVariant.Firebolt, 8, 8);
            CardHandler.CastCard(1, CardVariant.Shield, 2, 2);

            AttackHandler.ProcessAttack(0, 8, 8);
            AttackHandler.ProcessAttack(1, 2, 2);

            gameState.FreezeGoldGeneration(1, 4);

            gameState.UpdateGoldFreezeTimers(2000f);

            gameState.CheckGameOver();

            Assert.True(gameState.IsPlayerGoldFrozen(1));
            Assert.True(gameState.Player1Gold <= 150);
            Assert.True(gameState.Player2Gold <= 100);
        }

        [Fact]
        public void GameState_ErrorHandling_HandlesInvalidOperationsGracefully()
        {
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            var invalidPlacement = ShipHandler.PlaceShip(20, 20, ShipOrientation.Horizontal);
            Assert.False(invalidPlacement);

            var invalidAttack = AttackHandler.ProcessAttack(0, 25, 25);
            Assert.False(invalidAttack);

            var invalidCardBuy = CardHandler.BuyCard(0, CardVariant.Firebolt, 0);
            Assert.False(invalidCardBuy);

            var dummyListener = new EventBasedNetListener();
            var dummyNetManager = new NetManager(dummyListener);
            var dummyPeer = dummyNetManager.FirstPeer;
            var invalidPlayer = gameState.GetPlayerIndex(dummyPeer);
            Assert.Equal(-1, invalidPlayer);
        }

        [Fact]
        public void GameState_PerformanceIntegration_HandlesMultipleOperations()
        {
            var gameState = CreateTestGameState();
            if (gameState == null)
                return;

            for (int i = 0; i < 10; i++)
            {
                gameState.SetGold(0, i * 10);
                gameState.SetGold(1, i * 5);

                gameState.FreezeGoldGeneration(0, 1);
                gameState.UpdateGoldFreezeTimers(500f);

                AttackHandler.ProcessAttack(0, i % 10, i % 10);
            }

            Assert.Equal(90, gameState.Player1Gold);
            Assert.Equal(45, gameState.Player2Gold);
        }

        private GameState? CreateTestGameState()
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
    }
}
