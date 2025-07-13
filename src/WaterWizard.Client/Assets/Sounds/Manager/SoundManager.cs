using Raylib_cs;
using WaterWizard.Shared;
namespace WaterWizard.Client.Assets.Sounds.Manager;

public static class SoundManager
{
    public static Sound WinSound;
    public static Sound DefeatSound;
    public static Sound CardSound;
    public static Sound ButtonSound;
    public static List<Sound> Explosions { get; private set; } = [];
    public static Sound MissSound;
    public static Sound Magic1 { get; private set; }
    public static Sound HealSound;
    public static Sound FireboltSound;
    public static Sound FireballSound;
    public static Sound ThunderSound;

    public static void LoadSounds()
    {
        WinSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/GameOver/win.wav");
        Raylib.SetSoundVolume(WinSound, 0.5f);
        DefeatSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/GameOver/defeat2.mp3");
        CardSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/DrawCard.wav");
        ButtonSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/ButtonClick.wav");
        Explosions.Add(Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Explosions/explosion1.wav"));
        Explosions.Add(Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Explosions/explosion2.wav"));
        Raylib.SetSoundVolume(Explosions[0], 0.5f);
        Raylib.SetSoundVolume(Explosions[1], 0.5f);
        MissSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/miss.wav");
        Magic1 = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Magic/magic1.wav");
        HealSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Heal/repair.wav");
        FireboltSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Fire/firebolt.wav");
        FireballSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Fire/fireball.wav");
        ThunderSound = Raylib.LoadSound("src/WaterWizard.Client/Assets/Sounds/Thunder/thunder.wav");
    }

    public static void UnloadSounds()
    {
        Raylib.UnloadSound(WinSound);
        Raylib.UnloadSound(DefeatSound);
        Raylib.UnloadSound(CardSound);
        Raylib.UnloadSound(ButtonSound);
        Explosions.ForEach(Raylib.UnloadSound);
        Raylib.UnloadSound(MissSound);
        Raylib.UnloadSound(Magic1);
        Raylib.UnloadSound(HealSound);
        Raylib.UnloadSound(FireboltSound);
        Raylib.UnloadSound(FireballSound);
        Raylib.UnloadSound(ThunderSound);
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
            CardVariant.MagicAttack => Magic1,
            CardVariant.Heal => HealSound,
            CardVariant.Fireball => FireballSound,
            CardVariant.Thunder => ThunderSound,
            CardVariant.Firebolt => FireboltSound,
            _ => new(),
        };
    }
}