namespace ShiftPlanner.Application.Scheduling.Overview;

public class RiskIndicator
{
    public string Type { get; set; } = string.Empty;

    public ScheduleRiskLevel Severity { get; set; }

    public string Message { get; set; } = string.Empty;
}