using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling;

namespace ShiftPlanner.Tests.Helpers;

public static class ScheduleGeneratorServiceFactory
{
    public static ScheduleGeneratorService Create()
    {
        return new ScheduleGeneratorService(
            new EmployeeLoadService(),
            SchedulingRulesFactory.Create());
    }
}