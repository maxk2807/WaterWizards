using Raylib_cs;
using WaterWizard.Shared;
namespace WaterWizard.Client.Assets.Sounds.Manager;

public static class SoundManager
{
    public static Sound CardSound;
    public static Sound ButtonSound;
    public static List<Sound> Explosions { get; private set; } = [];
    public static Sound MissSound;
    public static Sound HealSound;
    public static Sound FireballSound;
    public static Sound ThunderSound;

    public static void LoadSounds()
    {
        CardSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/DrawCard.wav");
        ButtonSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/ButtonClick.wav");
        Explosions.Add(Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Explosions/explosion1.wav"));
        Explosions.Add(Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Explosions/explosion2.wav"));
        Raylib.SetSoundVolume(Explosions[0], 0.5f);
        Raylib.SetSoundVolume(Explosions[1], 0.5f);
        MissSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/miss.wav");
        HealSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Heal/repair.wav");
        FireballSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Fire/fireball.wav");
        ThunderSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Thunder/thunder.wav");
    }

    public static void UnloadSounds()
    {
        Raylib.UnloadSound(CardSound);
        Raylib.UnloadSound(ButtonSound);
        Explosions.ForEach(Raylib.UnloadSound);
        Raylib.UnloadSound(MissSound);
        Raylib.UnloadSound(HealSound);
        Raylib.UnloadSound(FireballSound);
    }

    public static Sound RandomExplosion()
    {
        int i = new Random().Next(0, Explosions.Count);
        return Explosions[i];
    }

    public static Sound GetCardSound(CardVariant variant)
    {
        return variant switch
        {
            CardVariant.Heal => HealSound,
            CardVariant.Fireball => FireballSound,
            CardVariant.Thunder => ThunderSound,
            _ => new(),
        };
    }
}