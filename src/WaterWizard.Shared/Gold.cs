// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 49 Zeilen
// - jlnhsrm: 2 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;

namespace WaterWizard.Shared
{
    /// <summary>
    /// Represents the Gold/Coin resource used for purchasing cards.
    /// Gold is generated every 2 seconds. By default 1 gold is generated,
    /// but if you have one or more merchant ships on the board, production increases.
    /// Production = 1 (base) + number of merchant ships.
    /// </summary>
    public class Gold
    {
        public int Amount { get; private set; }

        private readonly int tickIntervalInSeconds = 2;

        private double timeAccumulator = 0;

        public Gold()
        {
            Amount = 0;
        }

        /// <summary>
        /// Call this method each frame with deltaTime in seconds and the number of merchant ships.
        /// When the accumulated time exceeds the tick interval, gold is increased accordingly.
        /// </summary>
        /// <param name="deltaTime">Elapsed time in seconds since the last update.</param>
        /// <param name="merchantShipCount">The number of merchant ships on the board.</param>
        public void Update(double deltaTime, int merchantShipCount)
        {
            timeAccumulator += deltaTime;
            if (timeAccumulator >= tickIntervalInSeconds)
            {
                int production = 1 + merchantShipCount;
                Amount += production;
                timeAccumulator = 0;
            }
        }

        public bool Spend(int cost)
        {
            if (Amount >= cost)
            {
                Amount -= cost;
                return true;
            }
            return false;
        }
    }
}
