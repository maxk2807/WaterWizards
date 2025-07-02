# 🚀 WaterWizards Release Notes

## 📋 What's New in v0.0.29

### 🎮 Game Features
- F268- Card Template added (571d93b) by Paul
- F268-Card asset upload FireBolt and Thunder (b516b5a) by Paul
- F268- Card Template added (c749f1a) by Paul
- F268-Card asset upload FireBolt and Thunder (4884b2e) by Paul


### 🛠️ Technical Improvements
- refactoring complete (cc01866) by maxk2807
- refactor methods and comments (4321cd6) by maxk2807


### 🐛 Bug Fixes
- fixed (d63b9a3) by maxk2807


### 📝 Other Changes
- Aktualisierung der Analytics-Berichte: Anpassung der generierten Zeitstempel, Erhöhung der Gesamtanzahl der Dateien und Zeilen sowie Verbesserung der Qualitätsmetriken. Einführung einer Autor-Alias-Karte zur Normalisierung von Entwicklernamen in den Statistiken. (51082e8) by jdewi001
- Implementierung von Code- und Entwicklerstatistiken-Collector-Klassen sowie Git-Statistiken- und Qualitätsmetriken-Berechnung. Einführung von Modellen für Code-, Entwickler-, Git- und Qualitätsmetriken. Verbesserung der Berichtsgenerierung für Repository-Analysen. (ccf54a7) by jdewi001
- Pause-Management implementiert: PauseToggle-Anfrage vom Client an den Server gesendet und Server-Status aktualisiert. Neue Klasse HandlePause hinzugefügt, um die Pause-Logik zu verwalten. Spielzustand wird nun korrekt pausiert und fortgesetzt. (f7a5c57) by jdewi001
- finished ThunderCard, implemented in CardAbilities (f321731) by maxk2807
- should be fixed (7b24517) by maxk2807
- Thunder Card Refactoring (27ee5ae) by maxk2807
- Refactor card implementations and introduce new utility card (ef18abc) by erick
- Thunder Card Refactoring (394702e) by maxk2807
- Entfernen der CD-Pipeline-Konfiguration aus der Datei cd-pipeline.yml. Anpassungen im Release-Workflow zur Verwendung von PRs für die Changelog-Generierung und zur Verbesserung der Entwicklerstatistiken. Aktualisierung der Version in der WaterWizard.Server.csproj auf 0.0.29 und Anpassungen in den Analytics-Berichten zur Erfassung neuer Statistiken und Qualitätsmetriken. (949e595) by jdewi001
- zurückverschoben (30917b8) by maxk2807
- Utility Card Refactoring (3389d6a) by maxk2807
- Hinzufügen eines Skripts zur automatischen Aktualisierung der Version in der .csproj-Datei basierend auf dem neuesten Git-Tag. Anpassungen im Release-Workflow zur Verwendung der aktuellen Projektversion oder des neuesten Tags. Aktualisierung der Analytics-Berichte mit neuen Statistiken und Qualitätsmetriken. (d814398) by jdewi001
- Implement surrender mechanics: add surrender handling in client and server game states (d17d501) by Erickk0
- Aktualisierung der Analytics-Berichte: Anpassung der generierten Zeitstempel, Erhöhung der Gesamtanzahl der Dateien und Zeilen sowie Verbesserung der Qualitätsmetriken. Die Statistiken wurden in den JSON- und Markdown-Dateien aktualisiert, um die neuesten Änderungen widerzuspiegeln. (cd5c69d) by jdewi001
- Hinzufügen eines Analytics-Systems zur Generierung von Repository-Statistiken. Implementierung von Git- und Code-Statistiken, Entwickler-Aktivität und Qualitätsmetriken. Automatisierung der Ausführung über Git-Hooks (pre-commit, pre-merge, pre-receive) und Bereitstellung von JSON- und Markdown-Reports. Aktualisierung der .gitignore für Analytics-Berichte. (99c67f3) by jdewi001
- Remove unnecessary calculations for gold freeze dot position in Draw method (aca1be2) by Erickk0
- Add gold freeze mechanics and update handling in game state and client (7657a75) by Erickk0
- card deletion (5458de2) by maxk2807
- card deletion (aa3d60b) by maxk2807
- interface and factory without functionality (bc59521) by maxk2807
- Optimierung der Schleifen in RockFactory zur Verbesserung der Lesbarkeit: Entfernen unnötiger Typumwandlungen für die Koordinaten X und Y. (3d7474d) by jdewi001
- Hinzufügen von CHANGELOG- und COMMIT_CONVENTIONS-Dokumenten sowie Automatisierung des Release-Prozesses mit intelligenten Changelogs. Implementierung von Workflows zur automatischen Generierung von Releases und Changelogs basierend auf Commit-Nachrichten. (fd589ee) by jdewi001
- Hinzufügen von Start-Skripten und README für den WaterWizards Game Launcher. Implementierung von Skripten für Unix/macOS/Linux, Windows CMD und PowerShell zur einfachen Ausführung des Spiels. Dokumentation der Verwendung, Features und Troubleshooting im README. (1f1ced7) by jdewi001
- Implementierung der Stein-Logik für WaterWizards: Hinzufügen von RockFactory und RockHandler zur intelligenten Generierung und Synchronisation von Steinen. Aktualisierung der Client-Logik zur Behandlung von Stein-Synchronisation und Anpassung der Spiellogik zur Berücksichtigung von Steinen. Erweiterung des CellState um Rock und Anpassungen im GameBoard zur Darstellung von Steinen. (1f7144b) by jdewi001
- Update CardAbilities.cs (97fd374) by Justin Dewitz
- Add GreedHit card implementation and update related classes (e7276e5) by Erickk0
- Fix possible loss of precision (d599fca) by Paul
- Implementation responsive main menu backgrounds and ingame grid background (390935a) by Paul
- Placement State Ready Check UI Improvements (5600ded) by maxk2807
- Add ArcaneMissileCard implementation and update DamageCardFactory (242d05f) by erick
- Implementierung der Paralize-Karte und zugehörige Logik. Hinzufügen von ParalizeHandler und ManaHandler zur Verwaltung der Paralize-Effekte und deren Auswirkungen auf die Mana-Generierung. Aktualisierung der Kartenstapel und UI zur Unterstützung der neuen Karte. Anpassungen in mehreren Klassen zur Integration der neuen Funktionalität. (34b373e) by justinjd00
- floating point fix (ace9869) by maxk2807
- starts two clients (7cbbbe7) by maxk2807
- heal implementation (1a4bbeb) by maxk2807
- Center destRect calculation in MainMenuState (1594929) by erick
- this should fix it (0acf510) by Erickk0
- Enhance game mechanics and improve code readability (5f7b8de) by erick
- Add gold management to RessourceField and update UI on gold change (4abb1db) by erick
- Enhance thunder strike mechanics: increase MaxDuration to 1.0f, add Hit property, and update visual effects based on hit status; refactor related methods for clarity (0e3dd68) by Erickk0
- Enhance thunder strike mechanics: add damage tracking and visual effects; refactor related methods for clarity (5281b36) by Erickk0
- Refactor thunder strike handling in GameBoard and CardHandler; remove commented code for clarity (e730424) by erick
- Enhance thunder strike functionality and player index handling (b30e49f) by erick
- comment out tests (13e3f12) by maxk2807
- Bug Fix Stand bis jetzt (21a7153) by erick
- Fix potential null reference exception when checking game over state in AttackHandler. (d164e1d) by erick
- Fix nullability of GameState in AttackHandler; update players array initialization in GameState to use default! for null assignment. (ba41ee0) by erick
- commented tests out because of the CI Pipeline (4e36dc8) by erick
- Refactor attack handling by introducing AttackHandler class; streamline attack logic and improve game state management. (1248d31) by erick
- Refactor card handling logic by introducing CardHandler and CellHandler classes; streamline card buying and casting processes, and enhance cell reveal functionality. (ff89b04) by erick
- einfuegen einiger assets fuer dir UI2 (6dd97eb) by Paul
- einfuegen einiger assets fuer dir UI (6958db3) by Paul


### 📦 Installation & Usage

#### 🪟 Windows
1. Download `WaterWizard.Client.exe`
2. Run the executable
3. Host or join a game

#### 🍎 macOS
1. Download `WaterWizard.Client.MacOS`
2. Make it executable: `chmod +x WaterWizard.Client.MacOS`
3. Run the game

#### 🔧 Development Setup
```bash
# Clone the repository
git clone https://github.com/yourusername/WaterWizards.git
cd WaterWizards

# Use the optimized launcher
./start-game
```

### 🎯 Quick Start
1. **Host Game**: Click "Host Game" in the first client
2. **Join Game**: Click "Join Game" in the second client, enter "localhost"
3. **Ready Up**: Both players click "Ready"
4. **Start Game**: Click "Start Game" to begin

---

*This changelog was generated on: 2025-06-25 10:09:57 UTC*
