using System.Text.Json;
using System.Text.RegularExpressions;

namespace WaterWizard.Analytics;

/// <summary>
/// Code Analytics System für Repository-Statistiken
/// Generiert detaillierte Berichte über Code-Qualität, Entwickler-Aktivität und Projekt-Metriken
/// </summary>
public static class CodeAnalytics
{
    private static readonly string[] CodeExtensions = { ".cs", ".csproj", ".sln", ".xml", ".json", ".md", ".yml", ".yaml" };
    private static readonly string[] IgnorePatterns = { "bin/", "obj/", ".git/", "node_modules/", ".vs/", ".idea/" };
    
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
            await CollectGitStatistics(repositoryPath, analytics);
            
            // Code-Statistiken sammeln
            CollectCodeStatistics(repositoryPath, analytics);
            
            // Entwickler-Statistiken sammeln
            await CollectDeveloperStatistics(repositoryPath, analytics);
            
            // Qualitäts-Metriken berechnen
            CalculateQualityMetrics(analytics);
            
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
    /// Sammelt Git-Statistiken über Commits, Branches und Tags
    /// </summary>
    private static async Task CollectGitStatistics(string repositoryPath, RepositoryAnalytics analytics)
    {
        try
        {
            // Git-Status abrufen
            var gitStatus = await ExecuteGitCommand(repositoryPath, "status --porcelain");
            analytics.GitStatistics = new GitStatistics
            {
                UncommittedChanges = gitStatus.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length,
                CurrentBranch = await ExecuteGitCommand(repositoryPath, "branch --show-current"),
                TotalCommits = int.Parse(await ExecuteGitCommand(repositoryPath, "rev-list --count HEAD")),
                TotalBranches = int.Parse(await ExecuteGitCommand(repositoryPath, "branch -r | wc -l")),
                LastCommit = await ExecuteGitCommand(repositoryPath, "log -1 --format=%H"),
                LastCommitMessage = await ExecuteGitCommand(repositoryPath, "log -1 --format=%s"),
                LastCommitAuthor = await ExecuteGitCommand(repositoryPath, "log -1 --format=%an"),
                LastCommitDate = DateTime.Parse(await ExecuteGitCommand(repositoryPath, "log -1 --format=%ci"))
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CodeAnalytics] Git-Statistiken konnten nicht gesammelt werden: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Sammelt Code-Statistiken über Dateien, Zeilen und Komplexität
    /// </summary>
    private static void CollectCodeStatistics(string repositoryPath, RepositoryAnalytics analytics)
    {
        var codeStats = new CodeStatistics();
        var allFiles = GetAllCodeFiles(repositoryPath);
        
        foreach (var file in allFiles)
        {
            try
            {
                var fileInfo = AnalyzeFile(file);
                codeStats.TotalFiles++;
                codeStats.TotalLines += fileInfo.TotalLines;
                codeStats.CodeLines += fileInfo.CodeLines;
                codeStats.CommentLines += fileInfo.CommentLines;
                codeStats.EmptyLines += fileInfo.EmptyLines;
                codeStats.TotalSize += fileInfo.FileSize;
                
                // Dateityp-spezifische Statistiken
                var extension = Path.GetExtension(file).ToLower();
                if (!codeStats.FilesByType.ContainsKey(extension))
                    codeStats.FilesByType[extension] = 0;
                codeStats.FilesByType[extension]++;
                
                // Komplexitäts-Metriken
                if (extension == ".cs")
                {
                    codeStats.CSharpFiles++;
                    codeStats.Classes += CountPattern(file, @"\bclass\s+\w+");
                    codeStats.Methods += CountPattern(file, @"\b(public|private|protected|internal)\s+\w+\s+\w+\s*\([^)]*\)\s*\{");
                    codeStats.Interfaces += CountPattern(file, @"\binterface\s+\w+");
                    codeStats.Properties += CountPattern(file, @"\b(public|private|protected|internal)\s+\w+\s+\w+\s*\{\s*(get|set)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CodeAnalytics] Fehler bei der Analyse von {file}: {ex.Message}");
            }
        }
        
        analytics.CodeStatistics = codeStats;
    }
    
    /// <summary>
    /// Sammelt Entwickler-Statistiken über Commits und Aktivität
    /// </summary>
    private static async Task CollectDeveloperStatistics(string repositoryPath, RepositoryAnalytics analytics)
    {
        try
        {
            // Mappe alle Originalnamen auf den normierten Namen
            var nameToNormalized = new Dictionary<string, string>();
            var normalizedToAliases = new Dictionary<string, HashSet<string>>();
            var developerStats = new Dictionary<string, DeveloperStatistics>();
            
            // Git-Log für Entwickler-Statistiken
            var gitLog = await ExecuteGitCommand(repositoryPath, "log --pretty=format:\"%an|%ad|%s\" --date=short");
            var commits = gitLog.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var commit in commits)
            {
                var parts = commit.Split('|');
                if (parts.Length >= 3)
                {
                    var author = parts[0];
                    var dateStr = parts[1];
                    var message = parts[2];
                    
                    // Korrektes Datum parsen
                    if (!DateTime.TryParse(dateStr, out var date))
                    {
                        Console.WriteLine($"[CodeAnalytics] Konnte Datum nicht parsen: {dateStr}");
                        continue;
                    }
                    
                    // Normalisiere den Autorennamen für bessere Gruppierung
                    var normalizedAuthor = NormalizeAuthorName(author);
                    nameToNormalized[author] = normalizedAuthor;
                    if (!normalizedToAliases.ContainsKey(normalizedAuthor))
                        normalizedToAliases[normalizedAuthor] = new HashSet<string>();
                    normalizedToAliases[normalizedAuthor].Add(author);
                    
                    if (!developerStats.ContainsKey(normalizedAuthor))
                    {
                        developerStats[normalizedAuthor] = new DeveloperStatistics
                        {
                            Name = normalizedAuthor,
                            OriginalNames = new List<string>(),
                            FirstCommit = date,
                            LastCommit = date,
                            TotalCommits = 0,
                            CommitMessages = new List<string>(),
                            WeeklyActivity = new Dictionary<string, int>(),
                            MonthlyActivity = new Dictionary<string, int>()
                        };
                    }
                    
                    var dev = developerStats[normalizedAuthor];
                    dev.TotalCommits++;
                    
                    // Korrekte First/Last Commit Logik
                    if (date < dev.FirstCommit)
                        dev.FirstCommit = date;
                    if (date > dev.LastCommit)
                        dev.LastCommit = date;
                    
                    dev.CommitMessages.Add(message);
                    
                    // Wöchentliche und monatliche Aktivität mit korrektem ISO-Format
                    var weekKey = GetIsoWeekKey(date);
                    var monthKey = date.ToString("yyyy-MM");
                    
                    if (!dev.WeeklyActivity.ContainsKey(weekKey))
                        dev.WeeklyActivity[weekKey] = 0;
                    if (!dev.MonthlyActivity.ContainsKey(monthKey))
                        dev.MonthlyActivity[monthKey] = 0;
                    
                    dev.WeeklyActivity[weekKey]++;
                    dev.MonthlyActivity[monthKey]++;
                    
                    // Commit-Typ-Analyse basierend auf WaterWizards-Konventionen
                    if (IsFeatureCommit(message))
                        dev.FeatureCommits++;
                    else if (IsBugFixCommit(message))
                        dev.BugFixCommits++;
                    else if (message.StartsWith("refactor:", StringComparison.OrdinalIgnoreCase))
                        dev.RefactorCommits++;
                    else if (message.StartsWith("docs:", StringComparison.OrdinalIgnoreCase))
                        dev.DocumentationCommits++;
                }
            }
            // Aliase zuordnen (alle Originalnamen, die auf denselben normierten Namen mappen)
            foreach (var kv in developerStats)
            {
                if (normalizedToAliases.ContainsKey(kv.Key))
                    kv.Value.OriginalNames = normalizedToAliases[kv.Key].OrderBy(x => x).ToList();
            }
            analytics.DeveloperStatistics = developerStats.Values.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CodeAnalytics] Entwickler-Statistiken konnten nicht gesammelt werden: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Normalisiert Autorennamen für bessere Gruppierung ähnlicher Namen
    /// </summary>
    private static string NormalizeAuthorName(string authorName)
    {
        var normalized = authorName.ToLowerInvariant().Trim();
        
        // Alle Varianten von Erick auf exakt "Erick Zeiler" mappen
        if (normalized == "erick" || normalized == "erickk0" || normalized == "erick zeiler")
            return "Erick Zeiler";
        
        // Spezielle Fälle für bekannte Entwickler
        var nameMappings = new Dictionary<string, string>
        {
            { "justin", "Justin Dewitz" },
            { "justinjd00", "Justin Dewitz" },
            { "justindew", "Justin Dewitz" },
            { "jdewi001", "Justin Dewitz" },
            { "max", "Max Kondratov" },
            { "maxk2807", "Max Kondratov" },
            { "maxkondratov", "Max Kondratov" },
            { "julian", "Julian" },
            { "jlnhsrm", "Julian" },
            { "paul", "Paul" }
        };
        if (nameMappings.ContainsKey(normalized))
            return nameMappings[normalized];
        return char.ToUpperInvariant(authorName[0]) + authorName.Substring(1).ToLowerInvariant();
    }
    
    /// <summary>
    /// Prüft ob ein Commit ein Feature-Commit ist (F{TicketNumber} Konvention)
    /// </summary>
    private static bool IsFeatureCommit(string commitMessage)
    {
        // WaterWizards Feature-Konvention: F{TicketNumber}
        if (Regex.IsMatch(commitMessage, @"\bF\d+\b", RegexOptions.IgnoreCase))
            return true;
            
        // Standard-Konventionen als Fallback
        if (commitMessage.StartsWith("feat:", StringComparison.OrdinalIgnoreCase))
            return true;
            
        // Feature-bezogene Keywords
        var featureKeywords = new[] { "feature", "add", "implement", "new", "create" };
        return featureKeywords.Any(keyword => 
            commitMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Prüft ob ein Commit ein Bugfix-Commit ist (B{TicketNumber} Konvention)
    /// </summary>
    private static bool IsBugFixCommit(string commitMessage)
    {
        // WaterWizards Bug-Konvention: B{TicketNumber}
        if (Regex.IsMatch(commitMessage, @"\bB\d+\b", RegexOptions.IgnoreCase))
            return true;
            
        // Standard-Konventionen als Fallback
        if (commitMessage.StartsWith("fix:", StringComparison.OrdinalIgnoreCase))
            return true;
            
        // Bugfix-bezogene Keywords
        var bugKeywords = new[] { "bug", "fix", "repair", "resolve", "correct", "patch" };
        return bugKeywords.Any(keyword => 
            commitMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Berechnet Qualitäts-Metriken für das Projekt
    /// </summary>
    private static void CalculateQualityMetrics(RepositoryAnalytics analytics)
    {
        var qualityMetrics = new QualityMetrics();
        
        if (analytics.CodeStatistics != null)
        {
            var stats = analytics.CodeStatistics;
            
            // Code-Qualitäts-Metriken
            qualityMetrics.CodeToCommentRatio = stats.CommentLines > 0 ? (double)stats.CodeLines / stats.CommentLines : 0;
            qualityMetrics.EmptyLinesPercentage = stats.TotalLines > 0 ? (double)stats.EmptyLines / stats.TotalLines * 100 : 0;
            qualityMetrics.AverageFileSize = stats.TotalFiles > 0 ? stats.TotalSize / stats.TotalFiles : 0;
            qualityMetrics.AverageLinesPerFile = stats.TotalFiles > 0 ? stats.TotalLines / stats.TotalFiles : 0;
            
            // Komplexitäts-Metriken
            if (stats.CSharpFiles > 0)
            {
                qualityMetrics.AverageMethodsPerClass = stats.Classes > 0 ? (double)stats.Methods / stats.Classes : 0;
                qualityMetrics.AveragePropertiesPerClass = stats.Classes > 0 ? (double)stats.Properties / stats.Classes : 0;
                qualityMetrics.CodeComplexity = stats.Methods > 0 ? (double)stats.CodeLines / stats.Methods : 0;
                qualityMetrics.DocumentationCoverage = stats.Classes > 0 ? (double)stats.CommentLines / stats.Classes : 0;
            }
        }
        
        // Entwickler-Aktivitäts-Metriken
        if (analytics.DeveloperStatistics != null && analytics.DeveloperStatistics.Any())
        {
            qualityMetrics.TotalDevelopers = analytics.DeveloperStatistics.Count;
            
            // Erweiterte Entwickler-Statistiken berechnen
            foreach (var dev in analytics.DeveloperStatistics)
            {
                // Commit-Percentages
                dev.FeatureCommitPercentage = dev.TotalCommits > 0 ? (double)dev.FeatureCommits / dev.TotalCommits * 100 : 0;
                dev.BugFixCommitPercentage = dev.TotalCommits > 0 ? (double)dev.BugFixCommits / dev.TotalCommits * 100 : 0;
                dev.RefactorCommitPercentage = dev.TotalCommits > 0 ? (double)dev.RefactorCommits / dev.TotalCommits * 100 : 0;
                dev.DocumentationCommitPercentage = dev.TotalCommits > 0 ? (double)dev.DocumentationCommits / dev.TotalCommits * 100 : 0;
                
                // Zeit-basierte Statistiken
                var now = DateTime.Now;
                dev.DaysSinceFirstCommit = (now - dev.FirstCommit).Days;
                dev.DaysSinceLastCommit = (now - dev.LastCommit).Days;
                dev.CommitFrequency = dev.DaysSinceFirstCommit > 0 ? (double)dev.TotalCommits / dev.DaysSinceFirstCommit : 0;
                
                // Wöchentliche und monatliche Durchschnitte
                dev.AverageCommitsPerWeek = dev.WeeklyActivity.Count > 0 ? dev.WeeklyActivity.Values.Average() : 0;
                dev.AverageCommitsPerMonth = dev.MonthlyActivity.Count > 0 ? dev.MonthlyActivity.Values.Average() : 0;
                
                // Aktivste Perioden
                dev.MostActiveWeek = dev.WeeklyActivity.Count > 0 ? 
                    dev.WeeklyActivity.OrderByDescending(x => x.Value).First().Key : "";
                dev.MostActiveMonth = dev.MonthlyActivity.Count > 0 ? 
                    dev.MonthlyActivity.OrderByDescending(x => x.Value).First().Key : "";
                dev.MaxCommitsInWeek = dev.WeeklyActivity.Count > 0 ? dev.WeeklyActivity.Values.Max() : 0;
                dev.MaxCommitsInMonth = dev.MonthlyActivity.Count > 0 ? dev.MonthlyActivity.Values.Max() : 0;
            }
            
            // Verbesserte Most Active Developer Berechnung
            // Gewichtete Bewertung: Commits (30%) + Features (25%) + Bugfixes (20%) + Aktivität (15%) + Dokumentation (10%)
            var developerScores = analytics.DeveloperStatistics.Select(dev => new
            {
                Developer = dev,
                Score = (dev.TotalCommits * 0.3) + 
                       (dev.FeatureCommits * 0.25) + 
                       (dev.BugFixCommits * 0.2) + 
                       (dev.AverageCommitsPerWeek * 0.15) + 
                       (dev.DocumentationCommits * 0.1)
            }).OrderByDescending(x => x.Score).ToList();
            
            qualityMetrics.MostActiveDeveloper = developerScores.FirstOrDefault()?.Developer.Name ?? "Unknown";
            qualityMetrics.AverageCommitsPerDeveloper = analytics.DeveloperStatistics.Average(d => d.TotalCommits);
            
            // Zusätzliche Metriken für bessere Bewertung
            qualityMetrics.TopDeveloperScore = developerScores.FirstOrDefault()?.Score ?? 0;
            qualityMetrics.TopDeveloperBreakdown = developerScores.FirstOrDefault()?.Developer != null ? 
                $"Commits: {developerScores.First().Developer.TotalCommits}, " +
                $"Features: {developerScores.First().Developer.FeatureCommits}, " +
                $"Bugfixes: {developerScores.First().Developer.BugFixCommits}, " +
                $"Weekly Avg: {developerScores.First().Developer.AverageCommitsPerWeek:F1}, " +
                $"Docs: {developerScores.First().Developer.DocumentationCommits}" : "";
            
            // Projekt-weite Statistiken
            var allWeeks = analytics.DeveloperStatistics
                .SelectMany(d => d.WeeklyActivity)
                .GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
            
            var allMonths = analytics.DeveloperStatistics
                .SelectMany(d => d.MonthlyActivity)
                .GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
            
            qualityMetrics.MostActiveWeek = allWeeks.Count > 0 ? 
                allWeeks.OrderByDescending(x => x.Value).First().Key : "";
            qualityMetrics.MostActiveMonth = allMonths.Count > 0 ? 
                allMonths.OrderByDescending(x => x.Value).First().Key : "";
            
            // Repository-Alter berechnen
            var firstCommit = analytics.DeveloperStatistics.Min(d => d.FirstCommit);
            var lastCommit = analytics.DeveloperStatistics.Max(d => d.LastCommit);
            var repositoryAge = (lastCommit - firstCommit).Days;
            qualityMetrics.RepositoryAge = $"{repositoryAge} days ({repositoryAge / 365.25:F1} years)";
            
            // Projekt-Velocity (Commits pro Tag im Durchschnitt)
            qualityMetrics.ProjectVelocity = repositoryAge > 0 ? 
                (double)analytics.DeveloperStatistics.Sum(d => d.TotalCommits) / repositoryAge : 0;
        }
        
        analytics.QualityMetrics = qualityMetrics;
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
    
    #region Helper Methods
    
    private static async Task<string> ExecuteGitCommand(string repositoryPath, string command)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = repositoryPath,
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
    
    private static IEnumerable<string> GetAllCodeFiles(string repositoryPath)
    {
        var files = new List<string>();
        
        foreach (var file in Directory.GetFiles(repositoryPath, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(repositoryPath, file);
            var fileName = Path.GetFileName(file);
            
            // Ignoriere bestimmte Verzeichnisse
            if (IgnorePatterns.Any(pattern => relativePath.Contains(pattern)))
                continue;
                
            // Ignoriere unnötige Dateien
            if (AnalyticsIgnoreFiles.Any(pattern => 
                fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase) ||
                (pattern.StartsWith("*.") && fileName.EndsWith(pattern.Substring(1), StringComparison.OrdinalIgnoreCase))))
                continue;
            
            // Nur Code-Dateien
            if (CodeExtensions.Contains(Path.GetExtension(file).ToLower()))
            {
                files.Add(file);
            }
        }
        
        return files;
    }
    
    private static FileAnalysis AnalyzeFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var analysis = new FileAnalysis
        {
            FileSize = new FileInfo(filePath).Length,
            TotalLines = lines.Length,
            CodeLines = 0,
            CommentLines = 0,
            EmptyLines = 0
        };
        
        bool inMultiLineComment = false;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            if (string.IsNullOrEmpty(trimmedLine))
            {
                analysis.EmptyLines++;
            }
            else if (trimmedLine.StartsWith("//") || trimmedLine.StartsWith("///"))
            {
                analysis.CommentLines++;
            }
            else if (trimmedLine.StartsWith("/*"))
            {
                analysis.CommentLines++;
                inMultiLineComment = true;
            }
            else if (trimmedLine.Contains("*/"))
            {
                analysis.CommentLines++;
                inMultiLineComment = false;
            }
            else if (inMultiLineComment)
            {
                analysis.CommentLines++;
            }
            else
            {
                analysis.CodeLines++;
            }
        }
        
        return analysis;
    }
    
    private static int CountPattern(string filePath, string pattern)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            return Regex.Matches(content, pattern, RegexOptions.Multiline).Count;
        }
        catch
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Berechnet den ISO-Wochen-Schlüssel für ein Datum
    /// </summary>
    private static string GetIsoWeekKey(DateTime date)
    {
        // Einfache ISO-Woche-Berechnung
        var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        var weekOfYear = calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        var year = date.Year;
        
        // Wenn wir in der letzten Woche des Jahres sind, aber die Woche gehört zum nächsten Jahr
        if (date.Month == 12 && weekOfYear > 50)
        {
            year++;
            weekOfYear = 1;
        }
        
        return $"{year}-W{weekOfYear:D2}";
    }
    
    #endregion
}

#region Data Models

public class RepositoryAnalytics
{
    public DateTime GeneratedAt { get; set; }
    public string RepositoryPath { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public GitStatistics? GitStatistics { get; set; }
    public CodeStatistics? CodeStatistics { get; set; }
    public List<DeveloperStatistics>? DeveloperStatistics { get; set; }
    public QualityMetrics? QualityMetrics { get; set; }
}

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

public class CodeStatistics
{
    public int TotalFiles { get; set; }
    public int TotalLines { get; set; }
    public int CodeLines { get; set; }
    public int CommentLines { get; set; }
    public int EmptyLines { get; set; }
    public long TotalSize { get; set; }
    public Dictionary<string, int> FilesByType { get; set; } = new();
    public int CSharpFiles { get; set; }
    public int Classes { get; set; }
    public int Methods { get; set; }
    public int Interfaces { get; set; }
    public int Properties { get; set; }
}

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

public class FileAnalysis
{
    public long FileSize { get; set; }
    public int TotalLines { get; set; }
    public int CodeLines { get; set; }
    public int CommentLines { get; set; }
    public int EmptyLines { get; set; }
}

#endregion 