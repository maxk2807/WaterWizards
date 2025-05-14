using System.Collections.Generic;

namespace WaterWizard.Server.Models
{
    /// <summary>
    /// Repräsentiert die Historie eines Spielers in einer Session (alle Zustände im Verlauf).
    /// </summary>
    public class PlayerHistory
    {
        /// <summary>
        /// Name oder ID des Spielers.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Liste aller Zustandsaufzeichnungen dieses Spielers.
        /// </summary>
        public List<PlayerData> Entries { get; set; } = new();
    }
}