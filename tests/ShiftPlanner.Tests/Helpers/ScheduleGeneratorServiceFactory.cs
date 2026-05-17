using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling;
using ShiftPlanner.Application.Scheduling.Rules;

namespace ShiftPlanner.Tests.Helpers;

public static class ScheduleGeneratorServiceFactory
{
    public static ScheduleGeneratorService Create()
    {
        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();

        var rules = new List<ISchedulingRule>
        {
            new MaxAssignmentsRule(),
            new SameDayShiftRule(),
            new NightToDayRule(),
            new HighLoadRule(loadService, statusService)
        };

        return new ScheduleGeneratorService(
            loadService,
            rules);
    }
}