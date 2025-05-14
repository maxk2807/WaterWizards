using System;
using System.Threading;

namespace WaterWizard.Server.Services;

/// <summary>
/// Führt regelmäßig Mana-Updates durch.
/// </summary>
public class PeriodicManaUpdater : IDisposable
{
    private readonly Timer _timer;
    private readonly ManaUpdateService _manaService;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);

    public PeriodicManaUpdater(ManaUpdateService manaService)
    {
        _manaService = manaService;
        _timer = new Timer(
            _ => _manaService.UpdateAllPlayers(),
            null,
            TimeSpan.Zero,
            _interval
        );
        Console.WriteLine($"[PeriodicManaUpdater] Gestartet. Intervall: {_interval.TotalSeconds}s.");
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}