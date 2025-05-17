namespace WaterWizard.Server.ServerGameStates;

using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Server-Spielzustand für die eigentliche Spielphase (nach Platzierung).
/// </summary>
public class InGameState : IServerGameState
{
    private readonly NetManager server;
    public InGameState(NetManager server) { this.server = server; }

    /// <summary>
    /// Wird beim Eintritt in die Spielphase aufgerufen.
    /// </summary>
    public void OnEnter() { /* TODO: Spielstart-Logik */ }

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
    }
}
