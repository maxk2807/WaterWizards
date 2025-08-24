// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 28 Zeilen
// - jdewi001: 12 Zeilen
// - justinjd00: 7 Zeilen
// - Erickk0: 1 Zeilen
// - erick: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using Raylib_cs;

namespace WaterWizard.Client.gamescreen.cards;

/// <summary>
/// Stellt einen Kartenstapel im Spielfeld dar.
/// Der Stapel kann mit verschiedenen Kartentypen (Schaden, Heilung, Utility, Umgebung) 
/// initialisiert werden und rendert entweder die oberste Karte oder 
/// ein Platzhalter-Rechteck, wenn der Stapel leer ist.
/// </summary>
public class CardStack(GameScreen gameScreen, int x, int y)
{
    /// <summary>
    /// X-Position des Kartenstapels auf dem Bildschirm.
    /// </summary>
    public int X { get; private set; } = x;

    /// <summary>
    /// Y-Position des Kartenstapels auf dem Bildschirm.
    /// </summary>
    public int Y { get; private set; } = y;
    private List<GameCard> cards = [];

    /// <summary>
    /// Y-Position des Kartenstapels auf dem Bildschirm.
    /// </summary>
    public void InitDamage()
    {
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Firebolt)));
        cards.Add(new(gameScreen, new(Shared.CardVariant.GreedHit)));
    }
    /// <summary>
    /// Initialisiert den Stapel mit Utility-Karten 
    /// (z. B. Kontroll- oder Verteidigungsfähigkeiten).
    /// </summary>
    public void InitUtility()
    {
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Paralize)));
        cards.Add(new(gameScreen, new(Shared.CardVariant.Shield)));
    }

    /// <summary>
    /// Initialisiert den Stapel mit Umgebungskarten 
    /// (z. B. Wetter- oder Feld-Effekten).
    /// </summary>
    public void InitEnvironment()
    {
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Thunder)));
    }

    /// <summary>
    /// Initialisiert den Stapel mit Heilungs-Karten.
    /// </summary>
    public void InitHealing()
    {
        //later calls to Network to get correct Cards
        cards.Add(new(gameScreen, new(Shared.CardVariant.Heal)));
    }

    /// <summary>
    /// Zeichnet den Stapel. 
    /// Wenn Karten vorhanden sind, wird die oberste Karte dargestellt.
    /// Andernfalls wird ein Platzhalter-Rechteck angezeigt.
    /// </summary>
    public void Draw()
    {
        if (cards.Count == 0)
        {
            Raylib.DrawRectangle(X, Y, gameScreen.cardWidth, gameScreen.cardHeight, Color.Brown);
        }
        else
        {
            cards.First().Draw(X, Y, false);
        }
    }
}
