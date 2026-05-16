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

            var schedule = scheduleGeneratorService.Generate(
                employees,
                openShifts: shifts.Where(shift => shift.EmployeeId == null).ToList(),
                existingShifts: shifts.Where(shift => shift.EmployeeId != null).ToList());

            foreach (var assignment in schedule.Where(x => x.WasAssigned && x.EmployeeId.HasValue))
            {
                var shift = shifts.First(x => x.Id == assignment.ShiftId);

                var employeeId = assignment.EmployeeId.GetValueOrDefault();

                shift.AssignToEmployee(employeeId);

                await shiftRepository.UpdateAsync(shift);
            }

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