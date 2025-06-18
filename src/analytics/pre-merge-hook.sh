#!/bin/bash

# WaterWizards Pre-Merge Analytics Hook
# Führt automatisch Code-Analytics vor jedem Merge aus

set -e

# Farben für bessere Ausgabe
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Repository-Root finden
REPO_ROOT=$(git rev-parse --show-toplevel)
ANALYTICS_DIR="$REPO_ROOT/src/analytics"
ANALYTICS_EXE="$ANALYTICS_DIR/bin/Debug/net8.0/WaterWizard.Analytics"

echo -e "${BLUE}🔍 WaterWizards Pre-Merge Analytics Hook${NC}"
echo "=============================================="

# Prüfen ob Analytics-Tool existiert
if [ ! -f "$ANALYTICS_EXE" ]; then
    echo -e "${YELLOW}⚠️  Analytics-Tool nicht gefunden. Baue es zuerst...${NC}"
    
    # Analytics-Tool bauen
    cd "$ANALYTICS_DIR"
    if dotnet build --configuration Debug; then
        echo -e "${GREEN}✅ Analytics-Tool erfolgreich gebaut${NC}"
    else
        echo -e "${RED}❌ Fehler beim Bauen des Analytics-Tools${NC}"
        exit 1
    fi
    cd "$REPO_ROOT"
fi

# Analytics ausführen
echo -e "${BLUE}📊 Führe Code-Analytics aus...${NC}"
if "$ANALYTICS_EXE" "$REPO_ROOT"; then
    echo -e "${GREEN}✅ Analytics erfolgreich ausgeführt${NC}"
    
    # Analytics-Dateien zum Commit hinzufügen (optional)
    if [ -d "$REPO_ROOT/analytics" ]; then
        git add "$REPO_ROOT/analytics/analytics_latest.json" 2>/dev/null || true
        git add "$REPO_ROOT/analytics/analytics_latest.md" 2>/dev/null || true
        echo -e "${BLUE}📝 Analytics-Reports zum Commit hinzugefügt${NC}"
    fi
    
    # Qualitäts-Checks (optional)
    echo -e "${BLUE}🔍 Führe Qualitäts-Checks durch...${NC}"
    
    # Prüfe ob Analytics erfolgreich war
    if [ -f "$REPO_ROOT/analytics/analytics_latest.json" ]; then
        echo -e "${GREEN}✅ Analytics-Report erfolgreich generiert${NC}"
        
        # Hier können weitere Qualitäts-Checks hinzugefügt werden
        # z.B. Mindestanzahl von Tests, Code-Coverage, etc.
        
    else
        echo -e "${YELLOW}⚠️  Analytics-Report nicht gefunden${NC}"
    fi
    
else
    echo -e "${RED}❌ Analytics fehlgeschlagen${NC}"
    echo -e "${YELLOW}💡 Tipp: Sie können den Hook mit --no-verify überspringen${NC}"
    exit 1
fi

echo -e "${GREEN}🎉 Pre-Merge Analytics abgeschlossen${NC}"
echo "==============================================" 