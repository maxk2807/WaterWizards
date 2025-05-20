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

    /// <summary>
    /// Wird beim Eintritt in die Spielphase aufgerufen.
    /// </summary>
    public void OnEnter()
    {
        /* TODO: Spielstart-Logik */
        var writer = new NetDataWriter();
        writer.Put("StartInGamePhase");
        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        //TODO: Gold und Mana Initialisieren
        //TODO: Auf Input von Clients warten?
    }

    /// <summary>
    /// Wird beim Verlassen des States aufgerufen (hier leer).
    /// </summary>
    public void OnExit() { }

    /// <summary>
    /// Behandelt Netzwerkereignisse während der Spielphase.
    /// </summary>
    public void HandleNetworkEvent(NetPeer peer, NetPacketReader reader, NetManager serverInstance, ServerGameStateManager manager, string messageType)
    {
        // Handle game-specific messages here, using messageType
        Console.WriteLine($"[InGameState] HandleNetworkEvent called for peer {peer} with messageType {messageType}. Reader position: {reader.Position}");
        switch(messageType){
            case "PlaceShip":
                HandleShipPlacement(peer, reader);
                break;
            case "BuyCard":
                HandleCardBuying(peer, reader);
                break;
            case "CastCard":
                HandleCardCasting(peer, reader);
                break;
        }
    }

    private void HandleCardCasting(NetPeer peer, NetPacketReader reader)
    {
        gameState.HandleCardCasting(peer, reader);
    }

    private void HandleCardBuying(NetPeer peer, NetPacketReader reader)
    {
        gameState.HandleCardBuying(peer, reader);
    }

    private void HandleShipPlacement(NetPeer peer, NetPacketReader reader)
    {
        gameState.HandleShipPlacement(peer, reader);
    }
}
