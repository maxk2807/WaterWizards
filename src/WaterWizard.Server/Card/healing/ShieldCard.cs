using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.healing;

/// <summary>
/// Implementation of the Shield healing card
/// Creates a protective 3x3 area that prevents damage for 6 seconds
/// </summary>
public class ShieldCard : IHealingCard
{
    public CardVariant Variant => CardVariant.Shield;

    public Vector2 AreaOfEffect => new(3, 3);

    public int BaseHealing => 0; // Shield doesn't heal, it protects

    public bool HasSpecialTargeting => false;

    public bool ExecuteHealing(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        int startX = (int)targetCoords.X;
        int startY = (int)targetCoords.Y;

        // Find the player index of the caster
        int casterIndex = -1;
        for (int i = 0; i < gameState.players.Length; i++)
        {
            if (gameState.players[i]?.Equals(caster) == true)
            {
                casterIndex = i;
                break;
            }
        }

        if (casterIndex == -1)
        {
            Console.WriteLine("[ShieldCard] Could not find caster index for Shield");
            return false;
        }

        // Create shield effect
        var shieldEffect = new ShieldEffect(targetCoords, casterIndex, 6.0f);
        
        // Add shield to game state (we'll need to add this to GameState)
        gameState.AddShieldEffect(shieldEffect);

        Console.WriteLine($"[ShieldCard] Shield created at ({startX}, {startY}) for player {casterIndex + 1}, duration: 6 seconds");

        // Send shield creation message to both players
        SendShieldCreated(gameState.players, casterIndex, startX, startY, 6.0f);

        return true;
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        int boardWidth = GameState.boardWidth;
        int boardHeight = GameState.boardHeight;

        // Check if the 3x3 area fits within the board
        return targetCoords.X >= 0
            && targetCoords.Y >= 0
            && targetCoords.X + AreaOfEffect.X <= boardWidth
            && targetCoords.Y + AreaOfEffect.Y <= boardHeight;
    }

    /// <summary>
    /// Sends shield creation notification to all players
    /// </summary>
    private static void SendShieldCreated(NetPeer[] players, int casterIndex, int x, int y, float duration)
    {
        foreach (var player in players)
        {
            if (player != null)
            {
                var writer = new NetDataWriter();
                writer.Put("ShieldCreated");
                writer.Put(casterIndex);
                writer.Put(x);
                writer.Put(y);
                writer.Put(duration);
                player.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }

        Console.WriteLine($"[ShieldCard] Shield creation notification sent to all players: ({x},{y}) duration: {duration}s");
    }

    /// <summary>
    /// Sends shield expiration notification to all players
    /// </summary>
    public static void SendShieldExpired(NetPeer[] players, int playerIndex, int x, int y)
    {
        foreach (var player in players)
        {
            if (player != null)
            {
                var writer = new NetDataWriter();
                writer.Put("ShieldExpired");
                writer.Put(playerIndex);
                writer.Put(x);
                writer.Put(y);
                player.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }

        Console.WriteLine($"[ShieldCard] Shield expiration notification sent to all players: ({x},{y})");
    }
}
