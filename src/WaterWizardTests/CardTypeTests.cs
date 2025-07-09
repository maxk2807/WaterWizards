// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 51 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using Xunit;
using WaterWizard.Shared;

namespace WaterWizardTests
{
    public class CardTypeTests
    {
        [Theory]
        [InlineData(CardType.Damage)]
        [InlineData(CardType.Utility)]
        [InlineData(CardType.Environment)]
        [InlineData(CardType.Healing)]
        public void CardType_AllValidTypes_CanBeAssigned(CardType cardType)
        {
            // Arrange & Act
            var type = cardType;
            
            // Assert
            Assert.True(Enum.IsDefined(typeof(CardType), type));
        }

        [Fact]
        public void CardType_GetCardsOfType_ReturnsCorrectType()
        {
            // Arrange
            var targetType = CardType.Damage;
            
            // Act
            var cards = Cards.GetCardsOfType(targetType);
            
            // Assert
            Assert.All(cards, card => Assert.Equal(targetType, card.Type));
        }

        [Fact]
        public void CardType_GetCardsOfType_ReturnsNonEmptyList()
        {
            // Arrange & Act
            var damageCards = Cards.GetCardsOfType(CardType.Damage);
            var utilityCards = Cards.GetCardsOfType(CardType.Utility);
            var environmentCards = Cards.GetCardsOfType(CardType.Environment);
            var healingCards = Cards.GetCardsOfType(CardType.Healing);
            
            // Assert
            Assert.NotEmpty(damageCards);
            Assert.NotEmpty(utilityCards);
            Assert.NotEmpty(environmentCards);
            Assert.NotEmpty(healingCards);
        }
    }
}