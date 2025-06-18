# 🚀 WaterWizards Release Notes

## 📋 What's New in v0.0.0

### 🎮 Game Features
- F268- Card Template added (c749f1a) by Paul
- F268-Card asset upload FireBolt and Thunder (4884b2e) by Paul


### 🛠️ Technical Improvements
- refactoring complete (cc01866) by maxk2807
- refactor methods and comments (4321cd6) by maxk2807


### 🐛 Bug Fixes
- fixed (d63b9a3) by maxk2807


### 📝 Other Changes
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

*This changelog was generated on: 2025-06-18 12:06:05 UTC*
