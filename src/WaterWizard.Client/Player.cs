// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 20 Zeilen
// - jdewi001: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;

namespace WaterWizard.Client
{
    /// <summary>
    /// Repräsentiert einen Spieler im WaterWizards-Client.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Netzwerkadresse des Spielers (z.B. IP:Port).
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Anzeigename des Spielers.
        /// </summary>
        public string Name { get; set; } = "Player";

        /// <summary>
        /// Gibt an, ob der Spieler bereit ist (z.B. in der Lobby).
        /// </summary>
        public bool IsReady { get; set; } = false;

        /// <summary>
        /// Erstellt einen neuen Spieler mit der angegebenen Netzwerkadresse.
        /// </summary>
        /// <param name="address">Netzwerkadresse des Spielers</param>
        public Player(string address)
        {
            Address = address;
        }

        /// <summary>
        /// Gibt eine lesbare Repräsentation des Spielers zurück (Name und Ready-Status).
        /// </summary>
        /// <returns>String mit Name und Status</returns>
        public override string ToString()
        {
            return $"{Name} ({(IsReady ? "Ready" : "Not Ready")})";
        }
    }
}
