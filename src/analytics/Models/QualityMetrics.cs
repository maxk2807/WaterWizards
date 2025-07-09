// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 34 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

namespace WaterWizard.Analytics.Models;

public class QualityMetrics
{
    public double CodeToCommentRatio { get; set; }
    public double EmptyLinesPercentage { get; set; }
    public double AverageFileSize { get; set; }
    public double AverageLinesPerFile { get; set; }
    public double AverageMethodsPerClass { get; set; }
    public double AveragePropertiesPerClass { get; set; }
    public int TotalDevelopers { get; set; }
    public string MostActiveDeveloper { get; set; } = "";
    public double AverageCommitsPerDeveloper { get; set; }
    public double TopDeveloperScore { get; set; }
    public string TopDeveloperBreakdown { get; set; } = "";
    
    // Erweiterte Metriken
    public int TotalPullRequests { get; set; }
    public int MergedPullRequests { get; set; }
    public double AverageLinesPerCommit { get; set; }
    public string MostActiveWeek { get; set; } = "";
    public string MostActiveMonth { get; set; } = "";
    public int MaxCommitsInDay { get; set; }
    public double ProjectVelocity { get; set; } // Commits pro Tag im Durchschnitt
    public double CodeComplexity { get; set; } // Durchschnittliche Methodenlänge
    public double TestCoverage { get; set; } // Platzhalter für Test-Coverage
    public double DocumentationCoverage { get; set; } // Anteil dokumentierter Klassen
    public string RepositoryAge { get; set; } = "";
    public int TotalIssues { get; set; }
    public int ClosedIssues { get; set; }
    public double IssueResolutionRate { get; set; }
    public string MostProductiveDay { get; set; } = "";
    public string MostProductiveHour { get; set; } = "";
} 