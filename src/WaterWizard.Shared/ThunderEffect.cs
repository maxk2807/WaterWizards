// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 72 Zeilen
// - Erickk0: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - private readonly Random _random = new();   (jdewi001: 60 Zeilen)
// ===============================================

using System;
using System.Collections.Generic;
using System.Numerics;

namespace WaterWizard.Shared;

/// <summary>
/// Repräsentiert einen aktiven Blitzeffekt auf dem Spielfeld.
/// </summary>
public class ThunderEffect
{
    private readonly float _totalDuration = 5f; // Gesamtdauer des Effekts in Sekunden
    private readonly float _strikeInterval = 1.75f; // Intervall zwischen Donnereinschlägen
    private float _remainingDuration;
    private float _timeUntilNextStrike;
    private readonly Random _random = new();
    private readonly List<Cell[,]> _battlefields; // Liste aller Spielfelder
    private readonly int _gridSize; // Größe des Spielfelds

    public ThunderEffect(List<Cell[,]> battlefields, int gridSize)
    {
        _battlefields = battlefields;
        _gridSize = gridSize;
        _remainingDuration = _totalDuration;
        _timeUntilNextStrike = 0; // Erster Einschlag sofort
    }

    public void Update(float deltaTime)
    {
        _remainingDuration -= deltaTime;
        _timeUntilNextStrike -= deltaTime;

        if (_remainingDuration <= 0)
        {
            return; // Effekt ist beendet
        }

        if (_timeUntilNextStrike <= 0)
        {
            CreateThunderStrike();
            _timeUntilNextStrike = _strikeInterval;
        }
    }

    private void CreateThunderStrike()
    {
        // Für jedes Spielfeld einen Donnereinschlag erzeugen
        foreach (var battlefield in _battlefields)
        {
            // Zufällige Position für den 2x2 Einschlag finden
            // Wir müssen einen Rand von 1 lassen, damit der 2x2 Bereich ins Feld passt
            int x = _random.Next(0, _gridSize - 1);
            int y = _random.Next(0, _gridSize - 1);

            // 2x2 Bereich mit Schaden markieren
            for (int dx = 0; dx < 2; dx++)
            {
                for (int dy = 0; dy < 2; dy++)
                {
                    var cell = battlefield[x + dx, y + dy];
                    if (cell.CellState != CellState.Hit) // Nur treffen wenn noch nicht getroffen
                    {
                        cell.CellState = CellState.Hit;
                    }
                }
            }
        }
    }

    public bool IsActive => _remainingDuration > 0;

    public Vector2 GetLastStrikePosition() // Optional: Falls die UI die Position für Effekte braucht
    {
        return new Vector2(_random.Next(0, _gridSize - 1), _random.Next(0, _gridSize - 1));
    }
}
