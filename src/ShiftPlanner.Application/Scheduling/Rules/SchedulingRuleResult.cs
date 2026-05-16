namespace ShiftPlanner.Application.Scheduling.Rules;

public class SchedulingRuleResult
{
    public bool Success { get; set; }

    public string? FailureReason { get; set; }

    public static SchedulingRuleResult Passed()
    {
        return new SchedulingRuleResult
        {
            Success = true
        };
    }

    public static SchedulingRuleResult Failed(string reason)
    {
        return new SchedulingRuleResult
        {
            Success = false,
            FailureReason = reason
        };
    }
}