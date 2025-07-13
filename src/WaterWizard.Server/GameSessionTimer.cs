// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 110 Zeilen
// - jdewi001: 10 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Diagnostics;
using LiteNetLib;
using LiteNetLib.Utils;

namespace WaterWizard.Server;

/// <summary>
/// Verwaltet den Spielzeit-Timer auf dem Server und sendet regelmäßige Updates an die Clients.
/// </summary>
public class GameSessionTimer : IDisposable
{
    /// <summary>
    /// Gibt an, ob der Timer aktuell läuft.
    /// </summary>
    public bool IsRunning => _gameStopwatch?.IsRunning ?? false;
    /// <summary>
    /// Gibt die vergangenen Sekunden seit Start des Timers zurück.
    /// </summary>
    public float ElapsedSeconds => (float)(_gameStopwatch?.Elapsed.TotalSeconds ?? 0);

    private readonly NetManager _server;
    private Stopwatch? _gameStopwatch;
    private Timer? _timerUpdateTimer;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }

    /// <summary>
    /// Initialisiert eine neue Instanz des GameSessionTimer.
    /// </summary>
    /// <param name="server">NetManager-Instanz für Netzwerk-Updates</param>
    public GameSessionTimer(NetManager server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
    }

    /// <summary>
    /// Startet den Timer und beginnt mit dem Senden von Updates an die Clients.
    /// </summary>
    public void Start()
    {
        if (IsRunning)
        {
            Log("[GameSessionTimer] Timer is already running.");
            return;
        }

        _gameStopwatch = Stopwatch.StartNew();
        Log("[GameSessionTimer] Game timer started.");

        _timerUpdateTimer?.Dispose();

        _timerUpdateTimer = new Timer(
            _ => SendTimerUpdateToAll(),
            null,
            TimeSpan.Zero,
            _updateInterval
        );
        Log($"[GameSessionTimer] Started sending updates every {_updateInterval.TotalSeconds}s.");
    }

    /// <summary>
    /// Stoppt den Timer und beendet das Senden von Updates.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
            return;

        _gameStopwatch?.Stop();
        _timerUpdateTimer?.Dispose();
        _timerUpdateTimer = null;
        _gameStopwatch = null;
        Log("[GameSessionTimer] Game timer stopped.");
    }

    /// <summary>
    /// Sendet den aktuellen Timer-Wert an einen bestimmten Peer.
    /// </summary>
    /// <param name="peer">Empfänger-Peer</param>
    public void SendCurrentTimeToPeer(NetPeer peer)
    {
        if (!IsRunning)
            return;

        var writer = new NetDataWriter();
        writer.Put("TimerUpdate");
        writer.Put(ElapsedSeconds);
        peer.Send(writer, DeliveryMethod.Unreliable);
        Log($"[GameSessionTimer] Sent current time ({ElapsedSeconds:F2}s) to joining peer {peer}");
    }

    /// <summary>
    /// Sends the current timer value to all connected peers.
    /// Called periodically by the internal timer.
    /// </summary>
    private void SendTimerUpdateToAll()
    {
        if (!IsRunning || _server == null)
            return;

        var writer = new NetDataWriter();
        writer.Put("TimerUpdate");
        writer.Put(ElapsedSeconds);

        foreach (var connectedPeer in _server.ConnectedPeerList)
        {
            connectedPeer.Send(writer, DeliveryMethod.Unreliable);
        }
        Log(
            $"[GameSessionTimer] Broadcast TimerUpdate: {ElapsedSeconds:F2}s to {_server.ConnectedPeersCount} peers."
        );
    }

    /// <summary>
    /// Gibt die Ressourcen des Timers frei.
    /// </summary>
    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
