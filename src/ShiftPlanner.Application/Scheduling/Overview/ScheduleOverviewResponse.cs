namespace ShiftPlanner.Application.Scheduling.Overview;

public class ScheduleOverviewResponse
{
    public int TotalShifts { get; set; }

    public int AssignedShifts { get; set; }

    public int UnassignedShifts { get; set; }

    public decimal CoverageRate { get; set; }

    public int EmployeeCount { get; set; }

    public int HighRiskEmployeeCount { get; set; }

    public List<UnassignedShiftOverview> UnassignedShiftDetails { get; set; } = [];

    public List<SkillGapOverview> SkillGaps { get; set; } = [];

    public ScheduleRiskSummary RiskSummary { get; set; } = new();

    public List<RiskIndicator> RiskIndicators { get; set; } = [];
}