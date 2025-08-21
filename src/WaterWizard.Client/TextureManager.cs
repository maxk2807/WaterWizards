// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 15 Zeilen
// - jdewi001: 8 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using Raylib_cs;

namespace WaterWizard.Client;

/// <summary>
/// Verwaltet das Laden und Entladen von Texturen für das Spiel.
/// </summary>
public class TextureManager
{
    private static List<Texture2D> textures = [];

    /// <summary>
    /// Lädt eine Textur aus einer Datei und speichert sie für späteres Entladen.
    /// </summary>
    /// <param name="file">Relativer Pfad zur Texturdatei (z.B. "Background/WaterWizardsMenu1200x900.png")</param>
    /// <returns>Die geladene Textur als <see cref="Texture2D"/></returns>
    public static Texture2D LoadTexture(string file)
    {
        string normalizedPath = file.Replace('/', Path.DirectorySeparatorChar);
        string assetPath = Path.Combine(AppContext.BaseDirectory, "Assets", normalizedPath);
        
        Console.WriteLine($"[TextureManager] Loading texture: {assetPath}");
        Console.WriteLine($"[TextureManager] File exists: {File.Exists(assetPath)}");
        
        var texture = Raylib.LoadTexture(assetPath);
        textures.Add(texture);
        return texture;
    }

    /// <summary>
    /// Entlädt alle zuvor geladenen Texturen aus dem Speicher.
    /// </summary>
    public static void UnloadAllTextures()
    {
        foreach (var texture in textures)
        {
            Raylib.UnloadTexture(texture);
        }
    }
}
