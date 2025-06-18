#!/bin/bash

# WaterWizards Changelog Generator
# Generiert einen Changelog für den aktuellen Branch basierend auf Commits seit dem letzten Tag

set -e

# Farben
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}🚀 WaterWizards Changelog Generator${NC}"
echo "====================================="

# Check if we're on main branch
CURRENT_BRANCH=$(git branch --show-current)
if [ "$CURRENT_BRANCH" != "main" ]; then
    echo -e "${YELLOW}⚠️  Du bist auf Branch: $CURRENT_BRANCH${NC}"
    echo -e "${YELLOW}💡 Für Releases solltest du auf 'main' sein${NC}"
    echo ""
fi

# Get the previous tag
PREVIOUS_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")

if [ -z "$PREVIOUS_TAG" ]; then
    echo -e "${YELLOW}⚠️  Kein vorheriger Tag gefunden. Verwende alle Commits.${NC}"
    COMMITS=$(git log --pretty=format:"%s|%h|%an" --no-merges)
else
    echo -e "${BLUE}📊 Analysiere Commits seit $PREVIOUS_TAG${NC}"
    COMMITS=$(git log --pretty=format:"%s|%h|%an" --no-merges ${PREVIOUS_TAG}..HEAD)
fi

# Categorize commits based on WaterWizards conventions
GAME_FEATURES=""
TECHNICAL_IMPROVEMENTS=""
BUG_FIXES=""
DOCUMENTATION_CHANGES=""
MAINTENANCE_CHANGES=""
UNCATEGORIZED=""

TOTAL_COMMITS=0
CATEGORIZED_COMMITS=0

while IFS='|' read -r message hash author; do
    # Skip empty lines
    [ -z "$message" ] && continue
    
    TOTAL_COMMITS=$((TOTAL_COMMITS + 1))
    
    # WaterWizards-specific patterns
    if [[ "$message" =~ ^(feat|feature|add|new|implement|F[0-9]+) ]] || [[ "$message" =~ F[0-9]+ ]]; then
        GAME_FEATURES+="- $message ($hash) by $author"$'\n'
        CATEGORIZED_COMMITS=$((CATEGORIZED_COMMITS + 1))
    elif [[ "$message" =~ ^(fix|bug|resolve|patch|B[0-9]+) ]] || [[ "$message" =~ B[0-9]+ ]]; then
        BUG_FIXES+="- $message ($hash) by $author"$'\n'
        CATEGORIZED_COMMITS=$((CATEGORIZED_COMMITS + 1))
    elif [[ "$message" =~ ^(refactor|improve|optimize|update|upgrade|enhance) ]]; then
        TECHNICAL_IMPROVEMENTS+="- $message ($hash) by $author"$'\n'
        CATEGORIZED_COMMITS=$((CATEGORIZED_COMMITS + 1))
    elif [[ "$message" =~ ^(docs|documentation) ]]; then
        DOCUMENTATION_CHANGES+="- 📚 $message ($hash) by $author"$'\n'
        CATEGORIZED_COMMITS=$((CATEGORIZED_COMMITS + 1))
    elif [[ "$message" =~ ^(test|chore|style|ci|build) ]]; then
        MAINTENANCE_CHANGES+="- 🔧 $message ($hash) by $author"$'\n'
        CATEGORIZED_COMMITS=$((CATEGORIZED_COMMITS + 1))
    else
        # Only include meaningful uncategorized commits (not merge commits, etc.)
        if [[ ! "$message" =~ ^(merge|Merge|WIP|wip|temp|Temp|test|Test) ]] && [[ ${#message} -gt 10 ]]; then
            UNCATEGORIZED+="- $message ($hash) by $author"$'\n'
            CATEGORIZED_COMMITS=$((CATEGORIZED_COMMITS + 1))
        fi
    fi
done <<< "$COMMITS"

# Get current version
VERSION=$(grep -m1 '<Version>' src/WaterWizard.Server/WaterWizard.Server.csproj | sed -E 's/.*<Version>(.+)<\/Version>.*/\1/')
if [ -z "$VERSION" ]; then
    VERSION="0.0.0"
fi

# Create changelog
CHANGELOG="# 🚀 WaterWizards Release Notes

## 📋 What's New in v$VERSION

"

# Add sections only if they have content
if [ -n "$GAME_FEATURES" ]; then
    CHANGELOG+="### 🎮 Game Features
$GAME_FEATURES

"
else
    CHANGELOG+="### 🎮 Game Features
- No new game features in this release

"
fi

if [ -n "$TECHNICAL_IMPROVEMENTS" ]; then
    CHANGELOG+="### 🛠️ Technical Improvements
$TECHNICAL_IMPROVEMENTS

"
else
    CHANGELOG+="### 🛠️ Technical Improvements
- No technical improvements in this release

"
fi

if [ -n "$BUG_FIXES" ]; then
    CHANGELOG+="### 🐛 Bug Fixes
$BUG_FIXES

"
else
    CHANGELOG+="### 🐛 Bug Fixes
- No bug fixes in this release

"
fi

if [ -n "$DOCUMENTATION_CHANGES" ]; then
    CHANGELOG+="### 📚 Documentation
$DOCUMENTATION_CHANGES

"
fi

if [ -n "$MAINTENANCE_CHANGES" ]; then
    CHANGELOG+="### 🔧 Maintenance
$MAINTENANCE_CHANGES

"
fi

if [ -n "$UNCATEGORIZED" ]; then
    CHANGELOG+="### 📝 Other Changes
$UNCATEGORIZED

"
fi

CHANGELOG+="### 📦 Installation & Usage

#### 🪟 Windows
1. Download \`WaterWizard.Client.exe\`
2. Run the executable
3. Host or join a game

#### 🍎 macOS
1. Download \`WaterWizard.Client.MacOS\`
2. Make it executable: \`chmod +x WaterWizard.Client.MacOS\`
3. Run the game

#### 🔧 Development Setup
\`\`\`bash
# Clone the repository
git clone https://github.com/yourusername/WaterWizards.git
cd WaterWizards

# Use the optimized launcher
./start-game
\`\`\`

### 🎯 Quick Start
1. **Host Game**: Click \"Host Game\" in the first client
2. **Join Game**: Click \"Join Game\" in the second client, enter \"localhost\"
3. **Ready Up**: Both players click \"Ready\"
4. **Start Game**: Click \"Start Game\" to begin

---

*This changelog was generated on: $(date -u +'%Y-%m-%d %H:%M:%S UTC')*"

# Save changelog
echo "$CHANGELOG" > CHANGELOG.md

echo -e "${GREEN}✅ Changelog erfolgreich generiert: CHANGELOG.md${NC}"
echo ""
echo -e "${BLUE}📊 Zusammenfassung:${NC}"
echo "   - Total Commits: $TOTAL_COMMITS"
echo "   - Categorized: $CATEGORIZED_COMMITS"
echo "   - Game Features: $(echo "$GAME_FEATURES" | grep -c "^-" || echo "0")"
echo "   - Technical Improvements: $(echo "$TECHNICAL_IMPROVEMENTS" | grep -c "^-" || echo "0")"
echo "   - Bug Fixes: $(echo "$BUG_FIXES" | grep -c "^-" || echo "0")"
echo "   - Documentation: $(echo "$DOCUMENTATION_CHANGES" | grep -c "^-" || echo "0")"
echo "   - Maintenance: $(echo "$MAINTENANCE_CHANGES" | grep -c "^-" || echo "0")"
echo "   - Other Changes: $(echo "$UNCATEGORIZED" | grep -c "^-" || echo "0")"
echo ""

if [ "$CURRENT_BRANCH" != "main" ]; then
    echo -e "${YELLOW}💡 Tipp: Für einen Release, merge zuerst auf 'main'${NC}"
else
    echo -e "${GREEN}🎉 Bereit für Release!${NC}"
fi

echo -e "${YELLOW}💡 Tipp: Überprüfe CHANGELOG.md und passe bei Bedarf an${NC}" 