// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 23 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Shared;
using Xunit;

namespace WaterWizardTests
{
    public class CellStateTests
    {
        [Theory]
        [InlineData(CellState.Unknown)]
        [InlineData(CellState.Hit)]
        [InlineData(CellState.Miss)]
        [InlineData(CellState.Ship)]
        [InlineData(CellState.Rock)]
        public void CellState_AllValidStates_CanBeAssigned(CellState state)
        {
            // Arrange & Act
            var cellState = state;

            // Assert
            Assert.True(Enum.IsDefined(typeof(CellState), cellState));
        }
    }
}
