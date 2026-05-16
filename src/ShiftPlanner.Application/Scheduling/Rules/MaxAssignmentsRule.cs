using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Rules;

public class MaxAssignmentsRule : ISchedulingRule
{
    public bool CanAssign(
        Employee employee,
        Shift shift,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee)
    {
        var currentAssignmentCount = plannedShifts
            .Count(existingShift => existingShift.EmployeeId == employee.Id);

        return currentAssignmentCount < maxAssignmentsPerEmployee;
    }
}