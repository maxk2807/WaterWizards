// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 13 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Analytics.Models;

public class GitStatistics
{
    public int UncommittedChanges { get; set; }
    public string CurrentBranch { get; set; } = "";
    public int TotalCommits { get; set; }
    public int TotalBranches { get; set; }
    public string LastCommit { get; set; } = "";
    public string LastCommitMessage { get; set; } = "";
    public string LastCommitAuthor { get; set; } = "";
    public DateTime LastCommitDate { get; set; }
} 