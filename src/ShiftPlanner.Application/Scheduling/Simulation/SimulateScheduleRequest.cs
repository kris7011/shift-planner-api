using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Simulation;

public class SimulateScheduleRequest
{
    public DateOnly Date { get; set; }

    public ShiftType ShiftType { get; set; }

    public string RequiredSkill { get; set; } = string.Empty;

    public int RequiredStaff { get; set; } = 1;

    public int MaxAssignmentsPerEmployee { get; set; } = 5;
}