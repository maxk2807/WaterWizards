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
    public class Player
    {
        public string Address { get; set; }
        public string Name { get; set; } = "Player";
        public bool IsReady { get; set; } = false;

        public Player(string address)
        {
            Address = address;
        }

        public override string ToString()
        {
            return $"{Name} ({(IsReady ? "Ready" : "Not Ready")})";
        }
    }
}
