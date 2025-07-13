// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 35 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;
using System.Collections.Generic;

namespace WaterWizard.Analytics.Models;

public class DeveloperStatistics
{
    public string Name { get; set; } = "";
    public DateTime FirstCommit { get; set; }
    public DateTime LastCommit { get; set; }
    public int TotalCommits { get; set; }
    public int FeatureCommits { get; set; }
    public int BugFixCommits { get; set; }
    public int RefactorCommits { get; set; }
    public int DocumentationCommits { get; set; }
    public List<string> CommitMessages { get; set; } = new();
    public List<string> OriginalNames { get; set; } = new();
    public Dictionary<string, int> WeeklyActivity { get; set; } = new();
    public Dictionary<string, int> MonthlyActivity { get; set; } = new();

    // Erweiterte Statistiken
    public double AverageCommitsPerWeek { get; set; }
    public double AverageCommitsPerMonth { get; set; }
    public string MostActiveWeek { get; set; } = "";
    public string MostActiveMonth { get; set; } = "";
    public int MaxCommitsInWeek { get; set; }
    public int MaxCommitsInMonth { get; set; }
    public double FeatureCommitPercentage { get; set; }
    public double BugFixCommitPercentage { get; set; }
    public double RefactorCommitPercentage { get; set; }
    public double DocumentationCommitPercentage { get; set; }
    public int DaysSinceFirstCommit { get; set; }
    public int DaysSinceLastCommit { get; set; }
    public double CommitFrequency { get; set; } // Commits pro Tag
}
