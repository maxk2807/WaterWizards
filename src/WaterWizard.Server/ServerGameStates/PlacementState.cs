namespace WaterWizard.Server.ServerGameStates;

using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Server-Spielzustand für die Schiffsplatzierungsphase.
/// Wartet auf PlacementReady von allen Spielern und startet dann das Spiel.
/// </summary>
public class PlacementState(NetManager server, ServerGameStateManager manager) : IServerGameState
{
    private readonly Dictionary<string, bool> placementReady = new();
    private readonly NetManager server = server;
    private readonly ServerGameStateManager manager = manager;
    private GameState? GameState;

    /// <summary>
    /// Sendet die Nachricht zum Start der Platzierungsphase an alle Spieler.
    /// </summary>
    public void OnEnter()
    {
        GameState = new(server, manager);
        NotifyClients();
    }

    private void NotifyClients()
    {
        var writer = new NetDataWriter();
        writer.Put("StartPlacementPhase");
        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    /// <summary>
    /// Wird beim Verlassen des States aufgerufen (hier leer).
    /// </summary>
    public void OnExit() { }

    /// <summary>
    /// Behandelt Netzwerkereignisse während der Platzierungsphase.
    /// </summary>
    public void HandleNetworkEvent(NetPeer peer, NetPacketReader reader, NetManager serverInstance, ServerGameStateManager manager, string messageType)
    {
        switch (messageType)
        {
            case "PlacementReady":
                Console.WriteLine($"[PlacementState] Received PlacementReady from {peer}");
                placementReady[peer.ToString()] = true;
                if (placementReady.Count == serverInstance.ConnectedPeersCount && placementReady.Values.All(r => r))
                {
                    Console.WriteLine("[PlacementState] All players have placed ships. Starting game.");
                    GameState!.PrintAllShips();

                    var writer = new NetDataWriter();
                    writer.Put("StartGame");
                    foreach (var p in serverInstance.ConnectedPeerList)
                        p.Send(writer, DeliveryMethod.ReliableOrdered);

                    // Kopiere die Peers in ein Array, um Collection-Änderungen zu vermeiden
                    var peers = serverInstance.ConnectedPeerList.ToArray();

                    // ShipSync für eigene Schiffe
                    foreach (var p in peers)
                    {
                        var ships = GameState!.GetShips(p);
                        var shipWriter = new NetDataWriter();
                        shipWriter.Put("ShipSync");
                        shipWriter.Put(ships.Count);
                        foreach (var ship in ships)
                        {
                            shipWriter.Put(ship.X);
                            shipWriter.Put(ship.Y);
                            shipWriter.Put(ship.Width);
                            shipWriter.Put(ship.Height);
                        }
                        p.Send(shipWriter, DeliveryMethod.ReliableOrdered);
                    }

                    // OpponentShipSync für gegnerische Schiffe
                    foreach (var p in peers)
                    {
                        var opponent = peers.FirstOrDefault(peer2 => peer2 != p);
                        if (opponent != null)
                        {
                            var oppShips = GameState!.GetShips(opponent);
                            var oppWriter = new NetDataWriter();
                            oppWriter.Put("OpponentShipSync");
                            oppWriter.Put(oppShips.Count);
                            foreach (var ship in oppShips)
                            {
                                oppWriter.Put(ship.X);
                                oppWriter.Put(ship.Y);
                                oppWriter.Put(ship.Width);
                                oppWriter.Put(ship.Height);
                            }
                            p.Send(oppWriter, DeliveryMethod.ReliableOrdered);
                        }
                    }
                    // Bestehendes GameState-Objekt weitergeben!
                    manager.ChangeState(new InGameState(serverInstance, GameState!));
                }
                else
                {
                    Console.WriteLine($"[PlacementState] Waiting for other players to place ships. {placementReady.Count}/{serverInstance.ConnectedPeersCount} ready.");
                }
                break;
            case "PlaceShip":
                GameState!.HandleShipPlacement(peer, reader);
                break;
            default:
                Console.WriteLine($"[PlacementState] Received unhandled message type: {messageType}");
                break;
        }
    }
}