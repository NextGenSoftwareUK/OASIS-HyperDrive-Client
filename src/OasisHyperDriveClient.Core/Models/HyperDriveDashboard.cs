namespace OasisHyperDriveClient.Core.Models;

public class DashboardData
{
    public DateTime Timestamp { get; set; }
    public int ActiveProviders { get; set; }
    public int TotalRequests { get; set; }
    public double SystemHealth { get; set; }
    public PerformanceMetrics? PerformanceMetrics { get; set; }
    public CostMetrics? CostMetrics { get; set; }
    public List<DashboardAlert> Alerts { get; set; } = [];
    public List<Trend> Trends { get; set; } = [];
}

public class PerformanceMetrics
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public double Throughput { get; set; }
}

public class CostMetrics
{
    public double TotalCost { get; set; }
    public double AverageCost { get; set; }
    public double CostPerRequest { get; set; }
}

public class DashboardAlert
{
    public string ProviderType { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class Trend
{
    public string ProviderType { get; set; } = string.Empty;
    public double PerformanceTrend { get; set; }
    public double CostTrend { get; set; }
    public double ReliabilityTrend { get; set; }
}

public class ProviderPerformanceMetrics
{
    public string ProviderType { get; set; } = string.Empty;
    public double ResponseTimeMs { get; set; }
    public double ThroughputMbps { get; set; }
    public double ErrorRate { get; set; }
    public double UptimePercentage { get; set; }
    public int ActiveConnections { get; set; }
    public double CostPerOperation { get; set; }
    public DateTime LastUpdated { get; set; }
    public double OverallScore { get; set; }

    public bool IsHealthy => ErrorRate < 0.03 && UptimePercentage > 95;

    public string StatusColour => IsHealthy
        ? "#00FFEE"
        : ErrorRate > 0.1 ? "#FF3333" : "#FFD700";
}

public class HyperDriveConfig
{
    public bool IsEnabled { get; set; }
    public bool AutoFailoverEnabled { get; set; }
    public bool AutoReplicationEnabled { get; set; }
    public bool AutoLoadBalancingEnabled { get; set; }
    public List<string> EnabledProviders { get; set; } = [];
}

public class HyperDriveRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";   // Info | Warning | Critical
    public string ProviderType { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
}
