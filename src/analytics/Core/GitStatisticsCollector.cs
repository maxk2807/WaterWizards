// ===============================================
// Autoren-Statistik (automatisch generiert):
// - jdewi001: 51 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;
using System.Threading.Tasks;
using WaterWizard.Analytics.Models;

namespace WaterWizard.Analytics.Core;

public class GitStatisticsCollector
{
    private readonly GitCommandExecutor _gitCommandExecutor;

    public GitStatisticsCollector(string repositoryPath)
    {
        _gitCommandExecutor = new GitCommandExecutor(repositoryPath);
    }

    public async Task<GitStatistics> CollectAsync()
    {
        var stats = new GitStatistics();
        try
        {
            string gitStatus = "",
                totalCommitsStr = "",
                remoteBranchesStr = "",
                lastCommitDateStr = "",
                currentBranch = "",
                lastCommit = "",
                lastCommitMessage = "",
                lastCommitAuthor = "";

            try
            {
                gitStatus = await _gitCommandExecutor.ExecuteAsync("status --porcelain");
            }
            catch (Exception)
            { /* ignore */
            }
            try
            {
                totalCommitsStr = await _gitCommandExecutor.ExecuteAsync("rev-list --count HEAD");
            }
            catch (Exception)
            { /* ignore */
            }
            try
            {
                remoteBranchesStr = await _gitCommandExecutor.ExecuteAsync("branch -r");
            }
            catch (Exception)
            { /* ignore */
            }
            try
            {
                lastCommitDateStr = await _gitCommandExecutor.ExecuteAsync("log -1 --format=%ci");
            }
            catch (Exception)
            { /* ignore */
            }
            try
            {
                currentBranch = await _gitCommandExecutor.ExecuteAsync("branch --show-current");
            }
            catch (Exception)
            { /* ignore */
            }
            try
            {
                lastCommit = await _gitCommandExecutor.ExecuteAsync("log -1 --format=%H");
            }
            catch (Exception)
            { /* ignore */
            }
            try
            {
                lastCommitMessage = await _gitCommandExecutor.ExecuteAsync("log -1 --format=%s");
            }
            catch (Exception)
            { /* ignore */
            }
            try
            {
                lastCommitAuthor = await _gitCommandExecutor.ExecuteAsync("log -1 --format=%an");
            }
            catch (Exception)
            { /* ignore */
            }

            stats.UncommittedChanges = gitStatus
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Length;
            stats.CurrentBranch = currentBranch;
            stats.TotalCommits = int.TryParse(totalCommitsStr, out var tc) ? tc : 0;
            stats.TotalBranches = remoteBranchesStr
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Length;
            stats.LastCommit = lastCommit;
            stats.LastCommitMessage = lastCommitMessage;
            stats.LastCommitAuthor = lastCommitAuthor;
            stats.LastCommitDate = DateTime.TryParse(lastCommitDateStr, out var ld)
                ? ld
                : DateTime.MinValue;

            return stats;
        }
        catch (Exception ex)
        {
            // This is the final fallback catch block.
            Console.WriteLine(
                $"[CodeAnalytics] Unerwarteter Fehler bei der Git-Statistik-Sammlung: {ex.Message}"
            );
            return stats;
        }
    }
}
