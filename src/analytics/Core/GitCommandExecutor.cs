// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 34 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Analytics.Core;

public class GitCommandExecutor
{
    private readonly string _repositoryPath;

    public GitCommandExecutor(string repositoryPath)
    {
        _repositoryPath = repositoryPath;
    }
    
    public async Task<string> ExecuteAsync(string command)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = _repositoryPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        return output.Trim();
    }
} 