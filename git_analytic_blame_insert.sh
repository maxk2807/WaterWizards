#!/bin/bash

# Datei mit den Kommentar-Blöcken
BLAIME_FILE="git_analytic_blame.md"

# Alle relevanten Dateien im src-Ordner finden
find src -type f \( -name "*.cs" -o -name "*.ts" -o -name "*.js" -o -name "*.py" \) | while read file; do
    # Kommentar-Block aus der Blame-Datei extrahieren
    block=$(awk -v f="$file" '
        $0=="Kommentar-Block für "f":" {flag=1; next}
        /^\s*\*\// && flag {print; flag=0; next}
        flag {print}
    ' "$BLAIME_FILE")
    # Wenn Block gefunden, einfügen
    if [[ ! -z "$block" ]]; then
        echo "Füge Kommentar-Block in $file ein ..."
        # Prüfen, ob bereits ein Block am Anfang steht (optional: ersetzen)
        if grep -q "^/\*" "$file"; then
            # Ersetze existierenden Block
            awk '/^\/\*/{flag=1} /^ \*\//{flag=0; next} !flag' "$file" > "$file.tmp"
            (echo "/*"; echo "$block"; echo "*/"; cat "$file.tmp") > "$file.new"
            mv "$file.new" "$file"
            rm "$file.tmp"
        else
            # Füge neuen Block ein
            (echo "/*"; echo "$block"; echo "*/"; cat "$file") > "$file.new"
            mv "$file.new" "$file"
        fi
    fi
done

echo "Fertig! Alle Kommentar-Blöcke wurden eingefügt." 