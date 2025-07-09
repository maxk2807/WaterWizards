// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 79 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Shared;
using Xunit;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace WaterWizardTests
{
    public class CardsTest
    {
        /// <summary>
        /// Testet, ob der Cards-Konstruktor eine Exception wirft, wenn eine ungültige Kartenvariante übergeben wird.
        /// Erwartet: ArgumentException.
        /// </summary>
        [Fact]
        public void Cards_Constructor_ThrowsException_ForUnknownVariant()
        {
            Assert.Throws<ArgumentException>(() => new Cards((CardVariant)999));
        }

        /// <summary>
        /// Testet, ob der Cards-Konstruktor die Eigenschaften korrekt setzt, wenn eine gültige Kartenvariante übergeben wird.
        /// Erwartet: Richtige Zuordnung von Typ, Variante, Mana, CastTime, Duration und Target.
        /// </summary>
        [Fact]
        public void Cards_Constructor_SetsPropertiesCorrectly()
        {
            var card = new Cards(CardVariant.Firebolt);
            Assert.Equal(CardType.Damage, card.Type);
            Assert.Equal(CardVariant.Firebolt, card.Variant);
            Assert.Equal(2, card.Mana);
            Assert.Equal("instant", card.CastTime);
            Assert.Equal("instant", card.Duration);
            Assert.NotNull(card.Target);
        }

        /// <summary>
        /// Testet, ob GetCardsOfType nur Karten des angegebenen Typs zurückgibt.
        /// Erwartet: Alle zurückgegebenen Karten haben den Typ Damage.
        /// </summary>
        [Fact]
        public void GetCardsOfType_ReturnsOnlyCardsOfGivenType()
        {
            var damageCards = Cards.GetCardsOfType(CardType.Damage);
            Assert.All(damageCards, c => Assert.Equal(CardType.Damage, c.Type));
        }

        /// <summary>
        /// Testet, ob TargetAsVector die Zielgröße korrekt parst.
        /// Erwartet: Für Flächenziele (z.B. "3x3") wird die richtige Vector2 zurückgegeben, sonst (0,0).
        /// </summary>
        [Theory]
        [InlineData("3x3", 3, 3)]
        [InlineData("1x1", 1, 1)]
        [InlineData("random 1x1", 0, 0)]
        [InlineData("ship", 0, 0)]
        public void TargetAsVector_ParsesCorrectly(string target, int expectedX, int expectedY)
        {
            var card = new Cards(CardVariant.Firebolt);
            typeof(Cards).GetProperty("Target")!.SetValue(card, new CardTarget(target));
            var result = card.TargetAsVector();
            Assert.Equal(new Vector2(expectedX, expectedY), result);
        }

        /// <summary>
        /// Testet, ob der Thunder-Effekt korrekt aktiviert wird, wenn die Karte Thunder ist und ein Spielfeld gesetzt wurde.
        /// Erwartet: HasActiveEffect ist true nach Aktivierung.
        /// </summary>
        [Fact]
        public void ActivateEffect_ThunderCard_ActivatesThunderEffect()
        {
            var card = new Cards(CardVariant.Thunder);
            var battlefields = new List<Cell[,]> { new Cell[5,5] };
            card.SetBattlefieldInfo(battlefields, 5);
            card.ActivateEffect();
            Assert.True(card.HasActiveEffect);
        }
    }
}
