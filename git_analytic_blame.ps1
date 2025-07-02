# PowerShell-Skript: git_analytic_blame.ps1
$Output = "git_analytic_blame.md"
Set-Content -Path $Output -Value "# Git Analytic Blame Übersicht`n"

# Dateitypen anpassen, falls nötig
$files = Get-ChildItem -Path .\src -Recurse -Include *.cs,*.bat,*.json,*.txt,*.py,*.sh,*.ps1,*.md,*.cmd,*.sh,*.ps1,*.md

foreach ($file in $files) {
    # Relativer Pfad ab src/
    $relPath = $file.FullName -replace '.*\\src\\', 'src\'
    Write-Host "Analysiere $relPath ..."
    Add-Content -Path $Output -Value "## $relPath"
    $blame = git blame --line-porcelain $file.FullName | Select-String "^author " | ForEach-Object { $_.ToString().Substring(7) }
    $total = $blame.Count
    $counts = $blame | Group-Object | Sort-Object Count -Descending

    # Markdown-Tabelle
    Add-Content -Path $Output -Value "| Entwickler         | Zeilen | Anteil (%) |"
    Add-Content -Path $Output -Value "|-------------------|--------|------------|"
    foreach ($entry in $counts) {
        $percent = [math]::Round(($entry.Count / $total) * 100, 1)
        Add-Content -Path $Output -Value ("| {0,-17} | {1,6} | {2,10} |" -f $entry.Name, $entry.Count, $percent)
    }
    Add-Content -Path $Output -Value ""
    # Kommentar-Block für die Datei erzeugen
    Add-Content -Path $Output -Value "Kommentar-Block für ${relPath}:"
    Add-Content -Path $Output -Value "/*"
    foreach ($entry in $counts) {
        $percent = [math]::Round(($entry.Count / $total) * 100, 1)
        Add-Content -Path $Output -Value (" * {0}: {1} Zeilen ({2}%)" -f $entry.Name, $entry.Count, $percent)
    }
    Add-Content -Path $Output -Value " */`n"
}
Write-Host "Fertig! Siehe $Output"

# Mapping für die Namen
$nameMap = @{
    'jdewi001'      = 'Justin'
    'justinjd00'    = 'Justin'
    'maxk2807'      = 'Max'
    'Max Kondratov' = 'Max'
    'erick'         = 'Erick'
    'Erickk0'       = 'Erick'
    'Erick Zeiler'  = 'Erick'
    'Paul'          = 'Paul'
    'jlnhsrm'       = 'Julian'
}

# Gesamtauswertung
$allBlames = @()
foreach ($file in $files) {
    $blame = git blame --line-porcelain $file.FullName | Select-String "^author " | ForEach-Object { $_.ToString().Substring(7) }
    $allBlames += $blame
}

# Namen zusammenfassen
$grouped = @{}
foreach ($name in $allBlames) {
    $main = $nameMap[$name]
    if (-not $main) { $main = $name }
    if ($grouped.ContainsKey($main)) {
        $grouped[$main] += 1
    } else {
        $grouped[$main] = 1
    }
}

# Sortieren nach Zeilenzahl absteigend
$sorted = $grouped.GetEnumerator() | Sort-Object Value -Descending

$totalAll = $allBlames.Count

Add-Content -Path $Output -Value "## Gesamtübersicht (alle Dateien, zusammengefasst)`n"
Add-Content -Path $Output -Value "| Entwickler | Zeilen | Anteil (%) |"
Add-Content -Path $Output -Value "|------------|--------|------------|"
foreach ($entry in $sorted) {
    $percent = [math]::Round(($entry.Value / $totalAll) * 100, 1)
    Add-Content -Path $Output -Value ("| {0,-10} | {1,6} | {2,10} |" -f $entry.Key, $entry.Value, $percent)
}
Add-Content -Path $Output -Value ""

# Nach der Gesamttabelle:
Add-Content -Path $Output -Value "### Zeilenanteil pro Entwickler (gesamt, zusammengefasst)`n"

$maxValue = ($sorted | Select-Object -First 1).Value
foreach ($entry in $sorted) {
    $percent = [math]::Round(($entry.Value / $totalAll) * 100, 1)
    $barLen = [math]::Round(($entry.Value / $maxValue) * 40)
    $bar = ('█' * $barLen)
    Add-Content -Path $Output -Value ("{0,-8} [{1,-40}] {2} ({3}%) 
    " -f $entry.Key, $bar, $entry.Value, $percent)
}
Add-Content -Path $Output -Value ""

# GitHub-Lines: hinzugefügte und gelöschte Zeilen pro Autor
$gitLog = git log --pretty="%aN" --numstat
$authorStats = @{}
$currAuthor = ""
foreach ($line in $gitLog) {
    if ($line -match "^[^0-9]") {
        $currAuthor = $line.Trim()
        if (-not $authorStats.ContainsKey($currAuthor)) {
            $authorStats[$currAuthor] = @{added=0; removed=0}
        }
    } elseif ($line -match "^[0-9]") {
        $parts = $line -split "\s+"
        if ($parts.Length -ge 2) {
            $authorStats[$currAuthor].added += [int]$parts[0]
            $authorStats[$currAuthor].removed += [int]$parts[1]
        }
    }
}

# Namen zusammenfassen wie oben
$authorMap = @{
    'jdewi001'      = 'Justin'
    'justinjd00'    = 'Justin'
    'maxk2807'      = 'Max'
    'Max Kondratov' = 'Max'
    'erick'         = 'Erick'
    'Erickk0'       = 'Erick'
    'Erick Zeiler'  = 'Erick'
    'Paul'          = 'Paul'
    'jlnhsrm'       = 'Julian'
}

$githubLines = @{}
foreach ($author in $authorStats.Keys) {
    $main = $nameMap[$author]
    if (-not $main) { $main = $author }
    if (-not $githubLines.ContainsKey($main)) {
        $githubLines[$main] = @{added=0; removed=0}
    }
    $githubLines[$main].added += $authorStats[$author].added
    $githubLines[$main].removed += $authorStats[$author].removed
}

# Tabelle ausgeben
Add-Content -Path $Output -Value "## GitHub-Lines (alle Commits, zusammengefasst)`n"
Add-Content -Path $Output -Value "| Entwickler | Hinzugefügt | Gelöscht |"
Add-Content -Path $Output -Value "|------------|------------|----------|"
foreach ($entry in $githubLines.GetEnumerator() | Sort-Object { $_.Value.added } -Descending) {
    # Nur Namen aus $nameMap (bzw. die, die du wirklich willst)
    if ($nameMap.Values -contains $entry.Key) {
        Add-Content -Path $Output -Value ("| {0,-10} | {1,10} | {2,8} |" -f $entry.Key, $entry.Value.added, $entry.Value.removed)
    }
}
Add-Content -Path $Output -Value ""