// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 56 Zeilen
// - justinjd00: 40 Zeilen
// - Erickk0: 39 Zeilen
// - erick: 17 Zeilen
// - jlnhsrm: 11 Zeilen
// - jdewi001: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using Raylib_cs;

namespace WaterWizard.Client.gamescreen;

/// <summary>
/// Zeigt die Ressourcen des Spielers (Gold, Mana) sowie Statusindikatoren
/// (Paralyze, Gold-Freeze) an und verwaltet deren Aktualisierung/Positionierung.
/// </summary>
public class RessourceField(GameScreen gameScreen)
{
    private float Mana;
    private string manaString = "";
    public float Gold { private set; get; }
    private string goldString = "";
    private bool isParalized = false;
    private string paralizeString = "PARALYZED";
    private bool isGoldFrozen = false;
    private string goldFrozenString = "FROZEN";

    private int X;
    private int Y;
    private int Width;
    private int Height;
    private int CellSize => gameScreen.playerBoard!.CellSize;
    private int ZonePadding => (int)gameScreen.ZonePadding;

    private Rectangle GoldRec;
    private Rectangle ManaRec;
    private Rectangle ParalizeRec;

    /// <summary>
    /// Initialisiert Texte, Positionen und Layout-Rechtecke für Gold-, Mana- und Statusanzeigen.
    /// </summary>
    public void Initialize()
    {
        Mana = 0f;
        manaString = Mana.ToString() + " Mana";
        Gold = 0f;
        goldString = Gold.ToString() + " $";
        isParalized = false;

        X = ZonePadding;
        Y = ZonePadding * 2 + 20; //20 is fontsize of GameTimer in GameScreen
        Width = (int)gameScreen.opponentBoard!.Position.X - ZonePadding * 2;
        Height = gameScreen.cardStacksField!.Y - 20 - ZonePadding * 3;

        int goldFontSize = 30;
        int goldWidth = Raylib.MeasureText(goldString, goldFontSize);
        int goldX = X + (int)(Width / 4f - goldWidth / 2f);
        int goldY = (int)(Y + (Height - goldFontSize) / 2f);

        GoldRec = new(goldX, goldY, goldWidth, goldFontSize);

        int manaFontSize = 30;
        int manaWidth = Raylib.MeasureText(manaString, manaFontSize);
        int manaX = X + (int)(3f * Width / 4f - manaWidth / 2f);
        int manaY = (int)(Y + (Height - manaFontSize) / 2f);

        ManaRec = new(manaX, manaY, manaWidth, manaFontSize);

        // Paralize-Anzeige (rechts von Mana)
        int paralizeFontSize = 20;
        int paralizeWidth = Raylib.MeasureText(paralizeString, paralizeFontSize);
        int paralizeX = manaX + manaWidth + 20;
        int paralizeY = manaY;

        ParalizeRec = new(paralizeX, paralizeY, paralizeWidth, paralizeFontSize);
    }

    /// <summary>
    /// Setzt den aktuellen Manawert (ohne sofortige Neuberechnung der Anzeige).
    /// </summary>
    /// <param name="mana">Neuer Manawert.</param>
    public void SetMana(int mana)
    {
        Mana = mana;
    }

    /// <summary>
    /// Aktualisiert die Mana-Anzeige (Text und Position) basierend auf dem aktuellen Manawert.
    /// </summary>
    public void ManaFieldUpdate()
    {
        manaString = Mana + " Mana";

        int manaFontSize = 30;
        int manaWidth = Raylib.MeasureText(manaString, manaFontSize);
        int manaX = X + (int)(3f * Width / 4f - manaWidth / 2f);
        int manaY = (int)(Y + (Height - manaFontSize) / 2f);

        ManaRec = new(manaX, manaY, manaWidth, manaFontSize);

        // Aktualisiere auch Paralize-Position
        int paralizeFontSize = 20;
        int paralizeWidth = Raylib.MeasureText(paralizeString, paralizeFontSize);
        int paralizeX = manaX + manaWidth + 20;
        int paralizeY = manaY;

        ParalizeRec = new(paralizeX, paralizeY, paralizeWidth, paralizeFontSize);
    }

    /// <summary>
    /// Setzt den aktuellen Goldwert (ohne sofortige Neuberechnung der Anzeige).
    /// </summary>
    /// <param name="gold">Neuer Goldwert.</param>
    public void SetGold(int gold)
    {
        Gold = gold;
    }

    /// <summary>
    /// Aktualisiert die Gold-Anzeige (Text und Position) basierend auf dem aktuellen Goldwert.
    /// </summary>
    public void GoldFieldUpdate()
    {
        goldString = Gold + " $";

        int goldFontSize = 30;
        int goldWidth = Raylib.MeasureText(goldString, goldFontSize);
        int goldX = X + (int)(Width / 4f - goldWidth / 2f);
        int goldY = (int)(Y + (Height - goldFontSize) / 2f);

        GoldRec = new(goldX, goldY, goldWidth, goldFontSize);
    }

    /// <summary>
    /// Aktiviert/Deaktiviert den Paralyze-Statusindikator.
    /// </summary>
    /// <param name="paralized">True, wenn paralysiert; sonst false.</param>
    public void SetParalized(bool paralized)
    {
        Console.WriteLine($"[RessourceField] SetParalized aufgerufen: {paralized}");
        isParalized = paralized;
        Console.WriteLine($"[RessourceField] isParalized ist jetzt: {isParalized}");
    }

    /// <summary>
    /// Aktiviert/Deaktiviert den Gold-Freeze-Statusindikator.
    /// </summary>
    /// <param name="frozen">True, wenn Gold eingefroren ist; sonst false.</param>
    public void SetGoldFrozen(bool frozen)
    {
        Console.WriteLine($"[RessourceField] SetGoldFrozen aufgerufen: {frozen}");
        isGoldFrozen = frozen;
        Console.WriteLine($"[RessourceField] isGoldFrozen ist jetzt: {isGoldFrozen}");
    }

    /// <summary>
    /// Zeichnet Gold-, Mana- und ggf. Statusanzeigen (Paralyze/Gold-Freeze) auf den Bildschirm.
    /// </summary>
    public void Draw()
    {
        Raylib.DrawText(goldString, (int)GoldRec.X, (int)GoldRec.Y, 30, Color.Black);

        if (isGoldFrozen)
        {
            Console.WriteLine(
                $"[RessourceField] Zeichne blauen Gold-Freeze-Punkt - isGoldFrozen: {isGoldFrozen}"
            );
            int statusY = (int)GoldRec.Y - 25;
            int statusX = (int)GoldRec.X;

            Raylib.DrawCircle(statusX + 8, statusY + 8, 6, Color.SkyBlue);
            Raylib.DrawCircleLines(statusX + 8, statusY + 8, 6, Color.Black);

            Raylib.DrawText(goldFrozenString, statusX + 15, statusY, 15, Color.SkyBlue);
        }

        Raylib.DrawText(manaString, (int)ManaRec.X, (int)ManaRec.Y, 30, Color.Black);

        // Draw paralysis indicator if paralyzed
        if (isParalized)
        {
            Console.WriteLine(
                $"[RessourceField] Zeichne gelben Paralize-Punkt - isParalized: {isParalized}"
            );

            int statusY = (int)ManaRec.Y - 25;
            int statusX = (int)ManaRec.X;

            Raylib.DrawCircle(statusX + 8, statusY + 8, 6, Color.Yellow);
            Raylib.DrawCircleLines(statusX + 8, statusY + 8, 6, Color.Black);

            // Optional: "PARALYZED" Text anzeigen
            Raylib.DrawText(
                paralizeString,
                statusX + 15,
                statusY,
                (int)ParalizeRec.Height,
                Color.Yellow
            );
        }
    }
}
