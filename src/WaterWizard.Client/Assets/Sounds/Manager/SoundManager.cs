using Raylib_cs;
namespace WaterWizard.Client.Assets.Sounds.Manager;

public static class SoundManager
{
    public static Sound CardSound;
    public static Sound ButtonSound;

    public static void LoadSounds()
    {
        CardSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/DrawCard.wav");
        ButtonSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/ButtonClick.wav");
    }

    public static void UnloadSounds()
    {
        Raylib.UnloadSound(CardSound);
        Raylib.UnloadSound(ButtonSound);
    }
}