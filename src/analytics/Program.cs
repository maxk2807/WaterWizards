// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 115 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using WaterWizard.Analytics;
using WaterWizard.Analytics.Models;

namespace WaterWizard.Analytics.Program;

/// <summary>
/// Hauptprogramm für die Code-Analytics-Ausführung
/// Kann als Standalone-Tool oder als Pre-Merge-Hook verwendet werden
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("🚀 WaterWizards Code Analytics System");
        Console.WriteLine("=====================================");
        
        try
        {
            // Repository-Pfad bestimmen
            var repositoryPath = args.Length > 0 ? args[0] : ".";
            
            if (!Directory.Exists(repositoryPath))
            {
                Console.WriteLine($"❌ Repository-Pfad nicht gefunden: {repositoryPath}");
                return 1;
            }
            
            // Git-Repository prüfen
            if (!Directory.Exists(Path.Combine(repositoryPath, ".git")))
            {
                Console.WriteLine($"❌ Kein Git-Repository gefunden in: {repositoryPath}");
                return 1;
            }
            
            Console.WriteLine($"📁 Analysiere Repository: {Path.GetFullPath(repositoryPath)}");
            Console.WriteLine();
            
            // Analytics generieren
            var startTime = DateTime.UtcNow;
            var jsonReport = await CodeAnalytics.GenerateRepositoryAnalytics(repositoryPath);
            var endTime = DateTime.UtcNow;
            
            Console.WriteLine($"✅ Analytics erfolgreich generiert in {(endTime - startTime).TotalMilliseconds:F0}ms");
            
            // Report-Pfade anzeigen
            var analyticsDir = Path.Combine(repositoryPath, "analytics");
            Console.WriteLine($"📊 Reports gespeichert in: {analyticsDir}");
            Console.WriteLine($"   - analytics_latest.json (JSON-Format)");
            Console.WriteLine($"   - analytics_latest.md (Markdown-Format)");
            
            // Kurze Zusammenfassung anzeigen
            await ShowSummary(jsonReport);
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fehler bei der Analytics-Generierung: {ex.Message}");
            return 1;
        }
    }
    
    /// <summary>
    /// Zeigt eine kurze Zusammenfassung der Analytics an
    /// </summary>
    private static async Task ShowSummary(string jsonReport)
    {
        try
        {
            var analytics = System.Text.Json.JsonSerializer.Deserialize<RepositoryAnalytics>(
                jsonReport, 
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            
            if (analytics != null)
            {
                Console.WriteLine();
                Console.WriteLine("📈 Kurze Zusammenfassung:");
                Console.WriteLine("-------------------------");
                
                if (analytics.GitStatistics != null)
                {
                    Console.WriteLine($"📝 Commits: {analytics.GitStatistics.TotalCommits:N0}");
                    Console.WriteLine($"🌿 Branch: {analytics.GitStatistics.CurrentBranch}");
                    Console.WriteLine($"👤 Letzter Autor: {analytics.GitStatistics.LastCommitAuthor}");
                }
                
                if (analytics.CodeStatistics != null)
                {
                    Console.WriteLine($"📁 Dateien: {analytics.CodeStatistics.TotalFiles:N0}");
                    Console.WriteLine($"📄 Zeilen: {analytics.CodeStatistics.TotalLines:N0}");
                    Console.WriteLine($"💾 Größe: {analytics.CodeStatistics.TotalSize / 1024.0:F1} KB");
                }
                
                if (analytics.DeveloperStatistics != null && analytics.DeveloperStatistics.Any())
                {
                    var topDeveloper = analytics.DeveloperStatistics
                        .OrderByDescending(d => d.TotalCommits)
                        .First();
                    Console.WriteLine($"👑 Top Entwickler: {topDeveloper.Name} ({topDeveloper.TotalCommits:N0} Commits)");
                }
                
                if (analytics.QualityMetrics != null)
                {
                    Console.WriteLine($"🎯 Code/Comment Ratio: {analytics.QualityMetrics.CodeToCommentRatio:F1}");
                    Console.WriteLine($"📊 Durchschnittliche Zeilen pro Datei: {analytics.QualityMetrics.AverageLinesPerFile:F0}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Zusammenfassung konnte nicht angezeigt werden: {ex.Message}");
        }
    }
} 