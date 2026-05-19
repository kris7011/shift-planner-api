namespace ShiftPlanner.Application.Scheduling.Overview;

public class UncoveredRequiredSkillOverview
{
    public string Skill { get; set; } = string.Empty;

    public int RequiredByUnassignedShifts { get; set; }

    public int AvailableEmployees { get; set; }
}