// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 90 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;
using System.Linq;
using WaterWizard.Analytics.Models;

namespace WaterWizard.Analytics.Core;

public class QualityMetricsCalculator
{
    private readonly RepositoryAnalytics _analytics;

    public QualityMetricsCalculator(RepositoryAnalytics analytics)
    {
        _analytics = analytics;
    }

    public QualityMetrics Calculate()
    {
        var qualityMetrics = new QualityMetrics();

        if (_analytics.CodeStatistics != null)
        {
            var stats = _analytics.CodeStatistics;
            qualityMetrics.CodeToCommentRatio = stats.CommentLines > 0 ? (double)stats.CodeLines / stats.CommentLines : 0;
            qualityMetrics.EmptyLinesPercentage = stats.TotalLines > 0 ? (double)stats.EmptyLines / stats.TotalLines * 100 : 0;
            qualityMetrics.AverageFileSize = stats.TotalFiles > 0 ? stats.TotalSize / stats.TotalFiles : 0;
            qualityMetrics.AverageLinesPerFile = stats.TotalFiles > 0 ? stats.TotalLines / stats.TotalFiles : 0;

            if (stats.CSharpFiles > 0)
            {
                qualityMetrics.AverageMethodsPerClass = stats.Classes > 0 ? (double)stats.Methods / stats.Classes : 0;
                qualityMetrics.AveragePropertiesPerClass = stats.Classes > 0 ? (double)stats.Properties / stats.Classes : 0;
                qualityMetrics.CodeComplexity = stats.Methods > 0 ? (double)stats.CodeLines / stats.Methods : 0;
                qualityMetrics.DocumentationCoverage = stats.Classes > 0 ? (double)stats.CommentLines / stats.Classes : 0;
            }
        }

        if (_analytics.DeveloperStatistics != null && _analytics.DeveloperStatistics.Any())
        {
            qualityMetrics.TotalDevelopers = _analytics.DeveloperStatistics.Count;

            foreach (var dev in _analytics.DeveloperStatistics)
            {
                dev.FeatureCommitPercentage = dev.TotalCommits > 0 ? (double)dev.FeatureCommits / dev.TotalCommits * 100 : 0;
                dev.BugFixCommitPercentage = dev.TotalCommits > 0 ? (double)dev.BugFixCommits / dev.TotalCommits * 100 : 0;
                dev.RefactorCommitPercentage = dev.TotalCommits > 0 ? (double)dev.RefactorCommits / dev.TotalCommits * 100 : 0;
                dev.DocumentationCommitPercentage = dev.TotalCommits > 0 ? (double)dev.DocumentationCommits / dev.TotalCommits * 100 : 0;
                
                var now = DateTime.Now;
                dev.DaysSinceFirstCommit = (now - dev.FirstCommit).Days;
                dev.DaysSinceLastCommit = (now - dev.LastCommit).Days;
                dev.CommitFrequency = dev.DaysSinceFirstCommit > 0 ? (double)dev.TotalCommits / dev.DaysSinceFirstCommit : 0;
                
                dev.AverageCommitsPerWeek = dev.WeeklyActivity.Any() ? dev.WeeklyActivity.Values.Average() : 0;
                dev.AverageCommitsPerMonth = dev.MonthlyActivity.Any() ? dev.MonthlyActivity.Values.Average() : 0;
                
                dev.MostActiveWeek = dev.WeeklyActivity.Any() ? dev.WeeklyActivity.OrderByDescending(x => x.Value).First().Key : "";
                dev.MostActiveMonth = dev.MonthlyActivity.Any() ? dev.MonthlyActivity.OrderByDescending(x => x.Value).First().Key : "";
                dev.MaxCommitsInWeek = dev.WeeklyActivity.Any() ? dev.WeeklyActivity.Values.Max() : 0;
                dev.MaxCommitsInMonth = dev.MonthlyActivity.Any() ? dev.MonthlyActivity.Values.Max() : 0;
            }
            
            var developerScores = _analytics.DeveloperStatistics.Select(dev => new
            {
                Developer = dev,
                Score = (dev.TotalCommits * 0.3) + (dev.FeatureCommits * 0.25) + (dev.BugFixCommits * 0.2) + (dev.AverageCommitsPerWeek * 0.15) + (dev.DocumentationCommits * 0.1)
            }).OrderByDescending(x => x.Score).ToList();
            
            qualityMetrics.MostActiveDeveloper = developerScores.FirstOrDefault()?.Developer.Name ?? "Unknown";
            qualityMetrics.AverageCommitsPerDeveloper = _analytics.DeveloperStatistics.Average(d => d.TotalCommits);
            qualityMetrics.TopDeveloperScore = developerScores.FirstOrDefault()?.Score ?? 0;
            qualityMetrics.TopDeveloperBreakdown = developerScores.FirstOrDefault()?.Developer != null ? 
                $"Commits: {developerScores.First().Developer.TotalCommits}, Features: {developerScores.First().Developer.FeatureCommits}, Bugfixes: {developerScores.First().Developer.BugFixCommits}, Weekly Avg: {developerScores.First().Developer.AverageCommitsPerWeek:F1}, Docs: {developerScores.First().Developer.DocumentationCommits}" : "";
            
            var allWeeks = _analytics.DeveloperStatistics.SelectMany(d => d.WeeklyActivity).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
            var allMonths = _analytics.DeveloperStatistics.SelectMany(d => d.MonthlyActivity).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
            
            qualityMetrics.MostActiveWeek = allWeeks.Any() ? allWeeks.OrderByDescending(x => x.Value).First().Key : "";
            qualityMetrics.MostActiveMonth = allMonths.Any() ? allMonths.OrderByDescending(x => x.Value).First().Key : "";
            
            var firstCommit = _analytics.DeveloperStatistics.Min(d => d.FirstCommit);
            var lastCommit = _analytics.DeveloperStatistics.Max(d => d.LastCommit);
            var repositoryAge = (lastCommit - firstCommit).Days;
            qualityMetrics.RepositoryAge = $"{repositoryAge} days ({repositoryAge / 365.25:F1} years)";
            
            qualityMetrics.ProjectVelocity = repositoryAge > 0 ? (double)_analytics.DeveloperStatistics.Sum(d => d.TotalCommits) / repositoryAge : 0;
        }
        
        return qualityMetrics;
    }
} 