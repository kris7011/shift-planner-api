namespace ShiftPlanner.Application.Scheduling.Rules;

public class SameDayShiftRule : ISchedulingRule
{
    public SchedulingRuleResult Evaluate(SchedulingRuleContext context)
    {
        var alreadyAssignedSameDay = context.PlannedShifts.Any(existingShift =>
            existingShift.EmployeeId == context.Employee.Id &&
            existingShift.Date == context.Shift.Date);

        if (alreadyAssignedSameDay)
        {
            return SchedulingRuleResult.Failed(
                "Employee is already assigned to a shift on the same day.");
        }

        return SchedulingRuleResult.Passed();
    }
}