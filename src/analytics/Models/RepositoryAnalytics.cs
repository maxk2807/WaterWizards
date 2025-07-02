using System;
using System.Collections.Generic;

namespace WaterWizard.Analytics.Models;

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