// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 9 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public UnknownCardTypeException() {   (maxk2807: 5 Zeilen)
// ===============================================

namespace WaterWizard.Server;

/// <summary>
/// Exception, die geworfen wird, wenn ein unbekannter Kartentyp verarbeitet werden soll.
/// </summary>
public class UnknownCardTypeException : Exception
{
    /// <summary>
    /// Erstellt eine neue Instanz der UnknownCardTypeException.
    /// </summary>
    public UnknownCardTypeException() { }

    /// <summary>
    /// Erstellt eine neue Instanz der UnknownCardTypeException mit einer Fehlermeldung.
    /// </summary>
    /// <param name="message">Fehlermeldung</param>
    public UnknownCardTypeException(string? message)
        : base(message) { }
}
