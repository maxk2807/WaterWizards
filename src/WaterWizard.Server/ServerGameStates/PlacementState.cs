// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 88 Zeilen
// - maxk2807: 12 Zeilen
// - erick: 6 Zeilen
// - Erickk0: 6 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - private readonly Dictionary<string, bool> placementReady = new();   (jdewi001: 18 Zeilen)
// - public void OnExit() {   (jdewi001: 62 Zeilen)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;

namespace WaterWizard.Server.ServerGameStates;

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
    public void HandleNetworkEvent(
        NetPeer peer,
        NetPacketReader reader,
        NetManager serverInstance,
        ServerGameStateManager manager,
        string messageType
    )
    {
        switch (messageType)
        {
            case "PlacementReady":
                Console.WriteLine($"[PlacementState] Received PlacementReady from {peer}");
                placementReady[peer.ToString()] = true;
                if (
                    placementReady.Count == serverInstance.ConnectedPeersCount
                    && placementReady.Values.All(r => r)
                )
                {
                    Console.WriteLine(
                        "[PlacementState] All players have placed ships. Starting game."
                    );
                    ShipHandler.PrintAllShips();

                    var writer = new NetDataWriter();
                    writer.Put("StartGame");
                    foreach (var p in serverInstance.ConnectedPeerList)
                        p.Send(writer, DeliveryMethod.ReliableOrdered);

                    // Kopiere die Peers in ein Array, um Collection-Änderungen zu vermeiden
                    var peers = serverInstance.ConnectedPeerList.ToArray();

                    // ShipSync für eigene Schiffe
                    foreach (var p in peers)
                    {
                        var ships = ShipHandler.GetShips(p);
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

                    manager.ChangeState(new InGameState(serverInstance, GameState!));
                }
                else
                {
                    Console.WriteLine(
                        $"[PlacementState] Waiting for other players to place ships. {placementReady.Count}/{serverInstance.ConnectedPeersCount} ready."
                    );
                }
                break;
            case "PlaceShip":
                ShipHandler.HandleShipPlacement(peer, reader, GameState!);
                break;
            default:
                Console.WriteLine(
                    $"[PlacementState] Received unhandled message type: {messageType}"
                );
                break;
        }
    }
}
