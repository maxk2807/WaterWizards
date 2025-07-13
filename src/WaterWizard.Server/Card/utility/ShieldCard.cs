// ===============================================
// Autoren-Statistik (automatisch generiert):
// - Erickk0: 99 Zeilen
// - erick: 4 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new(3, 3);   (Erickk0: 85 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.utility;

/// <summary>
/// Implementation of the Shield utility card
/// Creates a protective 3x3 area that prevents damage for 6 seconds
/// </summary>
public class ShieldCard : IUtilityCard
{
    public CardVariant Variant => CardVariant.Shield;

    public Vector2 AreaOfEffect => new(3, 3);

    public bool HasSpecialTargeting => false;

    public bool ExecuteUtility(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        int startX = (int)targetCoords.X;
        int startY = (int)targetCoords.Y;

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

        var shieldEffect = new ShieldEffect(targetCoords, casterIndex, 6.0f);

        gameState.AddShieldEffect(shieldEffect);

        Console.WriteLine(
            $"[ShieldCard] Shield created at ({startX}, {startY}) for player {casterIndex + 1}, duration: 6 seconds"
        );

        SendShieldCreated(gameState.players, casterIndex, startX, startY, 6.0f);

        return true;
    }

    public bool IsValidTarget(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        int boardWidth = GameState.boardWidth;
        int boardHeight = GameState.boardHeight;

        return targetCoords.X >= 0
            && targetCoords.Y >= 0
            && targetCoords.X + AreaOfEffect.X <= boardWidth
            && targetCoords.Y + AreaOfEffect.Y <= boardHeight;
    }

    /// <summary>
    /// Sends shield creation notification to all players
    /// </summary>
    private static void SendShieldCreated(
        NetPeer[] players,
        int casterIndex,
        int x,
        int y,
        float duration
    )
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
    }
}
