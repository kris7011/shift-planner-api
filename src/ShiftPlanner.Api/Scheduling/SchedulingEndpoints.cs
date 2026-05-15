using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling;
using ShiftPlanner.Application.Shifts;

namespace ShiftPlanner.Api.Scheduling;

public static class SchedulingEndpoints
{
    public static void MapSchedulingEndpoints(this WebApplication app)
    {
        app.MapPost("/api/schedule/generate", async (
            GenerateScheduleRequest request,
            IScheduleGeneratorService scheduleGeneratorService,
            IEmployeeRepository employeeRepository,
            IShiftRepository shiftRepository) =>
        {
            var employees = await employeeRepository.GetAllAsync();
            var shifts = await shiftRepository.GetAllAsync();

            var schedule = scheduleGeneratorService.Generate(employees, shifts);

            return Results.Ok(new
            {
                message = "Schedule generation completed.",
                employeeCount = employees.Count,
                shiftCount = shifts.Count,
                assignments = schedule
            });
        });
    }
}

public class GenerateScheduleRequest
{
}