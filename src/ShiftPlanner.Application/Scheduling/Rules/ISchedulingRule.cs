namespace ShiftPlanner.Application.Scheduling.Rules;

public interface ISchedulingRule
{
    SchedulingRuleResult Evaluate(SchedulingRuleContext context);
}