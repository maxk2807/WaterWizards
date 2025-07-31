// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 17 Zeilen
// - jdewi001: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;

namespace WaterWizard.Client
{
    /// <summary>
    /// Repräsentiert die Informationen einer Lobby (IP, Name, Spielerzahl).
    /// </summary>
    public class LobbyInfo
    {
        /// <summary>
        /// IP-Adresse der Lobby.
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Anzeigename der Lobby.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Anzahl der Spieler in der Lobby.
        /// </summary>
        public int PlayerCount { get; set; }

        /// <summary>
        /// Erstellt eine neue LobbyInfo-Instanz.
        /// </summary>
        /// <param name="ip">IP-Adresse</param>
        /// <param name="name">Name der Lobby</param>
        /// <param name="playerCount">Anzahl der Spieler (optional)</param>
        public LobbyInfo(string ip, string name, int playerCount = 0)
        {
            IP = ip;
            Name = name;
            PlayerCount = playerCount;
        }
    }
}
