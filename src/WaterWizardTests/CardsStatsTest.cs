// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 47 Zeilen
// - Paul: 7 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Shared;
using Xunit;

namespace WaterWizardTests
{
    public class CardStatsTests
    {
        [Fact]
        public void CardStats_DefaultValues_AreCorrect()
        {
            // Act
            var cardStats = new CardStats();

            // Assert
            Assert.Equal(0, cardStats.Mana);
            Assert.Equal(string.Empty, cardStats.CastTime);
            Assert.Equal(string.Empty, cardStats.Duration);
            Assert.Equal("Opponent - ", cardStats.Target.ToString());
        }

        [Fact]
        public void CardStats_CanSet_ValuesCorrectly()
        {
            // Arrange
            var target = new CardTarget("battlefield");

            // Act
            var cardStats = new CardStats
            {
                Mana = 5,
                CastTime = "2",
                Duration = "permanent",
                Target = target
            };

            // Assert
            Assert.Equal(5, cardStats.Mana);
            Assert.Equal("2", cardStats.CastTime);
            Assert.Equal("permanent", cardStats.Duration);
            Assert.Equal("Opponent - battlefield", cardStats.Target.ToString());
        }

        [Fact]
        public void CardStats_Target_IsInitializedCorrectly()
        {
            // Act
            var cardStats = new CardStats { Target = new CardTarget("ship") };

            // Assert
            Assert.NotNull(cardStats.Target);
            Assert.Equal("Opponent - ship", cardStats.Target.ToString());
        }
    }
}