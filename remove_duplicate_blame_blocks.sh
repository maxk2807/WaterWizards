#!/bin/bash

CODE_DIR="src"

find "$CODE_DIR" -name "*.cs" ! -path "*/obj/*" ! -path "*/bin/*" | while read -r file; do
    awk '
    BEGIN {inblock=0}
    /^\/\/ =+/ {if(inblock==0){inblock=1; next} else{inblock=0; next}}
    inblock==1 {next}
    {print}
    ' "$file" > "$file.tmp" && mv "$file.tmp" "$file"
    echo "Statistik-Block entfernt: $file"
done

echo "Fertig! Alle Statistik-Kommentarblöcke wurden entfernt."