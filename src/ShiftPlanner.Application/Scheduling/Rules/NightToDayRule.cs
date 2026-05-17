using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Rules;

public class NightToDayRule : ISchedulingRule
{
    public SchedulingRuleResult Evaluate(SchedulingRuleContext context)
    {
        var hasNightShiftBefore = context.PlannedShifts.Any(existingShift =>
            existingShift.EmployeeId == context.Employee.Id &&
            existingShift.ShiftType == ShiftType.Night &&
            existingShift.Date.AddDays(1) == context.Shift.Date &&
            context.Shift.ShiftType == ShiftType.Day);

        if (hasNightShiftBefore)
        {
            return SchedulingRuleResult.Failed(
                "Employee cannot be assigned to a day shift directly after a night shift.");
        }

        return SchedulingRuleResult.Passed();
    }
}