using System;
using System.Collections.Generic;

namespace WaterWizard.Server.Models
{
    /// <summary>
    /// Ein einzelner Eintrag des Spielzustands eines Spielers zu einem bestimmten Zeitpunkt.
    /// </summary>
    public class PlayerData
    {
        /// <summary>
        /// Zeitpunkt der Erfassung dieses Eintrags (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Der aktuelle Mana-Wert.
        /// </summary>
        public int Mana { get; set; }

        /// <summary>
        /// Der aktuelle Gold-Wert.
        /// </summary>
        public int Gold { get; set; }

        /// <summary>
        /// Liste der Kartennamen, die der Spieler aktuell besitzt.
        /// </summary>
        public List<string> Cards { get; set; } = new();

        /// <summary>
        /// Liste der Schiffe, die der Spieler auf Lager hat.
        /// </summary>
        public List<string> ShipsInStock { get; set; } = new();
    }
}