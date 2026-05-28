using ShiftPlanner.Application.Employees.Preferences;
using ShiftPlanner.Application.Scheduling.Simulation;

namespace ShiftPlanner.Tests.Helpers;

public static class ScheduleSimulationServiceFactory
{
    public static ScheduleSimulationService Create()
    {
        var scheduleGeneratorService = ScheduleGeneratorServiceFactory.Create();
        var preferenceProfileProvider = new EmployeePreferenceProfileProvider();
        var preferenceScoreService = new EmployeePreferenceScoreService();

        return new ScheduleSimulationService(
            scheduleGeneratorService,
            preferenceProfileProvider,
            preferenceScoreService);
    }
}