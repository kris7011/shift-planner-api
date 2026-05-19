namespace ShiftPlanner.Application.Scheduling.Overview;

public class ScheduleRiskSummary
{
    public ScheduleRiskLevel CoverageRisk { get; set; }

    public int UnassignedShiftCount { get; set; }

    public int SkillGapCount { get; set; }

    public int HighRiskEmployeeCount { get; set; }
}