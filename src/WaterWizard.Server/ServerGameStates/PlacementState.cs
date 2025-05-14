namespace WaterWizard.Server.ServerGameStates;

using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Server-Spielzustand für die Schiffsplatzierungsphase.
/// Wartet auf PlacementReady von allen Spielern und startet dann das Spiel.
/// </summary>
public class PlacementState : IServerGameState
{
    private readonly Dictionary<string, bool> placementReady = new();
    private readonly NetManager server;

    public PlacementState(NetManager server)
    {
        this.server = server;
    }

    /// <summary>
    /// Sendet die Nachricht zum Start der Platzierungsphase an alle Spieler.
    /// </summary>
    public void OnEnter()
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
    public void HandleNetworkEvent(NetPeer peer, NetPacketReader reader, NetManager server, ServerGameStateManager manager)
    {
        string messageType = reader.GetString();
        switch (messageType)
        {
            case "PlacementReady":
                placementReady[peer.ToString()] = true;
                if (placementReady.Count == server.ConnectedPeersCount && placementReady.Values.All(r => r))
                {
                    var writer = new NetDataWriter();
                    writer.Put("StartGame");
                    foreach (var p in server.ConnectedPeerList)
                        p.Send(writer, DeliveryMethod.ReliableOrdered);
                    manager.ChangeState(new InGameState(server));
                }
                break;
        }
    }
}
