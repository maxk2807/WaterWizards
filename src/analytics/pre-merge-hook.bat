@echo off
setlocal enabledelayedexpansion

REM WaterWizards Pre-Merge Analytics Hook (Windows)
REM Führt automatisch Code-Analytics vor jedem Merge aus

echo 🔍 WaterWizards Pre-Merge Analytics Hook
echo ==============================================

REM Repository-Root finden
for /f "tokens=*" %%i in ('git rev-parse --show-toplevel') do set REPO_ROOT=%%i
set ANALYTICS_DIR=%REPO_ROOT%\src\analytics
set ANALYTICS_EXE=%ANALYTICS_DIR%\bin\Debug\net8.0\WaterWizard.Analytics.exe

REM Prüfen ob Analytics-Tool existiert
if not exist "%ANALYTICS_EXE%" (
    echo ⚠️  Analytics-Tool nicht gefunden. Baue es zuerst...
    
    REM Analytics-Tool bauen
    cd /d "%ANALYTICS_DIR%"
    dotnet build --configuration Debug
    if errorlevel 1 (
        echo ❌ Fehler beim Bauen des Analytics-Tools
        exit /b 1
    )
    echo ✅ Analytics-Tool erfolgreich gebaut
    cd /d "%REPO_ROOT%"
)

REM Analytics ausführen
echo 📊 Führe Code-Analytics aus...
"%ANALYTICS_EXE%" "%REPO_ROOT%"
if errorlevel 1 (
    echo ❌ Analytics fehlgeschlagen
    echo 💡 Tipp: Sie können den Hook mit --no-verify überspringen
    exit /b 1
)

echo ✅ Analytics erfolgreich ausgeführt

REM Analytics-Dateien zum Commit hinzufügen (optional)
if exist "%REPO_ROOT%\analytics" (
    git add "%REPO_ROOT%\analytics\analytics_latest.json" 2>nul
    git add "%REPO_ROOT%\analytics\analytics_latest.md" 2>nul
    echo 📝 Analytics-Reports zum Commit hinzugefügt
)

REM Qualitäts-Checks (optional)
echo 🔍 Führe Qualitäts-Checks durch...

REM Prüfe ob Analytics erfolgreich war
if exist "%REPO_ROOT%\analytics\analytics_latest.json" (
    echo ✅ Analytics-Report erfolgreich generiert
    
    REM Hier können weitere Qualitäts-Checks hinzugefügt werden
    REM z.B. Mindestanzahl von Tests, Code-Coverage, etc.
    
) else (
    echo ⚠️  Analytics-Report nicht gefunden
)

echo 🎉 Pre-Merge Analytics abgeschlossen
echo ============================================== 