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
    public void HandleNetworkEvent(NetPeer peer, NetPacketReader reader, NetManager server, ServerGameStateManager manager)
    {
    }
}
