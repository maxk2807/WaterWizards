# Stein-Implementierung für WaterWizards

## Übersicht

Diese Implementierung fügt zufällig generierte Steine zum Spielfeld hinzu, die das Platzieren von Schiffen verhindern. Die Steine werden intelligent platziert, um das Spiel interessanter und strategischer zu machen.

## Komponenten

### 1. RockFactory (`src/WaterWizard.Server/handler/RockFactory.cs`)
- **Zweck**: Intelligente Generierung von Steinen auf dem Spielfeld
- **Funktionen**:
  - Generiert 6 zufällige Steine pro Spieler
  - Platziert Steine in strategischen Zonen (Zentrum, Ecken, Ränder)
  - Verhindert zu nahe Platzierung von Steinen
  - Validiert verfügbare Positionen

### 2. RockHandler (`src/WaterWizard.Server/handler/RockHandler.cs`)
- **Zweck**: Verwaltung und Synchronisation der Steine
- **Funktionen**:
  - Generiert und synchronisiert Steine mit Clients
  - Überprüft Kollisionen mit Steinen
  - Entfernt Steine beim Reset
  - Verwaltet Stein-Positionen

### 3. HandleRocks (`src/WaterWizard.Client/gamescreen/handler/HandleRocks.cs`)
- **Zweck**: Client-seitige Behandlung der Stein-Synchronisation
- **Funktionen**:
  - Empfängt Stein-Positionen vom Server
  - Aktualisiert das Spielfeld mit Steinen
  - Behandelt Fehler bei der Synchronisation

## Strategische Steinplatzierung

Die RockFactory platziert Steine in folgenden strategischen Zonen:

1. **Zentrale Zone** (3x3): Verhindert einfache diagonale Schüsse
2. **Ecken-Zonen** (2x2): Verhindert einfache Eckenschüsse
3. **Rand-Zonen** (2x3 oder 3x2): Verhindert einfache Randschüsse

## Integration

### Server-Seite
- Steine werden beim GameState-Start generiert
- ShipHandler berücksichtigt Steine bei der Schiffsplatzierung
- Stein-Synchronisation erfolgt über "RockSync" Nachrichten

### Client-Seite
- CellState.Rock wurde zum Client hinzugefügt
- GameBoard zeigt Steine in dunkelgrauer Farbe an
- DraggingShip validiert Kollisionen mit Steinen
- NetworkManager behandelt RockSync-Nachrichten

## Verwendung

Die Steine werden automatisch beim Start eines neuen Spiels generiert und sind sofort sichtbar. Sie verhindern das Platzieren von Schiffen und machen das Spiel strategisch interessanter.

## Konfiguration

- **Anzahl Steine**: Standardmäßig 6 pro Spieler (konfigurierbar in RockFactory.GenerateRocks)
- **Mindestabstand**: 1 Zelle zwischen Steinen (konfigurierbar in IsValidRockPlacement)
- **Strategische Zonen**: Automatisch basierend auf Spielfeldgröße

## Technische Details

- Steine verwenden `CellState.Rock` im Shared und Client CellState
- Server-Validierung verhindert Schiffsplatzierung auf Steinen
- Client-Validierung zeigt visuelles Feedback bei Kollisionen
- Stein-Positionen werden über das Netzwerk synchronisiert 