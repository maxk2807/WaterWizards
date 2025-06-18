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
                    var date = DateTime.Parse(parts[1]);
                    var message = parts[2];
                    
                    if (!developerStats.ContainsKey(author))
                    {
                        developerStats[author] = new DeveloperStatistics
                        {
                            Name = author,
                            FirstCommit = date,
                            LastCommit = date,
                            TotalCommits = 0,
                            CommitMessages = new List<string>()
                        };
                    }
                    
                    var dev = developerStats[author];
                    dev.TotalCommits++;
                    dev.LastCommit = date;
                    dev.CommitMessages.Add(message);
                    
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
            
            analytics.DeveloperStatistics = developerStats.Values.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CodeAnalytics] Entwickler-Statistiken konnten nicht gesammelt werden: {ex.Message}");
        }
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
            }
        }
        
        // Entwickler-Aktivitäts-Metriken
        if (analytics.DeveloperStatistics != null && analytics.DeveloperStatistics.Any())
        {
            qualityMetrics.TotalDevelopers = analytics.DeveloperStatistics.Count;
            
            // Verbesserte Most Active Developer Berechnung
            // Gewichtete Bewertung: Commits (40%) + Features (30%) + Bugfixes (20%) + Dokumentation (10%)
            var developerScores = analytics.DeveloperStatistics.Select(dev => new
            {
                Developer = dev,
                Score = (dev.TotalCommits * 0.4) + 
                       (dev.FeatureCommits * 0.3) + 
                       (dev.BugFixCommits * 0.2) + 
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
                $"Docs: {developerScores.First().Developer.DocumentationCommits}" : "";
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
                report.AppendLine($"- **Total Commits**: {dev.TotalCommits:N0}");
                report.AppendLine($"- **First Commit**: {dev.FirstCommit:yyyy-MM-dd}");
                report.AppendLine($"- **Last Commit**: {dev.LastCommit:yyyy-MM-dd}");
                report.AppendLine($"- **Feature Commits**: {dev.FeatureCommits:N0}");
                report.AppendLine($"- **Bug Fix Commits**: {dev.BugFixCommits:N0}");
                report.AppendLine($"- **Refactor Commits**: {dev.RefactorCommits:N0}");
                report.AppendLine($"- **Documentation Commits**: {dev.DocumentationCommits:N0}");
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
            report.AppendLine($"- **Total Developers**: {metrics.TotalDevelopers}");
            report.AppendLine($"- **Most Active Developer**: {metrics.MostActiveDeveloper}");
            report.AppendLine($"- **Top Developer Score**: {metrics.TopDeveloperScore:F1}");
            report.AppendLine($"- **Top Developer Breakdown**: {metrics.TopDeveloperBreakdown}");
            report.AppendLine($"- **Average Commits Per Developer**: {metrics.AverageCommitsPerDeveloper:F1}");
            report.AppendLine();
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