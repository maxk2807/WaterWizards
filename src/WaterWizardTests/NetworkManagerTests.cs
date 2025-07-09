// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Erickk0: 59 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using Xunit;
using WaterWizard.Client.network;
using WaterWizard.Client.gamescreen;
using WaterWizard.Shared;

namespace WaterWizardTests
{
    public class NetworkManagerTest
    {
        [Fact]
        public void NetworkManager_Initialization_SetsDefaultValues()
        {
            // Arrange & Act
            var networkManager = new NetworkManager();
            
            // Assert
            Assert.NotNull(networkManager);
            Assert.False(networkManager.IsHost());
        }

        [Fact]
        public void GetConnectedPlayers_InitialState_ReturnsEmptyList()
        {
            // Arrange
            var networkManager = new NetworkManager();
            
            // Act
            var players = networkManager.GetConnectedPlayers();
            
            // Assert
            Assert.NotNull(players);
            Assert.Empty(players);
        }

        [Fact]
        public void IsClientReady_InitialState_ReturnsFalse()
        {
            // Arrange
            var networkManager = new NetworkManager();
            
            // Act
            var isReady = networkManager.IsClientReady();
            
            // Assert
            Assert.False(isReady);
        }

        [Fact]
        public void RequestCardBuy_ValidCardType_DoesNotThrow()
        {
            // Arrange
            var cardType = "Damage";
            
            // Act & Assert
            var exception = Record.Exception(() => NetworkManager.RequestCardBuy(cardType));
            Assert.Null(exception);
        }
    }
}