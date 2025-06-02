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
    
    private System.Timers.Timer? manaTimer;

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
        // Mana-Timer starten
        manaTimer = new System.Timers.Timer(10_000);
        manaTimer.Elapsed += (sender, e) => UpdateMana();
        manaTimer.AutoReset = true;
        manaTimer.Start();
    }



    private void UpdateMana()
    {
        gameState.Player1Mana.Add(1);
        gameState.Player2Mana.Add(1);

        for (int i = 0; i < server.ConnectedPeersCount; i++)
        {
            var peer = server.ConnectedPeerList[i];
            var writer = new NetDataWriter();
            writer.Put("UpdateMana");
            writer.Put(i); // Spielerindex
            writer.Put(i == 0 ? gameState.Player1Mana.CurrentMana : gameState.Player2Mana.CurrentMana);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        Console.WriteLine($"[Server] Mana updated: P1={gameState.Player1Mana.CurrentMana}, P2={gameState.Player2Mana.CurrentMana}");
    }

    /// <summary>
    /// Wird beim Verlassen des States aufgerufen (hier leer).
    /// </summary>
    public void OnExit()
    { 
        manaTimer?.Stop();
        manaTimer?.Dispose();
    }

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
        }
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
