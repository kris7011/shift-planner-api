using ShiftPlanner.Application.Scheduling.Simulation;

namespace ShiftPlanner.Tests.Helpers;

public static class ScheduleSimulationServiceFactory
{
    public static ScheduleSimulationService Create()
    {
        var scheduleGeneratorService = ScheduleGeneratorServiceFactory.Create();

        return new ScheduleSimulationService(scheduleGeneratorService);
    }
}