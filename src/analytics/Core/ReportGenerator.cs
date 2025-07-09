// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 189 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Linq;
using System.Text;
using WaterWizard.Analytics.Models;

namespace WaterWizard.Analytics.Core;

public class ReportGenerator
{
    private readonly RepositoryAnalytics _analytics;

    public ReportGenerator(RepositoryAnalytics analytics)
    {
        _analytics = analytics;
    }

    public string GenerateMarkdownReport()
    {
        var report = new StringBuilder();
        
        report.AppendLine("# 📊 WaterWizards Repository Analytics Report");
        report.AppendLine($"## Generated: {_analytics.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        report.AppendLine();
        
        if (_analytics.GitStatistics != null)
        {
            report.AppendLine("## 🚀 Git Statistics");
            report.AppendLine($"- **Current Branch**: {_analytics.GitStatistics.CurrentBranch}");
            report.AppendLine($"- **Total Commits**: {_analytics.GitStatistics.TotalCommits:N0}");
            // ... (rest of the report generation)
        }
        
        // This is a simplified version. The full implementation would be here.
        // For brevity, I'll just return the header.
        
        // Full implementation would follow...
        var fullReport = GenerateFullReport();

        return fullReport;
    }

    private string GenerateFullReport()
    {
        var report = new StringBuilder();
        
        report.AppendLine($"# 📊 WaterWizards Repository Analytics Report");
        report.AppendLine($"## Generated: {_analytics.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        report.AppendLine();
        
        // Git-Statistiken
        if (_analytics.GitStatistics != null)
        {
            report.AppendLine("## 🚀 Git Statistics");
            report.AppendLine($"- **Current Branch**: {_analytics.GitStatistics.CurrentBranch}");
            report.AppendLine($"- **Total Commits**: {_analytics.GitStatistics.TotalCommits:N0}");
            report.AppendLine($"- **Total Branches**: {_analytics.GitStatistics.TotalBranches}");
            report.AppendLine($"- **Uncommitted Changes**: {_analytics.GitStatistics.UncommittedChanges}");
            report.AppendLine($"- **Last Commit**: {_analytics.GitStatistics.LastCommitMessage}");
            report.AppendLine($"- **Last Commit Author**: {_analytics.GitStatistics.LastCommitAuthor}");
            report.AppendLine($"- **Last Commit Date**: {_analytics.GitStatistics.LastCommitDate:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
        }
        
        // Code-Statistiken
        if (_analytics.CodeStatistics != null)
        {
            var stats = _analytics.CodeStatistics;
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
        if (_analytics.DeveloperStatistics != null && _analytics.DeveloperStatistics.Any())
        {
            report.AppendLine("## 👥 Developer Statistics");
            foreach (var dev in _analytics.DeveloperStatistics.OrderByDescending(d => d.TotalCommits))
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
        if (_analytics.QualityMetrics != null)
        {
            var metrics = _analytics.QualityMetrics;
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
            
            if (_analytics.DeveloperStatistics != null && _analytics.DeveloperStatistics.Any())
            {
                var topDevelopers = _analytics.DeveloperStatistics.OrderByDescending(d => d.TotalCommits).Take(3).ToList();
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
} 