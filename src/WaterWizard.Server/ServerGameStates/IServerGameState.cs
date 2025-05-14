namespace WaterWizard.Server.ServerGameStates;

using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Interface für alle Server-Spielzustände (GameStates).
/// Definiert die wichtigsten Methoden für Zustandswechsel und Eventbehandlung.
/// </summary>
public interface IServerGameState
{
    /// <summary>
    /// Wird beim Eintritt in diesen State aufgerufen.
    /// </summary>
    void OnEnter();
    /// <summary>
    /// Wird beim Verlassen dieses States aufgerufen.
    /// </summary>
    void OnExit();
    /// <summary>
    /// Behandelt Netzwerkereignisse, die im aktuellen State eintreffen.
    /// </summary>
    void HandleNetworkEvent(NetPeer peer, NetPacketReader reader, NetManager server, ServerGameStateManager manager);
}