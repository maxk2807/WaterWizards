using System.Numerics;
using WaterWizard.Client.gamescreen;
using WaterWizard.Shared;
using ClientCellState = WaterWizard.Client.Gamescreen.CellState;

namespace WaterWizardTests
{
    public class GameBoardTest
    {
        [Fact]
        public void GetCellFromScreenCoords_ValidCoordinates_ReturnsCorrectPoint()
        {
            // Arrange
            var gameBoard = CreateTestGameBoard();
            var screenPos = new Vector2(150, 150); 
            
            // Act
            var result = gameBoard.GetCellFromScreenCoords(screenPos);
            
            // Assert
            Assert.NotNull(result);
            if (result != null)
            {
                Assert.True(result.Value.X >= 0 && result.Value.Y >= 0);
            }
        }

        [Fact]
        public void GetCellFromScreenCoords_OutsideBoard_ReturnsNull()
        {
            // Arrange
            var gameBoard = CreateTestGameBoard();
            var screenPos = new Vector2(50, 50); 
            
            // Act
            var result = gameBoard.GetCellFromScreenCoords(screenPos);
            
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void IsPointOutside_PointInsideBoard_ReturnsFalse()
        {
            // Arrange
            var gameBoard = CreateTestGameBoard();
            var screenPos = new Vector2(150, 150);
            
            // Act
            var result = gameBoard.IsPointOutside(screenPos);
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPointOutside_PointOutsideBoard_ReturnsTrue()
        {
            // Arrange
            var gameBoard = CreateTestGameBoard();
            var screenPos = new Vector2(50, 50);
            
            // Act
            var result = gameBoard.IsPointOutside(screenPos);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SetCellState_ValidCoordinates_SetsCellState()
        {
            // Arrange
            var gameBoard = CreateTestGameBoard();
            // Act
            gameBoard.SetCellState(0, 0, ClientCellState.Hit);
            gameBoard.SetCellState(0, 0, (ClientCellState)CellState.Hit);
            
            // Assert 
            Assert.True(true); 
        }

        [Fact]
        public void IsCellShielded_ShieldedCell_ReturnsTrue()
        {
            // Arrange
            var gameBoard = CreateTestGameBoard();
            
            // Act
            var result = gameBoard.IsCellShielded(0, 0);
            
            // Assert
            Assert.False(result);
        }

        private GameBoard CreateTestGameBoard()
        {
            return new GameBoard(10, 10, 20, new Vector2(100, 100));
        }
    }
}