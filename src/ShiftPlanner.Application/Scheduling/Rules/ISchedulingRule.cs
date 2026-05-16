using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Rules;

public interface ISchedulingRule
{
    bool CanAssign(
        Employee employee,
        Shift shift,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee);
}