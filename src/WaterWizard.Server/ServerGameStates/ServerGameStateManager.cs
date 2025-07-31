// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 29 Zeilen
// - Erickk0: 9 Zeilen
// - maxk2807: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Server.ServerGameStates;

using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Verwalter für die aktuellen und zukünftigen Server-Spielzustände (GameStates).
/// Leitet Netzwerkereignisse an den aktiven State weiter und steuert State-Wechsel.
/// </summary>
public class ServerGameStateManager
{
    public IServerGameState? CurrentState { get; private set; }
    public NetManager _server;

    public ServerGameStateManager(NetManager server)
    {
        _server = server;
        CurrentState = new LobbyState(_server);
        CurrentState.OnEnter();
    }

    /// <summary>
    /// Wechselt in einen neuen Spielzustand und ruft die passenden Methoden auf.
    /// </summary>
    public void ChangeState(IServerGameState newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();
    }

    /// <summary>
    /// Leitet ein Netzwerkereignis an den aktuellen State weiter.
    /// </summary>
    public void HandleNetworkEvent(NetPeer peer, NetPacketReader reader, string messageType)
    {
        CurrentState?.HandleNetworkEvent(peer, reader, _server, this, messageType);
    }
}
