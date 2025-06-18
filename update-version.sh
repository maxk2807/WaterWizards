#!/bin/bash

# WaterWizards Version Updater
# Aktualisiert die Version in der .csproj Datei basierend auf dem neuesten Git-Tag

set -e

# Farben
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}🔄 WaterWizards Version Updater${NC}"
echo "=================================="

# Fetch latest tags
git fetch --tags

# Get latest tag
LATEST_TAG=$(git tag --sort=-version:refname | head -1)

if [ -z "$LATEST_TAG" ]; then
    echo -e "${YELLOW}⚠️  Keine Tags gefunden. Verwende 0.0.0${NC}"
    BASE_VERSION="0.0.0"
else
    # Remove 'v' prefix and get version
    BASE_VERSION=${LATEST_TAG#v}
    echo -e "${BLUE}📊 Neuester Tag: $LATEST_TAG${NC}"
    echo -e "${BLUE}📊 Basis-Version: $BASE_VERSION${NC}"
fi

# Parse version components
MAJOR=$(echo "$BASE_VERSION" | cut -d. -f1)
MINOR=$(echo "$BASE_VERSION" | cut -d. -f2)
PATCH=$(echo "$BASE_VERSION" | cut -d. -f3)
[ -z "$PATCH" ] && PATCH=0

# Increment patch version for next release
NEXT_PATCH=$((PATCH + 1))
NEW_VERSION="$MAJOR.$MINOR.$NEXT_PATCH"

echo -e "${BLUE}🚀 Nächste Release-Version: $NEW_VERSION${NC}"

# Update .csproj file
CSPROJ_FILE="src/WaterWizard.Server/WaterWizard.Server.csproj"

if [ -f "$CSPROJ_FILE" ]; then
    # Check if Version tag already exists
    if grep -q "<Version>" "$CSPROJ_FILE"; then
        # Update existing Version tag
        if [[ "$OSTYPE" == "darwin"* ]]; then
            # macOS
            sed -i '' "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/" "$CSPROJ_FILE"
        else
            # Linux
            sed -i "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/" "$CSPROJ_FILE"
        fi
        echo -e "${GREEN}✅ Version in $CSPROJ_FILE aktualisiert: $NEW_VERSION${NC}"
    else
        # Add Version tag to PropertyGroup
        if [[ "$OSTYPE" == "darwin"* ]]; then
            # macOS
            sed -i '' "s/<\/PropertyGroup>/    <Version>$NEW_VERSION<\/Version>\n  <\/PropertyGroup>/" "$CSPROJ_FILE"
        else
            # Linux
            sed -i "s/<\/PropertyGroup>/    <Version>$NEW_VERSION<\/Version>\n  <\/PropertyGroup>/" "$CSPROJ_FILE"
        fi
        echo -e "${GREEN}✅ Version zu $CSPROJ_FILE hinzugefügt: $NEW_VERSION${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  $CSPROJ_FILE nicht gefunden${NC}"
    exit 1
fi

# Show current version
CURRENT_VERSION=$(grep -m1 '<Version>' "$CSPROJ_FILE" | sed -E 's/.*<Version>(.+)<\/Version>.*/\1/')
echo -e "${BLUE}📋 Aktuelle Version in .csproj: $CURRENT_VERSION${NC}"

# Calculate next version for next release
NEXT_NEXT_PATCH=$((NEXT_PATCH + 1))
NEXT_NEXT_VERSION="$MAJOR.$MINOR.$NEXT_NEXT_PATCH"

echo -e "${BLUE}🚀 Übernächste Release-Version wird sein: $NEXT_NEXT_VERSION${NC}"
echo ""
echo -e "${GREEN}✅ Version erfolgreich aktualisiert!${NC}"
echo -e "${YELLOW}💡 Tipp: Committe die Änderungen und pushe sie${NC}" 