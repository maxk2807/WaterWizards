namespace WaterWizard.Server.ServerGameStates;

using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Verwalter für die aktuellen und zukünftigen Server-Spielzustände (GameStates).
/// Leitet Netzwerkereignisse an den aktiven State weiter und steuert State-Wechsel.
/// </summary>
public class ServerGameStateManager
{
    private IServerGameState? _currentState;
    public IServerGameState? CurrentState => _currentState;
    public NetManager Server { get; }

    public ServerGameStateManager(NetManager server)
    {
        Server = server;
    }

    /// <summary>
    /// Wechselt in einen neuen Spielzustand und ruft die passenden Methoden auf.
    /// </summary>
    public void ChangeState(IServerGameState newState)
    {
        _currentState?.OnExit();
        _currentState = newState;
        _currentState.OnEnter();
    }

    /// <summary>
    /// Leitet ein Netzwerkereignis an den aktuellen State weiter.
    /// </summary>
    public void HandleNetworkEvent(NetPeer peer, NetPacketReader reader)
    {
        _currentState?.HandleNetworkEvent(peer, reader, Server, this);
    }
}
