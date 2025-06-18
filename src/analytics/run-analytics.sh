#!/bin/bash

# WaterWizards Analytics Runner
# Führt das Analytics-System für das Repository aus

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$SCRIPT_DIR/../.."
ANALYTICS_EXE="$SCRIPT_DIR/bin/Debug/net8.0/WaterWizard.Analytics"

# Farben
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

# Bauen, falls nötig
if [ ! -f "$ANALYTICS_EXE" ]; then
    echo -e "${BLUE}🔨 Baue Analytics-Tool...${NC}"
    cd "$SCRIPT_DIR"
    dotnet build --configuration Debug
fi

# Ausführen
echo -e "${BLUE}📊 Starte Analytics für Repository: $REPO_ROOT${NC}"
"$ANALYTICS_EXE" "$REPO_ROOT"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Analytics erfolgreich ausgeführt${NC}"
else
    echo -e "${RED}❌ Analytics fehlgeschlagen${NC}"
    exit 1
fi 