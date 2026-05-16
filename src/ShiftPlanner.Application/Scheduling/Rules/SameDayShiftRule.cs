using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Rules;

public class SameDayShiftRule : ISchedulingRule
{
    public SchedulingRuleResult Evaluate(
        Employee employee,
        Shift shift,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee)
    {
        var alreadyAssignedSameDay = plannedShifts.Any(existingShift =>
            existingShift.EmployeeId == employee.Id &&
            existingShift.Date == shift.Date);

        if (alreadyAssignedSameDay)
        {
            return SchedulingRuleResult.Failed(
                "Employee is already assigned to a shift on the same day.");
        }

        return SchedulingRuleResult.Passed();
    }
}