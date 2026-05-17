namespace ShiftPlanner.Application.Scheduling.Rules;

public class MaxAssignmentsRule : ISchedulingRule
{
    public SchedulingRuleResult Evaluate(SchedulingRuleContext context)
    {
        var currentAssignmentCount = context.PlannedShifts
            .Count(existingShift => existingShift.EmployeeId == context.Employee.Id);

        if (currentAssignmentCount >= context.MaxAssignmentsPerEmployee)
        {
            return SchedulingRuleResult.Failed(
                "Employee has reached the maximum number of assignments.");
        }

        return SchedulingRuleResult.Passed();
    }
}