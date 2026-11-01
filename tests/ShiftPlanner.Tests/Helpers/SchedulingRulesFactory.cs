using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling.Rules;

namespace ShiftPlanner.Tests.Helpers;

/// <summary>The scheduling rules in the order the application registers them.</summary>
public static class SchedulingRulesFactory
{
    public static List<ISchedulingRule> Create()
    {
        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();

        return
        [
            new MaxAssignmentsRule(),
            new SameDayShiftRule(),
            new NightToDayRule(),
            new HighLoadRule(loadService, statusService)
        ];
    }
}
