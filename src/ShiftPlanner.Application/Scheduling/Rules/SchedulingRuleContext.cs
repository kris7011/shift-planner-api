using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Rules;

public class SchedulingRuleContext
{
    public Employee Employee { get; set; } = null!;

    public Shift Shift { get; set; } = null!;

    public List<Shift> PlannedShifts { get; set; } = [];

    public int MaxAssignmentsPerEmployee { get; set; }
}