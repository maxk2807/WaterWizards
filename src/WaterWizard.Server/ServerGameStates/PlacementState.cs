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
        // messageType is already passed, no need to reader.GetString() again if Program.cs did it.
        // string messageType = reader.GetString(); // This would be wrong if Program.cs already read it.
        switch (messageType)
        {
            case "PlacementReady":
                Console.WriteLine($"[PlacementState] Received PlacementReady from {peer}");
                placementReady[peer.ToString()] = true;
                // Check if all connected players (not just those who sent PlacementReady) are ready
                if (placementReady.Count == serverInstance.ConnectedPeersCount && placementReady.Values.All(r => r))
                {
                    Console.WriteLine("[PlacementState] All players have placed ships. Starting game.");
                    var writer = new NetDataWriter();
                    writer.Put("StartGame");
                    foreach (var p in serverInstance.ConnectedPeerList) // Use serverInstance passed to this method
                        p.Send(writer, DeliveryMethod.ReliableOrdered);
                    manager.ChangeState(new InGameState(serverInstance, GameState!)); // Use serverInstance
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
