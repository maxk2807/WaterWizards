#!/bin/bash

# WaterWizards Git Hooks Setup
# Installiert automatisch die Pre-Merge Analytics Hooks

set -e

# Farben für bessere Ausgabe
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🔧 WaterWizards Git Hooks Setup${NC}"
echo "====================================="

# Repository-Root finden
REPO_ROOT=$(git rev-parse --show-toplevel)
HOOKS_DIR="$REPO_ROOT/.git/hooks"
ANALYTICS_DIR="$REPO_ROOT/src/analytics"

echo -e "${BLUE}📁 Repository: $REPO_ROOT${NC}"
echo -e "${BLUE}📁 Hooks-Verzeichnis: $HOOKS_DIR${NC}"

# Prüfen ob wir in einem Git-Repository sind
if [ ! -d "$HOOKS_DIR" ]; then
    echo -e "${RED}❌ Kein Git-Repository gefunden${NC}"
    exit 1
fi

# Analytics-Tool bauen
echo -e "${BLUE}🔨 Baue Analytics-Tool...${NC}"
cd "$ANALYTICS_DIR"
if dotnet build --configuration Debug; then
    echo -e "${GREEN}✅ Analytics-Tool erfolgreich gebaut${NC}"
else
    echo -e "${RED}❌ Fehler beim Bauen des Analytics-Tools${NC}"
    exit 1
fi
cd "$REPO_ROOT"

# Pre-Merge Hook installieren
echo -e "${BLUE}📝 Installiere Pre-Merge Hook...${NC}"

# Pre-commit Hook (wird vor jedem Commit ausgeführt)
PRE_COMMIT_HOOK="$HOOKS_DIR/pre-commit"
cat > "$PRE_COMMIT_HOOK" << 'EOF'
#!/bin/bash
# WaterWizards Pre-Commit Analytics Hook

REPO_ROOT=$(git rev-parse --show-toplevel)
ANALYTICS_EXE="$REPO_ROOT/src/analytics/bin/Debug/net8.0/WaterWizard.Analytics"

# Nur ausführen wenn Analytics-Tool existiert
if [ -f "$ANALYTICS_EXE" ]; then
    echo "🔍 Führe Pre-Commit Analytics aus..."
    "$ANALYTICS_EXE" "$REPO_ROOT"
fi
EOF

# Pre-merge-commit Hook (wird vor jedem Merge ausgeführt)
PRE_MERGE_COMMIT_HOOK="$HOOKS_DIR/pre-merge-commit"
cat > "$PRE_MERGE_COMMIT_HOOK" << 'EOF'
#!/bin/bash
# WaterWizards Pre-Merge Analytics Hook

REPO_ROOT=$(git rev-parse --show-toplevel)
ANALYTICS_EXE="$REPO_ROOT/src/analytics/bin/Debug/net8.0/WaterWizard.Analytics"

# Nur ausführen wenn Analytics-Tool existiert
if [ -f "$ANALYTICS_EXE" ]; then
    echo "🔍 Führe Pre-Merge Analytics aus..."
    "$ANALYTICS_EXE" "$REPO_ROOT"
fi
EOF

# Pre-receive Hook (wird auf dem Server vor dem Empfang von Commits ausgeführt)
PRE_RECEIVE_HOOK="$HOOKS_DIR/pre-receive"
cat > "$PRE_RECEIVE_HOOK" << 'EOF'
#!/bin/bash
# WaterWizards Pre-Receive Analytics Hook

REPO_ROOT=$(git rev-parse --show-toplevel)
ANALYTICS_EXE="$REPO_ROOT/src/analytics/bin/Debug/net8.0/WaterWizard.Analytics"

# Nur ausführen wenn Analytics-Tool existiert
if [ -f "$ANALYTICS_EXE" ]; then
    echo "🔍 Führe Pre-Receive Analytics aus..."
    "$ANALYTICS_EXE" "$REPO_ROOT"
fi
EOF

# Hooks ausführbar machen
chmod +x "$PRE_COMMIT_HOOK"
chmod +x "$PRE_MERGE_COMMIT_HOOK"
chmod +x "$PRE_RECEIVE_HOOK"

echo -e "${GREEN}✅ Git Hooks erfolgreich installiert${NC}"

# Analytics-Verzeichnis erstellen
ANALYTICS_OUTPUT_DIR="$REPO_ROOT/analytics"
mkdir -p "$ANALYTICS_OUTPUT_DIR"

# .gitignore Eintrag hinzufügen (falls nicht vorhanden)
GITIGNORE="$REPO_ROOT/.gitignore"
if [ -f "$GITIGNORE" ]; then
    if ! grep -q "analytics/" "$GITIGNORE"; then
        echo "" >> "$GITIGNORE"
        echo "# Analytics Reports" >> "$GITIGNORE"
        echo "analytics/analytics_*.json" >> "$GITIGNORE"
        echo "analytics/analytics_*.md" >> "$GITIGNORE"
        echo "!analytics/analytics_latest.*" >> "$GITIGNORE"
        echo -e "${GREEN}✅ .gitignore aktualisiert${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  .gitignore nicht gefunden${NC}"
fi

# Test-Ausführung
echo -e "${BLUE}🧪 Teste Analytics-System...${NC}"
ANALYTICS_EXE="$ANALYTICS_DIR/bin/Debug/net8.0/WaterWizard.Analytics"
if [ -f "$ANALYTICS_EXE" ] && "$ANALYTICS_EXE" "$REPO_ROOT"; then
    echo -e "${GREEN}✅ Analytics-System funktioniert korrekt${NC}"
else
    echo -e "${RED}❌ Analytics-System-Test fehlgeschlagen${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}🎉 Setup erfolgreich abgeschlossen!${NC}"
echo ""
echo -e "${BLUE}📋 Installierte Hooks:${NC}"
echo "   - pre-commit: Wird vor jedem Commit ausgeführt"
echo "   - pre-merge-commit: Wird vor jedem Merge ausgeführt"
echo "   - pre-receive: Wird auf dem Server vor dem Empfang ausgeführt"
echo ""
echo -e "${BLUE}📊 Analytics-Reports werden gespeichert in:${NC}"
echo "   $ANALYTICS_OUTPUT_DIR"
echo ""
echo -e "${BLUE}🔧 Manuelle Ausführung:${NC}"
echo "   cd $ANALYTICS_DIR && dotnet run"
echo ""
echo -e "${YELLOW}💡 Tipp: Hooks können mit --no-verify übersprungen werden${NC}" 