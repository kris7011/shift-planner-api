using ShiftPlanner.Application.Scheduling.Overview;

namespace ShiftPlanner.Application.Scheduling.Simulation;

public class SimulateScheduleResponse
{
    public bool CanBeCovered { get; set; }

    public string RequiredSkill { get; set; } = string.Empty;

    public ScheduleRiskLevel RiskLevel { get; set; }

    public Guid? SuggestedEmployeeId { get; set; }

    public string? SuggestedEmployeeName { get; set; }

    public List<string> FailureReasons { get; set; } = [];
}