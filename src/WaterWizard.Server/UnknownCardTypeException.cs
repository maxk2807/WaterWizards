// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 9 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public UnknownCardTypeException() {   (maxk2807: 5 Zeilen)
// ===============================================

namespace WaterWizard.Server;

public class UnknownCardTypeException : Exception
{
    public UnknownCardTypeException() { }

    public UnknownCardTypeException(string? message)
        : base(message) { }
}
