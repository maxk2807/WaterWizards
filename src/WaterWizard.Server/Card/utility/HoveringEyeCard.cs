// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 94 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new(2, 1);   (erick: 78 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.utility;

/// <summary>
/// Implementation of the HoveringEye utility card
/// Reveals a 2x2 area without damaging ships
/// </summary>
public class HoveringEyeCard : IUtilityCard
{
    public CardVariant Variant => CardVariant.HoveringEye;

    public Vector2 AreaOfEffect => new(2, 1);

    public bool HasSpecialTargeting => false;

    public bool ExecuteUtility(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        int startX = (int)targetCoords.X;
        int startY = (int)targetCoords.Y;

        Console.WriteLine($"[Server] HoveringEye revealing 2x2 area at ({startX}, {startY})");

        for (int dx = 0; dx < (int)AreaOfEffect.X; dx++)
        {
            for (int dy = 0; dy < (int)AreaOfEffect.Y; dy++)
            {
                int x = startX + dx;
                int y = startY + dy;

                if (x >= 0 && x < GameState.boardWidth && y >= 0 && y < GameState.boardHeight)
                {
                    var ships = ShipHandler.GetShips(opponent);
                    bool hasShip = false;

                    foreach (var ship in ships)
                    {
                        if (x >= ship.X && x < ship.X + ship.Width &&
                            y >= ship.Y && y < ship.Y + ship.Height)
                        {
                            hasShip = true;
                            break;
                        }
                    }

                    SendHoveringEyeReveal(caster, opponent, x, y, hasShip);
                }
            }
        }

        return true;
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        int boardWidth = GameState.boardWidth;
        int boardHeight = GameState.boardHeight;

        return targetCoords.X >= 0
            && targetCoords.Y >= 0
            && targetCoords.X + AreaOfEffect.X <= boardWidth
            && targetCoords.Y + AreaOfEffect.Y <= boardHeight;
    }

    /// <summary>
    /// Sends hovering eye reveal information to both players
    /// </summary>
    private static void SendHoveringEyeReveal(NetPeer caster, NetPeer opponent, int x, int y, bool hasShip)
    {
        var casterWriter = new LiteNetLib.Utils.NetDataWriter();
        casterWriter.Put("HoveringEyeReveal");
        casterWriter.Put(x);
        casterWriter.Put(y);
        casterWriter.Put(hasShip);
        casterWriter.Put(false); 
        caster.Send(casterWriter, DeliveryMethod.ReliableOrdered);

        var opponentWriter = new LiteNetLib.Utils.NetDataWriter();
        opponentWriter.Put("HoveringEyeReveal");
        opponentWriter.Put(x);
        opponentWriter.Put(y);
        opponentWriter.Put(hasShip);
        opponentWriter.Put(true); 
        opponent.Send(opponentWriter, DeliveryMethod.ReliableOrdered);

        Console.WriteLine(
            $"[Server] HoveringEye reveal sent to both players: ({x},{y}) = {(hasShip ? "ship present" : "empty")}"
        );
    }
}