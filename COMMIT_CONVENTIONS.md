# 📝 Commit Message Conventions

Diese Konventionen helfen bei der automatischen Generierung von Changelogs für GitHub Releases.

## 🎯 Commit Message Format

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### 📋 Types (für Changelog-Kategorisierung)

| Type | Description | Changelog Category |
|------|-------------|-------------------|
| `feat` | Neue Spiel-Features | 🎮 Game Features |
| `feature` | Neue Spiel-Features | 🎮 Game Features |
| `add` | Neue Funktionen hinzufügen | 🎮 Game Features |
| `new` | Neue Features | 🎮 Game Features |
| `implement` | Implementierung neuer Features | 🎮 Game Features |
| `fix` | Bug-Fixes | 🐛 Bug Fixes |
| `bug` | Bug-Fixes | 🐛 Bug Fixes |
| `resolve` | Probleme lösen | 🐛 Bug Fixes |
| `patch` | Kleine Fixes | 🐛 Bug Fixes |
| `refactor` | Code-Refactoring | 🛠️ Technical Improvements |
| `improve` | Verbesserungen | 🛠️ Technical Improvements |
| `optimize` | Optimierungen | 🛠️ Technical Improvements |
| `update` | Updates | 🛠️ Technical Improvements |
| `upgrade` | Upgrades | 🛠️ Technical Improvements |
| `enhance` | Erweiterungen | 🛠️ Technical Improvements |
| `docs` | Dokumentation | 📝 Other Changes |
| `style` | Formatierung | 📝 Other Changes |
| `test` | Tests | 📝 Other Changes |
| `chore` | Wartungsarbeiten | 📝 Other Changes |

## 🎯 Beispiele

### 🎮 Game Features
```bash
feat(game): add random rock generation on game board
feature(cards): implement new thunder card ability
add(ships): new ship type with special abilities
new(ui): add animated background effects
implement(network): add automatic reconnection feature
```

### 🐛 Bug Fixes
```bash
fix(client): resolve ship placement validation issue
bug(server): fix memory leak in game state management
resolve(network): connection timeout on slow networks
patch(ui): fix button click detection on high DPI displays
```

### 🛠️ Technical Improvements
```bash
refactor(architecture): improve game state management
improve(performance): optimize rendering pipeline
optimize(network): reduce packet size for better performance
update(dependencies): upgrade to .NET 8.0
upgrade(build): implement automated release pipeline
enhance(security): add input validation for all network messages
```

### 📝 Other Changes
```bash
docs(readme): update installation instructions
style(code): apply consistent formatting
test(unit): add comprehensive test coverage
chore(build): update build scripts
```

## 🚀 Best Practices

### ✅ Do's
- Verwenden Sie klare, beschreibende Commit-Messages
- Beginnen Sie mit dem passenden Type
- Halten Sie die erste Zeile unter 50 Zeichen
- Verwenden Sie Imperativ ("add" nicht "added")
- Erklären Sie das "Was" und "Warum", nicht das "Wie"

### ❌ Don'ts
- Vermeiden Sie vage Messages wie "fix stuff" oder "update"
- Keine zu langen Commit-Messages
- Keine technischen Details in der ersten Zeile
- Keine persönlichen Kommentare

## 🔧 Automatische Changelog-Generierung

Die GitHub Actions Workflow analysiert automatisch Ihre Commit-Messages und kategorisiert sie:

1. **🎮 Game Features**: Alle Commits mit `feat`, `feature`, `add`, `new`, `implement`
2. **🐛 Bug Fixes**: Alle Commits mit `fix`, `bug`, `resolve`, `patch`
3. **🛠️ Technical Improvements**: Alle Commits mit `refactor`, `improve`, `optimize`, `update`, `upgrade`, `enhance`
4. **📝 Other Changes**: Alle anderen Commits

## 📊 Beispiel-Changelog

Mit diesen Commit-Messages:
```bash
feat(game): add random rock generation on game board
fix(client): resolve ship placement validation issue
refactor(architecture): improve game state management
docs(readme): update installation instructions
```

Wird automatisch generiert:
```markdown
## 🚀 What's New in v1.2.0

### 🎮 Game Features
- add random rock generation on game board (abc123) by username

### 🛠️ Technical Improvements
- improve game state management (def456) by username

### 🐛 Bug Fixes
- resolve ship placement validation issue (ghi789) by username

### 📝 Other Changes
- update installation instructions (jkl012) by username
```

## 🎯 Workflow Integration

Die Commit-Konventionen funktionieren automatisch mit:
- **Automatische Releases**: Bei Push auf `main` Branch
- **Manuelle Releases**: Über GitHub Actions "Run workflow"
- **Changelog-Generierung**: Automatische Kategorisierung
- **Version Management**: Automatische Versions-Inkrementierung 