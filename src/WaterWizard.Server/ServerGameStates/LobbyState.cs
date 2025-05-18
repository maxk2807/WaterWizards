namespace WaterWizard.Server.ServerGameStates;

using LiteNetLib;
using LiteNetLib.Utils;

/// <summary>
/// Server-Spielzustand für die Lobby-Phase vor Spielbeginn.
/// </summary>
public class LobbyState : IServerGameState
{
    private readonly NetManager server;
    private Timer? countdownTimer;
    private int countdownSeconds = 5;
    
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
        if (MessageType == "PlayerReady")
        {
            string playerAddress = peer.ToString();
            Program.ConnectedPlayers[playerAddress] = true;
            CheckAllPlayersReady(manager);
        }
    }

    public void CheckAllPlayersReady(ServerGameStateManager manager)
    {
        if (Program.ConnectedPlayers.Count > 0 && Program.ConnectedPlayers.Values.All(ready => ready))
        {
            Console.WriteLine("[Server] All players are ready. Transitioning to Placement State.");
            StartCountdown(manager);
        }
        else
        {
            int readyCount = Program.ConnectedPlayers.Values.Count(ready => ready);
            Console.WriteLine($"[Server] Waiting for players: {readyCount}/{Program.ConnectedPlayers.Count} ready.");
            if (countdownTimer != null)
            {
                ResetCountdown();
                Console.WriteLine("[Server] Countdown reset due to player not ready.");
            }
            if (Program.ConnectedPlayers.Count == 0)
            {
                ResetCountdown();
                Console.WriteLine("[Server] Countdown reset due to no player Connected.");
            }
        }
    }

    /// <summary>
    /// Startet einen Countdown für den Spielbeginn.
    /// </summary>
    /// <param name="manager">Der ServerGameStateManager, der den Zustand verwaltet.</param>
    /// <returns></returns>
    public void StartCountdown(ServerGameStateManager manager)
    {
        countdownSeconds = 5;
        countdownTimer?.Dispose();
        countdownTimer = new Timer(_ =>
        {
            if (countdownSeconds > 0)
            {
                BroadcastCountdown(countdownSeconds);
                countdownSeconds--;
            }
            else
            {
                countdownTimer?.Dispose();
                Console.WriteLine("[Server] Countdown finished. Starting game.");
                manager.ChangeState(new PlacementState(server)); 
            }
        }, null, 0, 1000);
    }

    /// <summary>
    /// Broadcastet den Countdown an alle verbundenen Spieler.
    /// </summary>
    /// <param name="secondsLeft">Die verbleibenden Sekunden des Countdowns.</param>
    /// <returns></returns>
    private void BroadcastCountdown(int secondsLeft)
    {
        var writer = new NetDataWriter();
        writer.Put("LobbyCountdown");
        writer.Put(secondsLeft);
        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        Console.WriteLine($"[Server] Broadcasted countdown: {secondsLeft}");
    }

    /// <summary>
    /// Setzt den Countdown zurück.
    /// </summary>
    /// <returns></returns>
    public void ResetCountdown()
    {
        countdownTimer?.Dispose();
        countdownTimer = null;
        countdownSeconds = 5;

        var writer = new NetDataWriter();
        writer.Put("LobbyCountdown");
        writer.Put(0); 
        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        Console.WriteLine("[Server] Countdown timer reset and reset broadcasted.");
    }

    /// <summary>
    /// Broadcastet eine Countdown-Reset-Nachricht an alle verbundenen Spieler.
    /// </summary>
    /// <param name="server">Der NetManager-Server.</param>
    /// <returns></returns>
    private static void BroadcastCountdownReset(NetManager server)
    {
        var writer = new NetDataWriter();
        writer.Put("LobbyCountdown");
        writer.Put(0); 
        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        Console.WriteLine("[Server] Broadcasted countdown reset to all clients.");
    }
}
