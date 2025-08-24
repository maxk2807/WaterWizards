// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 32 Zeilen
// - jdewi001: 2 Zeilen
// - maxk2807: 1 Zeilen
// - erick: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Shared;

/// <summary>
/// Definiert alle konkreten Varianten von Karten im Spiel.
/// </summary>
public enum CardVariant
{
    // Damage Variants
    MagicAttack,
    ArcaneMissile,
    Firebolt,
    Fireball,
    GreedHit,
    FrostBolt,
    LifeSteal,

    // Utility Variants
    HoveringEye,
    SummonShip,
    Teleport,
    Paralize,
    ConeOfCold,
    Shield,

    // Environment Variants
    Thunder,
    SpawnRocks,
    RiseSun,
    CallWind,

    // Healing Variants
    Heal,
    PerfectMending,
    MassMending,
}
