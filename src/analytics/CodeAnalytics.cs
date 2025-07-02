using System.Text.Json;
using System.Text.RegularExpressions;
using WaterWizard.Analytics.Models;
using WaterWizard.Analytics.Core;

namespace WaterWizard.Analytics;

/// <summary>
/// Code Analytics System für Repository-Statistiken
/// Generiert detaillierte Berichte über Code-Qualität, Entwickler-Aktivität und Projekt-Metriken
/// </summary>
public static class CodeAnalytics
{
    // Dateien die für Analytics nicht benötigt werden
    private static readonly string[] AnalyticsIgnoreFiles = { 
        ".DS_Store", 
        "Thumbs.db", 
        "desktop.ini",
        "*.tmp",
        "*.log",
        "*.bak",
        "*.swp",
        "*.swo",
        "*~"
    };

    /// <summary>
    /// Hauptmethode für die Generierung von Repository-Statistiken
    /// </summary>
    /// <param name="repositoryPath">Pfad zum Repository</param>
    /// <returns>Analytics-Report als JSON</returns>
    public static async Task<string> GenerateRepositoryAnalytics(string repositoryPath = ".")
    {
        var analytics = new RepositoryAnalytics
        {
            GeneratedAt = DateTime.UtcNow,
            RepositoryPath = Path.GetFullPath(repositoryPath),
            ProjectName = "WaterWizards"
        };

        try
        {
            // Git-Statistiken sammeln
            var gitCollector = new GitStatisticsCollector(repositoryPath);
            analytics.GitStatistics = await gitCollector.CollectAsync();
            
            // Code-Statistiken sammeln
            var codeCollector = new CodeStatisticsCollector(repositoryPath);
            analytics.CodeStatistics = codeCollector.Collect();
            
            // Entwickler-Statistiken sammeln
            var devCollector = new DeveloperStatisticsCollector(repositoryPath);
            analytics.DeveloperStatistics = await devCollector.CollectAsync();
            
            // Qualitäts-Metriken berechnen
            var metricsCalculator = new QualityMetricsCalculator(analytics);
            analytics.QualityMetrics = metricsCalculator.Calculate();
            
            // Report generieren
            var report = GenerateReport(analytics);
            
            // JSON-Export erstellen
            var jsonReport = JsonSerializer.Serialize(analytics, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            // Report speichern
            await SaveReport(analytics, jsonReport, report);
            
            return jsonReport;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CodeAnalytics] Fehler bei der Analyse: {ex.Message}");
            return JsonSerializer.Serialize(new { Error = ex.Message, GeneratedAt = DateTime.UtcNow });
        }
    }
    
    /// <summary>
    /// Generiert einen lesbaren Report
    /// </summary>
    private static string GenerateReport(RepositoryAnalytics analytics)
    {
        var report = new System.Text.StringBuilder();
        
        report.AppendLine("# 📊 WaterWizards Repository Analytics Report");
        report.AppendLine($"## Generated: {analytics.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        report.AppendLine();
        
        // Git-Statistiken
        if (analytics.GitStatistics != null)
        {
            report.AppendLine("## 🚀 Git Statistics");
            report.AppendLine($"- **Current Branch**: {analytics.GitStatistics.CurrentBranch}");
            report.AppendLine($"- **Total Commits**: {analytics.GitStatistics.TotalCommits:N0}");
            report.AppendLine($"- **Total Branches**: {analytics.GitStatistics.TotalBranches}");
            report.AppendLine($"- **Uncommitted Changes**: {analytics.GitStatistics.UncommittedChanges}");
            report.AppendLine($"- **Last Commit**: {analytics.GitStatistics.LastCommitMessage}");
            report.AppendLine($"- **Last Commit Author**: {analytics.GitStatistics.LastCommitAuthor}");
            report.AppendLine($"- **Last Commit Date**: {analytics.GitStatistics.LastCommitDate:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
        }
        
        // Code-Statistiken
        if (analytics.CodeStatistics != null)
        {
            var stats = analytics.CodeStatistics;
            report.AppendLine("## 📝 Code Statistics");
            report.AppendLine($"- **Total Files**: {stats.TotalFiles:N0}");
            report.AppendLine($"- **Total Lines**: {stats.TotalLines:N0}");
            report.AppendLine($"- **Code Lines**: {stats.CodeLines:N0}");
            report.AppendLine($"- **Comment Lines**: {stats.CommentLines:N0}");
            report.AppendLine($"- **Empty Lines**: {stats.EmptyLines:N0}");
            report.AppendLine($"- **Total Size**: {stats.TotalSize / 1024.0:F2} KB");
            report.AppendLine();
            
            report.AppendLine("### 📁 Files by Type");
            foreach (var fileType in stats.FilesByType.OrderByDescending(x => x.Value))
            {
                report.AppendLine($"- **{fileType.Key}**: {fileType.Value:N0} files");
            }
            report.AppendLine();
            
            if (stats.CSharpFiles > 0)
            {
                report.AppendLine("### 🔧 C# Specific Metrics");
                report.AppendLine($"- **C# Files**: {stats.CSharpFiles:N0}");
                report.AppendLine($"- **Classes**: {stats.Classes:N0}");
                report.AppendLine($"- **Methods**: {stats.Methods:N0}");
                report.AppendLine($"- **Interfaces**: {stats.Interfaces:N0}");
                report.AppendLine($"- **Properties**: {stats.Properties:N0}");
                report.AppendLine();
            }
        }
        
        // Entwickler-Statistiken
        if (analytics.DeveloperStatistics != null && analytics.DeveloperStatistics.Any())
        {
            report.AppendLine("## 👥 Developer Statistics");
            foreach (var dev in analytics.DeveloperStatistics.OrderByDescending(d => d.TotalCommits))
            {
                report.AppendLine($"### {dev.Name}");
                if (dev.OriginalNames.Count > 1)
                {
                    report.AppendLine($"- **Aliases**: {string.Join(", ", dev.OriginalNames)}");
                }
                report.AppendLine($"- **Total Commits**: {dev.TotalCommits:N0}");
                report.AppendLine($"- **First Commit**: {dev.FirstCommit:yyyy-MM-dd}");
                report.AppendLine($"- **Last Commit**: {dev.LastCommit:yyyy-MM-dd}");
                report.AppendLine($"- **Days Active**: {dev.DaysSinceFirstCommit:N0} days");
                report.AppendLine($"- **Commit Frequency**: {dev.CommitFrequency:F2} commits/day");
                report.AppendLine();
                
                report.AppendLine("#### 📊 Commit Breakdown");
                report.AppendLine($"- **Feature Commits**: {dev.FeatureCommits:N0} ({dev.FeatureCommitPercentage:F1}%)");
                report.AppendLine($"- **Bug Fix Commits**: {dev.BugFixCommits:N0} ({dev.BugFixCommitPercentage:F1}%)");
                report.AppendLine($"- **Refactor Commits**: {dev.RefactorCommits:N0} ({dev.RefactorCommitPercentage:F1}%)");
                report.AppendLine($"- **Documentation Commits**: {dev.DocumentationCommits:N0} ({dev.DocumentationCommitPercentage:F1}%)");
                report.AppendLine();
                
                report.AppendLine("#### 📈 Activity Patterns");
                report.AppendLine($"- **Average Commits/Week**: {dev.AverageCommitsPerWeek:F1}");
                report.AppendLine($"- **Average Commits/Month**: {dev.AverageCommitsPerMonth:F1}");
                report.AppendLine($"- **Most Active Week**: {dev.MostActiveWeek} ({dev.MaxCommitsInWeek} commits)");
                report.AppendLine($"- **Most Active Month**: {dev.MostActiveMonth} ({dev.MaxCommitsInMonth} commits)");
                report.AppendLine();
            }
        }
        
        // Qualitäts-Metriken
        if (analytics.QualityMetrics != null)
        {
            var metrics = analytics.QualityMetrics;
            report.AppendLine("## 🎯 Quality Metrics");
            report.AppendLine($"- **Code to Comment Ratio**: {metrics.CodeToCommentRatio:F2}");
            report.AppendLine($"- **Empty Lines Percentage**: {metrics.EmptyLinesPercentage:F1}%");
            report.AppendLine($"- **Average File Size**: {metrics.AverageFileSize / 1024.0:F2} KB");
            report.AppendLine($"- **Average Lines Per File**: {metrics.AverageLinesPerFile:F1}");
            report.AppendLine($"- **Average Methods Per Class**: {metrics.AverageMethodsPerClass:F1}");
            report.AppendLine($"- **Average Properties Per Class**: {metrics.AveragePropertiesPerClass:F1}");
            report.AppendLine($"- **Code Complexity**: {metrics.CodeComplexity:F1} lines/method");
            report.AppendLine($"- **Documentation Coverage**: {metrics.DocumentationCoverage:F1} lines/class");
            report.AppendLine();
            
            report.AppendLine("### 👥 Team Statistics");
            report.AppendLine($"- **Total Developers**: {metrics.TotalDevelopers}");
            report.AppendLine($"- **Most Active Developer**: {metrics.MostActiveDeveloper}");
            report.AppendLine($"- **Top Developer Score**: {metrics.TopDeveloperScore:F1}");
            report.AppendLine($"- **Top Developer Breakdown**: {metrics.TopDeveloperBreakdown}");
            report.AppendLine($"- **Average Commits Per Developer**: {metrics.AverageCommitsPerDeveloper:F1}");
            report.AppendLine();
            
            report.AppendLine("### 📈 Project Velocity");
            report.AppendLine($"- **Repository Age**: {metrics.RepositoryAge}");
            report.AppendLine($"- **Project Velocity**: {metrics.ProjectVelocity:F2} commits/day");
            report.AppendLine($"- **Most Active Week**: {metrics.MostActiveWeek}");
            report.AppendLine($"- **Most Active Month**: {metrics.MostActiveMonth}");
            report.AppendLine();
            
            report.AppendLine("### 🏆 Team Excellence");
            report.AppendLine("🎉 **Congratulations to our amazing development team!** 🎉");
            report.AppendLine();
            report.AppendLine("**What makes us special:**");
            report.AppendLine($"- **{metrics.TotalDevelopers} talented developers** working together");
            report.AppendLine($"- **{metrics.ProjectVelocity:F1} commits per day** average velocity");
            report.AppendLine($"- **{metrics.AverageCommitsPerDeveloper:F1} commits per developer** showing dedication");
            report.AppendLine($"- **{metrics.CodeToCommentRatio:F1} code-to-comment ratio** indicating good documentation");
            report.AppendLine($"- **{metrics.EmptyLinesPercentage:F1}% empty lines** showing clean, readable code");
            report.AppendLine();
            
            // Top 3 Entwickler hervorheben
            if (analytics.DeveloperStatistics != null && analytics.DeveloperStatistics.Any())
            {
                var topDevelopers = analytics.DeveloperStatistics
                    .OrderByDescending(d => d.TotalCommits)
                    .Take(3)
                    .ToList();
                
                report.AppendLine("**🏅 Top Contributors:**");
                for (int i = 0; i < topDevelopers.Count; i++)
                {
                    var dev = topDevelopers[i];
                    var medal = i == 0 ? "🥇" : i == 1 ? "🥈" : "🥉";
                    report.AppendLine($"- {medal} **{dev.Name}**: {dev.TotalCommits} commits ({dev.FeatureCommits} features, {dev.BugFixCommits} bugfixes)");
                }
                report.AppendLine();
            }
        }
        
        report.AppendLine("---");
        report.AppendLine("*This report was automatically generated by the CodeAnalytics system*");
        
        return report.ToString();
    }
    
    /// <summary>
    /// Speichert den Report in Dateien
    /// </summary>
    private static async Task SaveReport(RepositoryAnalytics analytics, string jsonReport, string markdownReport)
    {
        var analyticsDir = Path.Combine(analytics.RepositoryPath, "analytics");
        Directory.CreateDirectory(analyticsDir);
        
        var timestamp = analytics.GeneratedAt.ToString("yyyyMMdd_HHmmss");
        
        // Nur die neuesten Reports speichern (max. 5 historische Reports)
        const int maxHistoricalReports = 5;
        
        // Alte Reports aufräumen
        await CleanupOldReports(analyticsDir, maxHistoricalReports);
        
        // JSON-Report speichern
        var jsonPath = Path.Combine(analyticsDir, $"analytics_{timestamp}.json");
        await File.WriteAllTextAsync(jsonPath, jsonReport);
        
        // Markdown-Report speichern
        var markdownPath = Path.Combine(analyticsDir, $"analytics_{timestamp}.md");
        await File.WriteAllTextAsync(markdownPath, markdownReport);
        
        // Neuesten Report als "latest" speichern
        var latestJsonPath = Path.Combine(analyticsDir, "analytics_latest.json");
        var latestMarkdownPath = Path.Combine(analyticsDir, "analytics_latest.md");
        
        await File.WriteAllTextAsync(latestJsonPath, jsonReport);
        await File.WriteAllTextAsync(latestMarkdownPath, markdownReport);
        
        Console.WriteLine($"[CodeAnalytics] Reports saved to {analyticsDir}");
    }
    
    /// <summary>
    /// Räumt alte Reports auf und behält nur die neuesten
    /// </summary>
    private static async Task CleanupOldReports(string analyticsDir, int maxReports)
    {
        try
        {
            // Alle historischen Reports finden
            var jsonFiles = Directory.GetFiles(analyticsDir, "analytics_*.json")
                .Where(f => !f.EndsWith("latest.json"))
                .OrderByDescending(f => f)
                .ToList();
                
            var markdownFiles = Directory.GetFiles(analyticsDir, "analytics_*.md")
                .Where(f => !f.EndsWith("latest.md"))
                .OrderByDescending(f => f)
                .ToList();
            
            // Alte Reports löschen (behalte nur die neuesten)
            var filesToDelete = jsonFiles.Skip(maxReports).Concat(markdownFiles.Skip(maxReports));
            
            // Zusätzlich: Lösche unnötige Dateien im analytics-Verzeichnis
            var allFiles = Directory.GetFiles(analyticsDir, "*.*", SearchOption.TopDirectoryOnly);
            var unnecessaryFiles = allFiles.Where(file => 
            {
                var fileName = Path.GetFileName(file);
                return AnalyticsIgnoreFiles.Any(pattern => 
                    fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase) ||
                    (pattern.StartsWith("*.") && fileName.EndsWith(pattern.Substring(1), StringComparison.OrdinalIgnoreCase)));
            });
            
            filesToDelete = filesToDelete.Concat(unnecessaryFiles);
            
            foreach (var file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CodeAnalytics] Konnte Datei nicht löschen: {file} - {ex.Message}");
                }
            }
            
            if (filesToDelete.Any())
            {
                Console.WriteLine($"[CodeAnalytics] {filesToDelete.Count()} Dateien aufgeräumt");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CodeAnalytics] Fehler beim Aufräumen: {ex.Message}");
        }
    }
} 