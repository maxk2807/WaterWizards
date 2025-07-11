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
    public class LobbyInfo
    {
        public string IP { get; set; }
        public string Name { get; set; }
        public int PlayerCount { get; set; }

        public LobbyInfo(string ip, string name, int playerCount = 0)
        {
            IP = ip;
            Name = name;
            PlayerCount = playerCount;
        }
    }
}
