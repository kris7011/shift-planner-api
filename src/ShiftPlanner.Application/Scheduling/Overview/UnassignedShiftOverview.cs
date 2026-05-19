using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Overview;

public class UnassignedShiftOverview
{
    public Guid ShiftId { get; set; }

    public DateOnly Date { get; set; }

    public ShiftType ShiftType { get; set; }

    public string RequiredSkill { get; set; } = string.Empty;

    public List<string> FailureReasons { get; set; } = [];
}