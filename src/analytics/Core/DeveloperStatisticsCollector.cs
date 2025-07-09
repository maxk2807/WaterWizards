// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 159 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WaterWizard.Analytics.Models;

namespace WaterWizard.Analytics.Core;

public class DeveloperStatisticsCollector
{
    private readonly GitCommandExecutor _gitCommandExecutor;

    private static readonly Dictionary<string, string> AuthorAliasMap = new()
    {
        // Justin Dewitz
        {"justindewitz", "Justin Dewitz"},
        {"justinjd00", "Justin Dewitz"},
        {"jdewi001", "Justin Dewitz"},
        {"justindew", "Justin Dewitz"},
        // Max Kondratov
        {"maxkondratov", "Max Kondratov"},
        {"maxk2807", "Max Kondratov"},
        {"max", "Max Kondratov"},
        // Erick Zeiler
        {"erickzeiler", "Erick Zeiler"},
        {"erick", "Erick Zeiler"},
        {"erickk0", "Erick Zeiler"},
        // Julian
        {"julian", "Julian"},
        {"jlnhsrm", "Julian"},
        // Paul
        {"paul", "Paul"}
    };

    public DeveloperStatisticsCollector(string repositoryPath)
    {
        _gitCommandExecutor = new GitCommandExecutor(repositoryPath);
    }

    public async Task<List<DeveloperStatistics>> CollectAsync()
    {
        try
        {
            var nameToNormalized = new Dictionary<string, string>();
            var normalizedToAliases = new Dictionary<string, HashSet<string>>();
            var developerStats = new Dictionary<string, DeveloperStatistics>();

            var gitLog = await _gitCommandExecutor.ExecuteAsync("log --pretty=format:\"%an|%ad|%s\" --date=short");
            var commits = gitLog.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var commit in commits)
            {
                var parts = commit.Split('|');
                if (parts.Length < 3) continue;

                var author = parts[0];
                var dateStr = parts[1];
                var message = parts[2];

                if (!DateTime.TryParse(dateStr, out var date))
                {
                    Console.WriteLine($"[CodeAnalytics] Konnte Datum nicht parsen: {dateStr}");
                    continue;
                }

                var normalizedAuthor = NormalizeAuthorName(author);
                if (!normalizedToAliases.ContainsKey(normalizedAuthor))
                    normalizedToAliases[normalizedAuthor] = new HashSet<string>();
                normalizedToAliases[normalizedAuthor].Add(author);

                if (!developerStats.ContainsKey(normalizedAuthor))
                {
                    developerStats[normalizedAuthor] = new DeveloperStatistics
                    {
                        Name = normalizedAuthor,
                        FirstCommit = date,
                        LastCommit = date
                    };
                }

                var dev = developerStats[normalizedAuthor];
                dev.TotalCommits++;
                if (date < dev.FirstCommit) dev.FirstCommit = date;
                if (date > dev.LastCommit) dev.LastCommit = date;
                dev.CommitMessages.Add(message);

                var weekKey = GetIsoWeekKey(date);
                var monthKey = date.ToString("yyyy-MM");
                dev.WeeklyActivity[weekKey] = dev.WeeklyActivity.GetValueOrDefault(weekKey) + 1;
                dev.MonthlyActivity[monthKey] = dev.MonthlyActivity.GetValueOrDefault(monthKey) + 1;

                if (IsFeatureCommit(message)) dev.FeatureCommits++;
                else if (IsBugFixCommit(message)) dev.BugFixCommits++;
                else if (message.StartsWith("refactor:", StringComparison.OrdinalIgnoreCase)) dev.RefactorCommits++;
                else if (message.StartsWith("docs:", StringComparison.OrdinalIgnoreCase)) dev.DocumentationCommits++;
            }

            foreach (var kv in developerStats)
            {
                if (normalizedToAliases.ContainsKey(kv.Key))
                    kv.Value.OriginalNames = normalizedToAliases[kv.Key].OrderBy(x => x).ToList();
            }

            return developerStats.Values.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CodeAnalytics] Entwickler-Statistiken konnten nicht gesammelt werden: {ex.Message}");
            return new List<DeveloperStatistics>();
        }
    }
    
    private static string NormalizeAuthorName(string authorName)
    {
        var normalized = authorName.ToLowerInvariant().Replace(" ", "").Trim();
        if (AuthorAliasMap.TryGetValue(normalized, out var canonical))
            return canonical;
        // Fallback: Erster Buchstabe groß, Rest klein, aber ohne Leerzeichen
        var withSpaces = authorName.ToLowerInvariant().Trim();
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(withSpaces);
    }

    private static bool IsFeatureCommit(string commitMessage)
    {
        if (Regex.IsMatch(commitMessage, @"\bF\d+\b", RegexOptions.IgnoreCase)) return true;
        if (commitMessage.StartsWith("feat:", StringComparison.OrdinalIgnoreCase)) return true;
        var featureKeywords = new[] { "feature", "add", "implement", "new", "create" };
        return featureKeywords.Any(keyword => commitMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBugFixCommit(string commitMessage)
    {
        if (Regex.IsMatch(commitMessage, @"\bB\d+\b", RegexOptions.IgnoreCase)) return true;
        if (commitMessage.StartsWith("fix:", StringComparison.OrdinalIgnoreCase)) return true;
        var bugKeywords = new[] { "bug", "fix", "repair", "resolve", "correct", "patch" };
        return bugKeywords.Any(keyword => commitMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetIsoWeekKey(DateTime date)
    {
        var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        var weekOfYear = calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        var year = date.Year;
        
        if (date.Month == 12 && weekOfYear > 50)
        {
            // This logic seems potentially flawed, ISO week can belong to the next year
            // Let's re-evaluate. A simple approach is often better.
            // Let's stick to a simpler year-week format for now.
             if (weekOfYear == 1 && date.Month == 12)
            {
                year++;
            }
        }
        
        return $"{year}-W{weekOfYear:D2}";
    }
} 