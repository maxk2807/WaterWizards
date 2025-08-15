// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 119 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WaterWizard.Analytics.Models;

namespace WaterWizard.Analytics.Core;

public class CodeStatisticsCollector
{
    private readonly string _repositoryPath;
    private static readonly string[] CodeExtensions =
    {
        ".cs",
        ".csproj",
        ".sln",
        ".xml",
        ".json",
        ".md",
        ".yml",
        ".yaml",
    };
    private static readonly string[] IgnorePatterns =
    {
        "bin/",
        "obj/",
        ".git/",
        "node_modules/",
        ".vs/",
        ".idea/",
    };
    private static readonly string[] AnalyticsIgnoreFiles =
    {
        ".DS_Store",
        "Thumbs.db",
        "desktop.ini",
        "*.tmp",
        "*.log",
        "*.bak",
        "*.swp",
        "*.swo",
        "*~",
    };

    public CodeStatisticsCollector(string repositoryPath)
    {
        _repositoryPath = repositoryPath;
    }

    public CodeStatistics Collect()
    {
        var codeStats = new CodeStatistics();
        var allFiles = GetAllCodeFiles(_repositoryPath);

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

                var extension = Path.GetExtension(file).ToLower();
                if (!codeStats.FilesByType.ContainsKey(extension))
                    codeStats.FilesByType[extension] = 0;
                codeStats.FilesByType[extension]++;

                if (extension == ".cs")
                {
                    codeStats.CSharpFiles++;
                    codeStats.Classes += CountPattern(file, @"\bclass\s+\w+");
                    codeStats.Methods += CountPattern(
                        file,
                        @"\b(public|private|protected|internal)\s+\w+\s+\w+\s*\([^)]*\)\s*\{"
                    );
                    codeStats.Interfaces += CountPattern(file, @"\binterface\s+\w+");
                    codeStats.Properties += CountPattern(
                        file,
                        @"\b(public|private|protected|internal)\s+\w+\s+\w+\s*\{\s*(get|set)"
                    );
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(
                    $"[CodeAnalytics] Fehler bei der Analyse von {file}: {ex.Message}"
                );
            }
        }

        return codeStats;
    }

    private static IEnumerable<string> GetAllCodeFiles(string repositoryPath)
    {
        var files = new List<string>();
        foreach (var file in Directory.GetFiles(repositoryPath, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(repositoryPath, file);
            var fileName = Path.GetFileName(file);

            if (IgnorePatterns.Any(pattern => relativePath.Contains(pattern)))
                continue;

            if (
                AnalyticsIgnoreFiles.Any(pattern =>
                    fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase)
                    || (
                        pattern.StartsWith("*.")
                        && fileName.EndsWith(
                            pattern.Substring(1),
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
            )
                continue;

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
        };

        bool inMultiLineComment = false;
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine))
                analysis.EmptyLines++;
            else if (trimmedLine.StartsWith("//") || trimmedLine.StartsWith("///"))
                analysis.CommentLines++;
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
                analysis.CommentLines++;
            else
                analysis.CodeLines++;
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
}
