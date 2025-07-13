// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 30 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Shared
{
    /// <summary>
    /// Repräsentiert die Mana-Ressource eines Spielers.
    /// </summary>
    public class Mana
    {
        /// <summary>
        /// Der aktuelle Manawert des Spielers.
        /// </summary>
        public int CurrentMana { get; set; }

        public Mana(int initialAmount = 0)
        {
            CurrentMana = initialAmount;
        }

        public void Add(int amount)
        {
            CurrentMana += amount;
        }

        public bool Spend(int cost)
        {
            if (CurrentMana >= cost)
            {
                CurrentMana -= cost;
                return true;
            }
            return false;
        }
    }
}
