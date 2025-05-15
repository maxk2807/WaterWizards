namespace WaterWizard.Server.ServerGameStates;

using System;
using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Server-Spielzustand für die eigentliche Spielphase (nach Platzierung).
/// </summary>
public class InGameState : IServerGameState
{
    private readonly NetManager server;
    public InGameState(NetManager server) { this.server = server; }

    private Dictionary<string, Cell[,]> player1;
    private Dictionary<string, Cell[,]> player2;
    private static readonly int boardWidthInCells = 12;
    private static readonly int boardHeightInCells = 10;

    private static Cell[,] InitBoard()
    {
        Cell[,] result = new Cell[12, 10];
        for (int i = 0; i < boardWidthInCells; i++)
        {
            for (int j = 0; j < boardHeightInCells; j++)
            {
                result[i, j] = new(CellState.Empty);
            }
        }
        return result;
    }

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
        //TODO: Client Verbindung Spielern zuweisen
        //TODO: Boards Initialisieren
        if (server.ConnectedPeerList.Count >= 2)
        {
            player1 = [];
            player1.Add(server.ConnectedPeerList[0].ToString(), InitBoard());

            player2 = [];
            player2.Add(server.ConnectedPeerList[1].ToString(), InitBoard());
        }
        //TODO: Handkarten Initialisieren
        //TODO: Graveyard Initialisieren
        //TODO: Kartenstapel Initialisieren
        //TODO: Aktive Karten Initialisieren
        //TODO: Gold und Mana Initialisieren
        //TODO: Ship Placement Phase abwarten?
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
        if (messageType == "PlaceShip")
        {
            HandleShipPlacement(peer, reader);
        }

    }

    private void HandleShipPlacement(NetPeer peer, NetPacketReader reader)
    {
        int x = reader.GetInt();
        int y = reader.GetInt();
        int width =  reader.GetInt();
        int height = reader.GetInt();
        Console.WriteLine(x+" "+y+" "+width+" "+height);
    }
}
