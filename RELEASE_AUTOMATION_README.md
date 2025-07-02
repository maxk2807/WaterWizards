# 🚀 Automated Release & Changelog System

Automatische Generierung von GitHub Releases mit intelligenten Changelogs für WaterWizards.

## 📋 Übersicht

Das System generiert automatisch:
- **GitHub Releases** mit ausführbaren Dateien
- **Intelligente Changelogs** basierend auf Commit-Messages
- **Plattform-spezifische Builds** (Windows, macOS, Linux)
- **Automatische Versionsverwaltung**

## 🔧 Workflows

### 1. **cd-pipeline.yml** (Basis-Release)
- **Trigger**: Push auf `main` Branch
- **Features**: 
  - Automatische Builds
  - Basis-Changelog-Generierung
  - Asset-Upload

### 2. **release-with-changelog.yml** (Erweiterte Version)
- **Trigger**: Push auf `main` + Manueller Trigger
- **Features**:
  - Intelligente Commit-Kategorisierung
  - Manuelle Versionsangabe möglich
  - Detaillierte Changelogs

## 🎯 Verwendung

### Automatische Releases
```bash
# Einfach auf main pushen
git push origin main
```

### Manuelle Releases
1. Gehen Sie zu **Actions** → **Release with Changelog**
2. Klicken Sie auf **Run workflow**
3. Optional: Geben Sie eine spezifische Version ein
4. Klicken Sie auf **Run workflow**

## 📝 Commit-Konventionen

### 🎮 Game Features
```bash
feat(game): add random rock generation
feature(cards): implement new thunder ability
add(ships): new ship type with special abilities
```

### 🐛 Bug Fixes
```bash
fix(client): resolve ship placement issue
bug(server): fix memory leak
resolve(network): connection timeout fix
```

### 🛠️ Technical Improvements
```bash
refactor(architecture): improve game state management
improve(performance): optimize rendering
optimize(network): reduce packet size
```

## 📊 Changelog-Beispiel

**Automatisch generiert aus Commits:**
```markdown
## 🚀 What's New in v1.2.0

### 🎮 Game Features
- add random rock generation on game board (abc123) by username
- implement new thunder card ability (def456) by username

### 🛠️ Technical Improvements
- improve game state management (ghi789) by username
- optimize rendering pipeline (jkl012) by username

### 🐛 Bug Fixes
- resolve ship placement validation issue (mno345) by username

### 📦 Installation & Usage
- **Windows**: Download `WaterWizard.Client.exe`
- **macOS**: Download `WaterWizard.Client.MacOS`
- **Development**: Use `./start-game` for easy setup
```

## 🔧 Konfiguration

### Workflow-Dateien
- **`.github/workflows/cd-pipeline.yml`**: Basis-Release-Pipeline
- **`.github/workflows/release-with-changelog.yml`**: Erweiterte Pipeline
- **`CHANGELOG_TEMPLATE.md`**: Template für Release-Notizen

### Commit-Konventionen
- **`COMMIT_CONVENTIONS.md`**: Detaillierte Commit-Richtlinien

## 🎯 Features

### ✅ Automatische Kategorisierung
- **Game Features**: `feat`, `feature`, `add`, `new`, `implement`
- **Bug Fixes**: `fix`, `bug`, `resolve`, `patch`
- **Technical**: `refactor`, `improve`, `optimize`, `update`, `upgrade`, `enhance`
- **Other**: `docs`, `style`, `test`, `chore`

### ✅ Intelligente Versionsverwaltung
- **Automatische Inkrementierung**: Patch-Version wird automatisch erhöht
- **Manuelle Versionen**: Über Workflow-Input möglich
- **Tag-Konflikte**: Automatische Auflösung von Tag-Duplikaten

### ✅ Plattform-Unterstützung
- **Windows**: `.exe` Dateien
- **macOS**: Native macOS-Executables
- **Linux**: Linux-Builds (erweiterbar)

### ✅ Asset-Management
- **Automatische Erkennung**: Findet Executables automatisch
- **Benennung**: Konsistente Dateinamen
- **Upload**: Automatischer Upload zu GitHub Releases

## 🚀 Workflow-Schritte

### 1. Build & Test
```yaml
- Run NUKE Build
- Compile & Test
```

### 2. Publish
```yaml
- Publish Client (Windows)
- Publish Server (Windows)
- Publish Client (macOS)
- Publish Server (macOS)
```

### 3. Asset Preparation
```yaml
- Find executables
- Copy to release-assets
- Validate files
```

### 4. Version Management
```yaml
- Get current version
- Increment patch version
- Generate tag
- Handle conflicts
```

### 5. Changelog Generation
```yaml
- Get previous tag
- Analyze commits
- Categorize changes
- Generate markdown
```

### 6. Release Creation
```yaml
- Create GitHub Release
- Upload assets
- Set release notes
```

## 🔧 Troubleshooting

### Build-Fehler
```bash
# Lokaler Test
dotnet build WaterWizards.sln
./build.sh Clean Compile Test
```

### Workflow-Fehler
1. Überprüfen Sie die **Actions** Tab
2. Schauen Sie sich die **Logs** an
3. Testen Sie lokal mit den Build-Befehlen

### Changelog-Probleme
- Überprüfen Sie Commit-Message-Konventionen
- Stellen Sie sicher, dass Commits auf `main` sind
- Testen Sie mit manueller Workflow-Ausführung

## 📈 Erweiterte Optionen

### Custom Changelog Template
Bearbeiten Sie `CHANGELOG_TEMPLATE.md` für eigene Templates.

### Additional Platforms
Fügen Sie Linux-Builds zur Pipeline hinzu:
```yaml
- name: Publish Client (Linux)
  run: dotnet publish src/WaterWizard.Client/WaterWizard.Client.csproj -c Release -r linux-x64 --self-contained true -o ./publish/linux
```

### Release Drafts
Ändern Sie `draft: false` zu `draft: true` für manuelle Überprüfung.

## 🎯 Vorteile

1. **⏱️ Zeitersparnis**: Automatische Release-Generierung
2. **📝 Konsistenz**: Einheitliche Changelog-Struktur
3. **🎯 Kategorisierung**: Intelligente Commit-Analyse
4. **🔄 Automatisierung**: Keine manuellen Schritte nötig
5. **📊 Übersicht**: Klare Release-Notizen
6. **🛡️ Zuverlässigkeit**: Automatische Fehlerbehandlung

## 🔗 Links

- **GitHub Actions**: `.github/workflows/`
- **Commit-Konventionen**: `COMMIT_CONVENTIONS.md`
- **Changelog-Template**: `CHANGELOG_TEMPLATE.md`
- **Game Launcher**: `GAME_LAUNCHER_README.md` 