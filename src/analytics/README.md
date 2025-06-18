# 📊 WaterWizards Code Analytics System

Ein intelligentes Code-Analytics-System für das WaterWizards-Projekt, das automatisch bei Git-Operationen ausgeführt wird und detaillierte Repository-Statistiken generiert.

## 🚀 Features

### 📈 Umfassende Statistiken
- **Git-Statistiken**: Commits, Branches, Entwickler-Aktivität
- **Code-Statistiken**: Zeilen, Dateien, Komplexitäts-Metriken
- **Entwickler-Statistiken**: Individuelle Beiträge und Aktivität
- **Qualitäts-Metriken**: Code-Qualität und Projekt-Gesundheit

### 🔄 Automatische Ausführung
- **Pre-Commit Hook**: Vor jedem Commit
- **Pre-Merge Hook**: Vor jedem Merge
- **Pre-Receive Hook**: Auf dem Server vor dem Empfang
- **Manuelle Ausführung**: Als Standalone-Tool

### 📊 Ausgabe-Formate
- **JSON**: Maschinenlesbar für weitere Verarbeitung
- **Markdown**: Lesbar für Menschen
- **Konsolen-Ausgabe**: Zusammenfassung mit Emojis

## 🛠️ Installation

### 1. Analytics-System bauen
```bash
cd src/analytics
dotnet build
```

### 2. Git-Hooks installieren
```bash
# Unix/Linux/macOS
chmod +x setup-hooks.sh
./setup-hooks.sh

# Windows
setup-hooks.bat
```

### 3. Manuelle Ausführung testen
```bash
cd src/analytics
dotnet run
```

## 📋 Verwendung

### Automatische Ausführung
Das System wird automatisch ausgeführt bei:
- `git commit` (pre-commit hook)
- `git merge` (pre-merge-commit hook)
- `git push` (pre-receive hook auf Server)

### Manuelle Ausführung
```bash
# Im Analytics-Verzeichnis
cd src/analytics
dotnet run

# Mit spezifischem Repository-Pfad
dotnet run /path/to/repository
```

### Hooks überspringen
```bash
git commit --no-verify
git merge --no-verify
```

## 📊 Generierte Statistiken

### Git-Statistiken
```json
{
  "gitStatistics": {
    "uncommittedChanges": 5,
    "currentBranch": "main",
    "totalCommits": 1250,
    "totalBranches": 8,
    "lastCommit": "abc123...",
    "lastCommitMessage": "feat: Neue Spielmechanik hinzugefügt",
    "lastCommitAuthor": "Max Mustermann",
    "lastCommitDate": "2024-01-15T10:30:00Z"
  }
}
```

### Code-Statistiken
```json
{
  "codeStatistics": {
    "totalFiles": 150,
    "totalLines": 12500,
    "codeLines": 9800,
    "commentLines": 1800,
    "emptyLines": 900,
    "totalSize": 512000,
    "filesByType": {
      ".cs": 120,
      ".csproj": 4,
      ".md": 15,
      ".json": 8
    },
    "cSharpFiles": 120,
    "classes": 85,
    "methods": 420,
    "interfaces": 12,
    "properties": 180
  }
}
```

### Entwickler-Statistiken
```json
{
  "developerStatistics": [
    {
      "name": "Max Mustermann",
      "firstCommit": "2023-01-01T00:00:00Z",
      "lastCommit": "2024-01-15T10:30:00Z",
      "totalCommits": 450,
      "featureCommits": 120,
      "bugFixCommits": 80,
      "refactorCommits": 60,
      "documentationCommits": 40
    }
  ]
}
```

### Qualitäts-Metriken
```json
{
  "qualityMetrics": {
    "codeToCommentRatio": 5.4,
    "emptyLinesPercentage": 7.2,
    "averageFileSize": 3413.3,
    "averageLinesPerFile": 83.3,
    "averageMethodsPerClass": 4.9,
    "averagePropertiesPerClass": 2.1,
    "totalDevelopers": 5,
    "mostActiveDeveloper": "Max Mustermann",
    "averageCommitsPerDeveloper": 250.0
  }
}
```

## 📁 Ausgabe-Dateien

Das System generiert folgende Dateien im `analytics/` Verzeichnis:

- `analytics_YYYYMMDD_HHMMSS.json` - Zeitstempel-basierter JSON-Report
- `analytics_YYYYMMDD_HHMMSS.md` - Zeitstempel-basierter Markdown-Report
- `analytics_latest.json` - Neuester JSON-Report
- `analytics_latest.md` - Neuester Markdown-Report

## 🔧 Konfiguration

### .gitignore Einträge
Das Setup-Skript fügt automatisch folgende Einträge zur `.gitignore` hinzu:
```
# Analytics Reports
analytics/analytics_*.json
analytics/analytics_*.md
!analytics/analytics_latest.*
```

### Anpassbare Parameter
In `CodeAnalytics.cs` können Sie folgende Parameter anpassen:

```csharp
// Unterstützte Datei-Erweiterungen
private static readonly string[] CodeExtensions = { ".cs", ".csproj", ".sln", ".xml", ".json", ".md", ".yml", ".yaml" };

// Zu ignorierende Verzeichnisse
private static readonly string[] IgnorePatterns = { "bin/", "obj/", ".git/", "node_modules/", ".vs/", ".idea/" };
```

## 🎯 Qualitäts-Checks

Das System kann erweitert werden um zusätzliche Qualitäts-Checks:

### Beispiel-Erweiterungen
```bash
# Mindestanzahl von Tests
if [ $(find . -name "*Test.cs" | wc -l) -lt 10 ]; then
    echo "❌ Mindestens 10 Test-Dateien erforderlich"
    exit 1
fi

# Code-Coverage prüfen
if [ "$COVERAGE" -lt 80 ]; then
    echo "❌ Mindestens 80% Code-Coverage erforderlich"
    exit 1
fi

# Linting-Ergebnisse prüfen
if ! dotnet format --verify-no-changes; then
    echo "❌ Code-Formatierung erforderlich"
    exit 1
fi
```

## 🚀 CI/CD Integration

### GitHub Actions Beispiel
```yaml
name: Code Analytics
on: [push, pull_request]

jobs:
  analytics:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Run Analytics
        run: |
          cd src/analytics
          dotnet run
      - name: Upload Analytics Report
        uses: actions/upload-artifact@v3
        with:
          name: analytics-report
          path: analytics/
```

## 📈 Dashboard Integration

Die JSON-Reports können in Dashboards integriert werden:

### Grafana Beispiel
```json
{
  "targets": [
    {
      "expr": "waterwizards_commits_total",
      "legendFormat": "Commits"
    },
    {
      "expr": "waterwizards_lines_of_code",
      "legendFormat": "Lines of Code"
    }
  ]
}
```

## 🔍 Troubleshooting

### Häufige Probleme

1. **Analytics-Tool nicht gefunden**
   ```bash
   cd src/analytics
   dotnet build
   ```

2. **Git-Hooks funktionieren nicht**
   ```bash
   chmod +x .git/hooks/pre-commit
   chmod +x .git/hooks/pre-merge-commit
   ```

3. **Keine Berechtigungen**
   ```bash
   sudo chown -R $USER:$USER .git/hooks/
   ```

### Debug-Modus
```bash
# Detaillierte Ausgabe
cd src/analytics
dotnet run --verbosity detailed
```

## 🤝 Beitragen

### Commit-Konventionen
- `feat:` Neue Features
- `fix:` Bugfixes
- `refactor:` Code-Refactoring
- `docs:` Dokumentation
- `test:` Tests
- `chore:` Wartungsarbeiten

### Pull Request Prozess
1. Feature-Branch erstellen
2. Änderungen implementieren
3. Tests hinzufügen
4. Analytics-Report generieren
5. Pull Request erstellen

## 📄 Lizenz

Dieses Projekt ist Teil des WaterWizards-Repositories und unterliegt der gleichen Lizenz.

## 🆘 Support

Bei Fragen oder Problemen:
1. Issues im GitHub Repository erstellen
2. Dokumentation durchsuchen
3. Team-Mitglieder kontaktieren

---

*Letzte Aktualisierung: Januar 2024* 