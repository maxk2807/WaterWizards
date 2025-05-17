namespace WaterWizard.Server.ServerGameStates;

using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Server-Spielzustand für die Lobby-Phase vor Spielbeginn.
/// </summary>
public class LobbyState : IServerGameState
{
    private readonly NetManager server;
    public LobbyState(NetManager server) { this.server = server; }

    /// <summary>
    /// Wird beim Eintritt in die Lobby aufgerufen.
    /// </summary>
    public void OnEnter() { /* TODO: Lobby-Logik */ }

    /// <summary>
    /// Wird beim Verlassen des States aufgerufen (hier leer).
    /// </summary>
    public void OnExit() { }

    /// <summary>
    /// Behandelt Netzwerkereignisse während der Lobby-Phase.
    /// </summary>
    public void HandleNetworkEvent(NetPeer peer, NetPacketReader reader, NetManager server, ServerGameStateManager manager, string MessageType)
    {
        Console.WriteLine($"[LobbyState] HandleNetworkEvent called for peer {peer} with messageType {MessageType}. Reader position: {reader.Position}");
        // TODO: Implement network event handling for the lobby
        // For example, receiving "PlayerReady" messages and then calling CheckAllPlayersReady
    }

    public void CheckAllPlayersReady(ServerGameStateManager manager)
    {
        if (Program.ConnectedPlayers.Count > 0 && Program.ConnectedPlayers.Values.All(ready => ready))
        {
            Console.WriteLine("[Server] All players are ready. Transitioning to Placement State.");
            manager.ChangeState(new PlacementState(this.server)); // Use the server instance from the LobbyState
        }
        else
        {
            int readyCount = Program.ConnectedPlayers.Values.Count(ready => ready);
            Console.WriteLine($"[Server] Waiting for players: {readyCount}/{Program.ConnectedPlayers.Count} ready.");
        }
    }
}
