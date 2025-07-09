#!/bin/bash

CODE_DIR="src"

find "$CODE_DIR" -name "*.cs" ! -path "*/obj/*" ! -path "*/bin/*" | while read -r file; do
    echo "Bearbeite $file ..."

    git blame --line-porcelain "$file" 2>/dev/null | grep "^author " | cut -d' ' -f2- | sort | uniq -c | sort -nr > /tmp/authors.txt

    methods=$(grep -E '^\s*(public|private|protected|internal)[^;{]*\([^\)]*\)\s*[{;]' "$file" | \
        sed -E 's/^\s*(public|private|protected|internal)[^ ]* [^ ]* ([^ (]+).*/\2/' | sort | uniq)

    comment="// ===============================================\n"
    comment+="// Autoren-Statistik (automatisch generiert):\n"
    while read -r count author; do
        comment+="// - $author: $count Zeilen\n"
    done < <(awk '{count=$1; $1=""; sub(/^ /,""); print count, $0}' /tmp/authors.txt)
    comment+="// \n// Methoden/Funktionen in dieser Datei:\n"
    if [[ -z "$methods" ]]; then
        comment+="// (Keine Methoden/Funktionen gefunden)\n"
    else
        while read -r method; do
            [[ -n "$method" ]] && comment+="// - $method\n"
        done <<< "$methods"
    fi
    comment+="// ===============================================\n"

    tmpfile=$(mktemp)
    echo -e "$comment" > "$tmpfile"
    cat "$file" >> "$tmpfile"
    mv "$tmpfile" "$file"

    echo "Kommentarblock eingefügt in $file"
done

echo "Fertig!"