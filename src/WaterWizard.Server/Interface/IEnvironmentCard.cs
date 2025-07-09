// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 48 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using LiteNetLib;
using WaterWizard.Shared;

namespace WaterWizard.Server.Interface;

public interface IEnvironmentCard
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
    /// Whether this card has special targeting rules
    /// </summary>
    bool HasSpecialTargeting { get; }

    /// <summary>
    /// Executes the environment effect of the card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates where the card is targeted</param>
    /// <param name="caster">The player casting the card</param>
    /// <param name="opponent">The opponent player</param>
    /// <returns>True if Environment Effect was dealt, false otherwise</returns>
    bool ExecuteEnvironment(
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
    /// <param name="caster">the player casting the card</param>
    /// <param name="defender">The opponent player</param>
    /// <returns>True if the target is valid, false otherwise</returns>
    bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer defender);
}