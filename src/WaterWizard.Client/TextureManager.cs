using Raylib_cs;

namespace WaterWizard.Client;

public class TextureManager{
    private static List<Texture2D> textures = [];

    public static Texture2D LoadTexture(string file){
        var texture = Raylib.LoadTexture(file);
        textures.Add(texture);
        return texture;
    }

    public static void UnloadAllTextures(){
        foreach(var texture in textures)
        {
            Raylib.UnloadTexture(texture);
        }
    }
}