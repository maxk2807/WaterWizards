// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 59 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Shared;
using Xunit;

namespace WaterWizardTests
{
    public class ManaTests
    {
        [Fact]
        public void Mana_InitialAmount_IsSetCorrectly()
        {
            // Arrange & Act
            var mana = new Mana(10);

            // Assert
            Assert.Equal(10, mana.CurrentMana);
        }

        [Fact]
        public void Mana_Add_IncreasesCurrentMana()
        {
            // Arrange
            var mana = new Mana(5);

            // Act
            mana.Add(10);

            // Assert
            Assert.Equal(15, mana.CurrentMana);
        }

        [Fact]
        public void Mana_Spend_DecreasesCurrentMana_WhenEnoughMana()
        {
            // Arrange
            var mana = new Mana(20);

            // Act
            var result = mana.Spend(15);

            // Assert
            Assert.True(result);
            Assert.Equal(5, mana.CurrentMana);
        }

        [Fact]
        public void Mana_Spend_ReturnsFalse_WhenNotEnoughMana()
        {
            // Arrange
            var mana = new Mana(10);

            // Act
            var result = mana.Spend(15);

            // Assert
            Assert.False(result);
            Assert.Equal(10, mana.CurrentMana);
        }
    }
}
