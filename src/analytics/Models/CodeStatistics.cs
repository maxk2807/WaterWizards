using System.Collections.Generic;

namespace WaterWizard.Analytics.Models;

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