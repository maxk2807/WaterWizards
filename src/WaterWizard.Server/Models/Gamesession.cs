using System.Collections.Generic;

namespace WaterWizard.Server.Models
{
    /// <summary>
    /// Repräsentiert eine gesamte Spielsession mit zugehörigen Spielern und deren Daten.
    /// </summary>
    public class GameSession
    {
        /// <summary>
        /// Eindeutige Kennung der Spielsession.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Sammlung aller Spieler dieser Session und deren Verlaufsdaten.
        /// </summary>
        public Dictionary<string, PlayerHistory> Players { get; set; } = new();
    }
}