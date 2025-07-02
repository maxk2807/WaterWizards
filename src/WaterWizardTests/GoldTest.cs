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
            var gold = new Gold();

            // Assert
            Assert.Equal(0, gold.Amount);
        }

        [Fact]
        public void Gold_Update_IncreasesAmountBasedOnMerchantShips()
        {
            // Arrange
            var gold = new Gold();

            // Act
            gold.Update(2.0, 3); // 2 Sekunden vergangen, 3 Handelsschiffe

            // Assert
            Assert.Equal(4, gold.Amount); // 1 Basis + 3 von Handelsschiffen
        }

        [Fact]
        public void Gold_Update_DoesNotIncreaseAmountBeforeTickInterval()
        {
            // Arrange
            var gold = new Gold();

            // Act
            gold.Update(1.0, 2); // Nur 1 Sekunde vergangen, 2 Handelsschiffe

            // Assert
            Assert.Equal(0, gold.Amount); // Noch kein Gold generiert
        }

        [Fact]
        public void Gold_Spend_DecreasesAmount_WhenEnoughGold()
        {
            // Arrange
            var gold = new Gold();
            gold.Update(2.0, 2); // 3 Gold generieren (1 Basis + 2 von Schiffen)

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
            var gold = new Gold();

            // Act
            var result = gold.Spend(5);

            // Assert
            Assert.False(result);
            Assert.Equal(0, gold.Amount);
        }
    }
}
