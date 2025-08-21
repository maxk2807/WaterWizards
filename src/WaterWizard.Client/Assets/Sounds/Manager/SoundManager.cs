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
    public static Sound SpawnSound;
    public static Sound SweeperSound;
    public static Sound TeleportSound;
    public static Music PauseSound;
    public static Sound GreedSound;

    public static void LoadSounds()
    {
        string basePath = AppContext.BaseDirectory;
        
        WinSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/GameOver/win.wav"));
        Raylib.SetSoundVolume(WinSound, 0.5f);
        DefeatSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/GameOver/defeat2.mp3"));
        CardSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/DrawCard.wav"));
        ButtonSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/ButtonClick.wav"));
        Explosions.Add(
            Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Explosions/explosion1.wav"))
        );
        Explosions.Add(
            Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Explosions/explosion2.wav"))
        );
        Raylib.SetSoundVolume(Explosions[0], 0.5f);
        Raylib.SetSoundVolume(Explosions[1], 0.5f);
        MissSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/miss.wav"));
        Magic1 = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Magic/magic1.wav"));
        HealSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Heal/repair.wav"));
        FireboltSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Fire/firebolt.wav"));
        FireballSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Fire/fireball.wav"));
        ThunderSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Thunder/thunder.wav"));
        SpawnSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Spawn/spawn.wav"));
        SweeperSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Sweeper/sweeper.wav"));
        TeleportSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/Teleport/teleport.wav"));
        PauseSound = Raylib.LoadMusicStream(Path.Combine(basePath, "Assets/Sounds/Pause/pause2.mp3"));
        Raylib.SetMusicVolume(PauseSound, 0.5f);
        GreedSound = Raylib.LoadSound(Path.Combine(basePath, "Assets/Sounds/GreedSound/greedSound.wav"));
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
        Raylib.UnloadSound(SpawnSound);
        Raylib.UnloadSound(SweeperSound);
        Raylib.UnloadSound(TeleportSound);
        Raylib.UnloadMusicStream(PauseSound);
        Raylib.UnloadSound(GreedSound);
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
            CardVariant.Teleport => TeleportSound,
            CardVariant.HoveringEye => SweeperSound,
            CardVariant.SummonShip => SpawnSound,
            CardVariant.GreedHit => GreedSound,
            _ => new(),
        };
    }
}
