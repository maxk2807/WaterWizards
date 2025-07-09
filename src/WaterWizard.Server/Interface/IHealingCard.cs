// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 53 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using LiteNetLib;
using WaterWizard.Shared;

namespace WaterWizard.Server.Interface;

public interface IHealingCard
{
    /// <summary>
    /// The variant of the card
    /// </summary>
    CardVariant Variant { get; }

    /// <summary>
    /// The area of effect as a Vector2 (width x height)
    /// </summary>
    Vector2 AreaOfEffect { get; }

    /// <summary>
    /// The base amount this card heals for
    /// </summary>
    int BaseHealing { get; }

    /// <summary>
    /// Whether this card has special targeting rules
    /// </summary>
    bool HasSpecialTargeting { get; }

    /// <summary>
    /// Executes the healing effect of the card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates where the card is targeted</param>
    /// <param name="caster">The player casting the card</param>
    /// <param name="opponent">The opponent player</param>
    /// <returns>True if any healing was done, false otherwise</returns>
    bool ExecuteHealing(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    );

    /// <summary>
    /// Validates if the target coordinates are valid for this card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates to validate</param>
    /// <param name="caster">The player casting the card</param>
    /// <param name="opponent">The opponent player</param>
    /// <returns>True if the target is valid, false otherwise</returns>
    bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent);
}