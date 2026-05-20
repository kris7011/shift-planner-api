using ShiftPlanner.Application.Scheduling.Overview;

namespace ShiftPlanner.Application.Scheduling.Simulation;

public class SimulationImpactIndicator
{
    public string Type { get; set; } = string.Empty;

    public ScheduleRiskLevel Severity { get; set; }

    public string Message { get; set; } = string.Empty;
}