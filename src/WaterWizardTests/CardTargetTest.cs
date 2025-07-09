// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 45 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Shared;
using Xunit;

namespace WaterWizardTests
{
    public class CardTargetTest
    {
        /// <summary>
        /// Testet, ob der Standard-Konstruktor von CardTarget das Ziel als Gegner (Opponent) setzt.
        /// Erwartet: Ally ist false und Target entspricht dem übergebenen Wert.
        /// </summary>
        [Fact]
        public void CardTarget_DefaultsToOpponent()
        {
            var target = new CardTarget("ship");
            Assert.False(target.Ally);
            Assert.Equal("ship", target.Target);
        }

        /// <summary>
        /// Testet, ob der Ally-Konstruktor von CardTarget das Ziel als Verbündeten (Ally) setzt.
        /// Erwartet: Ally ist true und Target entspricht dem übergebenen Wert.
        /// </summary>
        [Fact]
        public void CardTarget_AllyConstructor_SetsAllyTrue()
        {
            var target = new CardTarget(true, "battlefield");
            Assert.True(target.Ally);
            Assert.Equal("battlefield", target.Target);
        }

        /// <summary>
        /// Testet, ob die ToString-Methode von CardTarget die korrekte String-Repräsentation zurückgibt.
        /// Erwartet: "Ally - ..." oder "Opponent - ..." je nach Konstruktor.
        /// </summary>
        [Fact]
        public void CardTarget_ToString_ReflectsAllyAndTarget()
        {
            var target = new CardTarget(true, "ship");
            Assert.Equal("Ally - ship", target.ToString());
            var target2 = new CardTarget("battlefield");
            Assert.Equal("Opponent - battlefield", target2.ToString());
        }
    }
}
