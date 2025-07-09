// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 44 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Shared;
using Xunit;

namespace WaterWizardTests
{
    public class GameSessionIdTest
    {
        /// <summary>
        /// Testet, ob der Standard-Konstruktor von GameSessionId eine gültige GUID erzeugt.
        /// Erwartet: SessionId ist ein gültiger GUID-String.
        /// </summary>
        [Fact]
        public void GameSessionId_DefaultConstructor_CreatesValidGuid()
        {
            var session = new GameSessionId();
            Guid guid;
            Assert.True(Guid.TryParse(session.SessionId, out guid));
        }

        /// <summary>
        /// Testet, ob der String-Konstruktor von GameSessionId die SessionId korrekt setzt.
        /// Erwartet: SessionId entspricht dem übergebenen Wert.
        /// </summary>
        [Fact]
        public void GameSessionId_StringConstructor_SetsSessionId()
        {
            var id = "test-session-id";
            var session = new GameSessionId(id);
            Assert.Equal(id, session.SessionId);
        }

        /// <summary>
        /// Testet, ob die ToString-Methode von GameSessionId die SessionId korrekt zurückgibt.
        /// Erwartet: Rückgabewert entspricht der SessionId.
        /// </summary>
        [Fact]
        public void GameSessionId_ToString_ReturnsSessionId()
        {
            var id = "abc-123";
            var session = new GameSessionId(id);
            Assert.Equal(id, session.ToString());
        }
    }
}
