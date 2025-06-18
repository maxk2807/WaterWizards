# 🚀 WaterWizards Game Launcher

Optimierte Start-Skripte für einfaches Starten von WaterWizards mit Server und zwei Client-Instanzen.

## 📋 Verfügbare Start-Skripte

### 🐧 **Unix/macOS/Linux**
- **`./start-game`** - Universelles Skript (empfohlen)
- **`./start-game.sh`** - Bash-Skript mit erweiterten Features

### 🪟 **Windows**
- **`start-game.bat`** - Batch-Skript für CMD
- **`start-game.ps1`** - PowerShell-Skript (empfohlen für Windows)

## 🎯 Verwendung

### Einfachste Methode (alle Plattformen)
```bash
./start-game
```

### Windows PowerShell (empfohlen)
```powershell
.\start-game.ps1
```

### Windows CMD
```cmd
start-game.bat
```

### Unix/macOS/Linux
```bash
./start-game.sh
```

## ✨ Features

### 🔄 **Automatische Komponenten**
- **Server**: Startet automatisch im Hintergrund
- **Client 1**: Erste Spieler-Instanz
- **Client 2**: Zweite Spieler-Instanz

### 🛡️ **Intelligente Verwaltung**
- **Build-Check**: Überprüft Build-Erfolg vor dem Start
- **Prozess-Management**: Automatisches Beenden aller Komponenten
- **Fehlerbehandlung**: Zeigt detaillierte Fehlermeldungen
- **Logging**: Speichert Logs in separaten Dateien

### 🎨 **Benutzerfreundlichkeit**
- **Farbige Ausgabe**: Übersichtliche Statusmeldungen
- **OS-Erkennung**: Automatische Plattform-Erkennung
- **Graceful Shutdown**: Sauberes Beenden mit Ctrl+C

## 📊 Was passiert beim Start

1. **🔍 Build-Check**: Überprüft, ob das Projekt erfolgreich kompiliert
2. **🖥️ Server-Start**: Startet den Game-Server im Hintergrund
3. **⏳ Wartezeit**: Kurze Pause für Server-Initialisierung
4. **👤 Client 1**: Startet erste Client-Instanz
5. **👤 Client 2**: Startet zweite Client-Instanz
6. **✅ Bereit**: Zeigt Status und Prozess-IDs an

## 🛑 Beenden des Spiels

### Unix/macOS/Linux
```bash
Ctrl+C
```

### Windows
```cmd
# Schließen Sie das Fenster oder drücken Sie Ctrl+C
```

## 📝 Logging

Die Skripte erstellen automatisch Log-Dateien:
- **`server.log`** - Server-Logs
- **`client1.log`** - Client 1 Logs
- **`client2.log`** - Client 2 Logs

## 🔧 Troubleshooting

### Build-Fehler
```bash
❌ Build failed! Please fix the errors and try again.
```
**Lösung**: Beheben Sie die Kompilierungsfehler und versuchen Sie es erneut.

### Server-Start-Fehler
```bash
❌ Server failed to start. Check server.log for details.
```
**Lösung**: Überprüfen Sie `server.log` für detaillierte Fehlermeldungen.

### Port-Konflikte
**Symptom**: Server startet nicht oder Clients können sich nicht verbinden
**Lösung**: 
- Überprüfen Sie, ob Port 7777 verfügbar ist
- Beenden Sie andere dotnet-Prozesse
- Verwenden Sie `netstat -an | grep 7777` (Unix) oder `netstat -an | findstr 7777` (Windows)

## 🎮 Spielstart

Nach erfolgreichem Start:
1. **Client 1**: Klicken Sie auf "Host Game" oder "Join Game"
2. **Client 2**: Klicken Sie auf "Join Game" und geben Sie "localhost" ein
3. **Beide Spieler**: Klicken Sie auf "Ready" und dann "Start Game"

## 🚀 Erweiterte Optionen

### Nur Server starten
```bash
dotnet run --project src/WaterWizard.Server/WaterWizard.Server.csproj
```

### Nur Client starten
```bash
dotnet run --project src/WaterWizard.Client/WaterWizard.Client.csproj
```

### Manueller Build
```bash
dotnet build WaterWizards.sln
```

## 📱 Plattform-Unterstützung

| Plattform | Empfohlenes Skript | Alternative |
|-----------|-------------------|-------------|
| macOS | `./start-game` | `./start-game.sh` |
| Linux | `./start-game` | `./start-game.sh` |
| Windows | `start-game.ps1` | `start-game.bat` |
| WSL | `./start-game` | `./start-game.sh` |
| Git Bash | `./start-game` | `./start-game.sh` |

## 🎯 Vorteile der neuen Skripte

1. **⏱️ Zeitersparnis**: Ein Befehl startet alles
2. **🛡️ Zuverlässigkeit**: Automatische Fehlerbehandlung
3. **🎨 Benutzerfreundlichkeit**: Klare Statusmeldungen
4. **🔧 Wartbarkeit**: Saubere Prozessverwaltung
5. **📊 Übersicht**: Logging und Prozess-IDs
6. **🔄 Konsistenz**: Gleiche Erfahrung auf allen Plattformen 