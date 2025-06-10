using System.Numerics;
using LiteNetLib;
using WaterWizard.Shared;

namespace WaterWizard.Server.Interface;

/// <summary>
/// Interface for damage cards that defines the basic structure for implementing damage effects
/// </summary>
public interface IDamageCard
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
    /// The base damage this card deals
    /// </summary>
    int BaseDamage { get; }
    
    /// <summary>
    /// Whether this card has special targeting rules
    /// </summary>
    bool HasSpecialTargeting { get; }
    
    /// <summary>
    /// Executes the damage effect of the card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates where the card is targeted</param>
    /// <param name="attacker">The player casting the card</param>
    /// <param name="defender">The player being targeted</param>
    /// <returns>True if any damage was dealt, false otherwise</returns>
    bool ExecuteDamage(GameState gameState, Vector2 targetCoords, NetPeer attacker, NetPeer defender);
    
    /// <summary>
    /// Validates if the target coordinates are valid for this card
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <param name="targetCoords">The coordinates to validate</param>
    /// <param name="defender">The player being targeted</param>
    /// <returns>True if the target is valid, false otherwise</returns>
    bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer defender);
}