using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Rules;

public class MaxAssignmentsRule : ISchedulingRule
{
    public SchedulingRuleResult Evaluate(
        Employee employee,
        Shift shift,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee)
    {
        var currentAssignmentCount = plannedShifts
            .Count(existingShift => existingShift.EmployeeId == employee.Id);

        if (currentAssignmentCount >= maxAssignmentsPerEmployee)
        {
            return SchedulingRuleResult.Failed(
                "Employee has reached the maximum number of assignments.");
        }

        return SchedulingRuleResult.Passed();
    }
}