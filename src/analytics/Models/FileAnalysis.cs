namespace WaterWizard.Analytics.Models;

public class FileAnalysis
{
    public long FileSize { get; set; }
    public int TotalLines { get; set; }
    public int CodeLines { get; set; }
    public int CommentLines { get; set; }
    public int EmptyLines { get; set; }
} 