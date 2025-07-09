#!/bin/bash
echo "Starte Skript"
OUTPUT="git_analytic_blame.md"
echo "# Git Analytic Blame Übersicht" > $OUTPUT
echo >> $OUTPUT

find src -type f \( -name "*.cs" -o -name "*.ts" -o -name "*.js" -o -name "*.py" \) | while read file; do
    echo "Analysiere $file ..."
    echo "## $file" >> $OUTPUT
    git blame --line-porcelain "$file" | sed -n 's/^author //p' | sort | uniq -c | sort -nr >> $OUTPUT
    echo >> $OUTPUT
    echo "Kommentar-Block für $file:" >> $OUTPUT
    echo "/*" >> $OUTPUT
    git blame --line-porcelain "$file" | sed -n 's/^author //p' | sort | uniq -c | sort -nr | while read count author; do
        echo " * $author: $count Zeilen" >> $OUTPUT
    done
    echo " */" >> $OUTPUT
    echo >> $OUTPUT
done