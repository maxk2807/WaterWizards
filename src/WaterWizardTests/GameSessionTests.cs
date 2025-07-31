// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 32 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Shared;
using Xunit;

namespace WaterWizardTests
{
    public class GameSessionTests
    {
        [Fact]
        public void GameSession_CreateNew_GeneratesUniqueId()
        {
            // Arrange & Act
            var session1 = new GameSessionId();
            var session2 = new GameSessionId();

            // Assert
            Assert.NotEqual(session1.SessionId, session2.SessionId);
        }

        [Fact]
        public void GameSession_WithCustomId_AcceptsProvidedId()
        {
            // Arrange
            var customId = "test-session-123";

            // Act
            var session = new GameSessionId(customId);

            // Assert
            Assert.Equal(customId, session.SessionId);
        }
    }
}
