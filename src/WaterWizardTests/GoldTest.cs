using WaterWizard.Shared;
using Xunit;

namespace WaterWizardTests
{
    public class GoldTests
    {
        [Fact]
        public void Gold_InitialAmount_IsZero()
        {
            // Arrange & Act
            var gold = new Coin();

            // Assert
            Assert.Equal(0, gold.Amount);
        }

        [Fact]
        public void Gold_Update_IncreasesAmountBasedOnMerchantShips()
        {
            // Arrange
            var gold = new Coin();

            // Act
            gold.Update(2.0, 3); // 2 seconds elapsed, 3 merchant ships

            // Assert
            Assert.Equal(4, gold.Amount); // 1 base + 3 from merchant ships
        }

        [Fact]
        public void Gold_Update_DoesNotIncreaseAmountBeforeTickInterval()
        {
            // Arrange
            var gold = new Coin();

            // Act
            gold.Update(1.0, 2); // Only 1 second elapsed, 2 merchant ships

            // Assert
            Assert.Equal(0, gold.Amount); // No gold generated yet
        }

        [Fact]
        public void Gold_Spend_DecreasesAmount_WhenEnoughGold()
        {
            // Arrange
            var gold = new Coin();
            gold.Update(2.0, 2); // Generate 3 gold (1 base + 2 from ships)

            // Act
            var result = gold.Spend(2);

            // Assert
            Assert.True(result);
            Assert.Equal(1, gold.Amount);
        }

        [Fact]
        public void Gold_Spend_ReturnsFalse_WhenNotEnoughGold()
        {
            // Arrange
            var gold = new Coin();

            // Act
            var result = gold.Spend(5);

            // Assert
            Assert.False(result);
            Assert.Equal(0, gold.Amount);
        }
    }
}
