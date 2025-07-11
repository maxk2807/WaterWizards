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

public class TextureManager
{
    private static List<Texture2D> textures = [];

    public static Texture2D LoadTexture(string file)
    {
        var texture = Raylib.LoadTexture(file);
        textures.Add(texture);
        return texture;
    }

    public static void UnloadAllTextures()
    {
        foreach (var texture in textures)
        {
            Raylib.UnloadTexture(texture);
        }
    }
}
