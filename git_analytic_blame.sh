#!/bin/bash
echo "Starte Skript"
# Übersicht-Datei
OUTPUT="git_analytic_blame.md"
echo "# Git Analytic Blame Übersicht" > $OUTPUT
echo >> $OUTPUT

# Alle relevanten Dateien im src-Ordner finden
find src -type f \( -name "*.cs" -o -name "*.ts" -o -name "*.js" -o -name "*.py" \) | while read file; do
    echo "Analysiere $file ..."
    echo "## $file" >> $OUTPUT
    # Zeilen pro Autor zählen
    git blame --line-porcelain "$file" | sed -n 's/^author //p' | sort | uniq -c | sort -nr >> $OUTPUT
    echo >> $OUTPUT
    # Kommentar-Block für die Datei erzeugen
    echo "Kommentar-Block für $file:" >> $OUTPUT
    echo "/*" >> $OUTPUT
    git blame --line-porcelain "$file" | sed -n 's/^author //p' | sort | uniq -c | sort -nr | while read count author; do
        echo " * $author: $count Zeilen" >> $OUTPUT
    done
    echo " */" >> $OUTPUT
    echo >> $OUTPUT
done