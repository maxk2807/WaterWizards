namespace WaterWizard.Server.ServerGameStates;

using System;
using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Server-Spielzustand für die eigentliche Spielphase (nach Platzierung).
/// </summary>
public class InGameState(NetManager server, GameState gameState) : IServerGameState
{
    private readonly NetManager server = server;
    private readonly GameState gameState = gameState;

    public void OnEnter()
    {
        var writer = new NetDataWriter();
        writer.Put("StartInGamePhase");
        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    public void OnExit() { }

    public void HandleNetworkEvent(NetPeer peer, NetPacketReader reader, NetManager serverInstance, ServerGameStateManager manager, string messageType)
    {
        Console.WriteLine($"[InGameState] HandleNetworkEvent called for peer {peer} with messageType {messageType}. Reader position: {reader.Position}");
        switch (messageType)
        {
            case "PlaceShip":
                gameState.HandleShipPlacement(peer, reader);
                break;
            case "BuyCard":
                gameState.HandleCardBuying(peer, reader);
                break;
            case "CastCard":
                gameState.HandleCardCasting(peer, reader);
                break;
            case "Attack":
                int x = reader.GetInt();
                int y = reader.GetInt();
                Console.WriteLine($"[Server] Attack received at ({x}, {y}) from {peer}");
                var defender = FindOpponent(peer);
                if (defender != null)
                    gameState.HandleAttack(peer, defender, x, y);
                else
                    Console.WriteLine("[Server] Kein Gegner gefunden für Attack.");
                break;
            default:
                Console.WriteLine($"[InGameState] Unbekannter Nachrichtentyp: {messageType}");
                break;
        }
    }

    private NetPeer? FindOpponent(NetPeer attacker)
    {
        foreach (var peer in server.ConnectedPeerList)
        {
            if (!peer.Equals(attacker))
                return peer;
        }
        return null;
    }
}