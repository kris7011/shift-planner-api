using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling.Analysis;
using ShiftPlanner.Application.Shifts;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Api.Shifts;

public static class ShiftEndpoints
{
    public static IEndpointRouteBuilder MapShiftEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/shifts", async (
            CreateShiftRequest request,
            IShiftRepository repository) =>
        {
            if (!Shift.IsSupportedRequiredStaff(request.RequiredStaff))
            {
                return Results.BadRequest(new
                {
                    error = ShiftStaffingErrors.UnsupportedRequiredStaff
                });
            }

            var shift = new Shift(
                request.Date,
                request.ShiftType,
                request.RequiredSkill,
                request.RequiredStaff,
                request.EmployeeId
            );

            var createdShift = await repository.CreateAsync(shift);

            return Results.Ok(new
            {
                createdShift.Id,
                createdShift.EmployeeId,
                createdShift.Date,
                createdShift.ShiftType,
                createdShift.RequiredSkill,
                createdShift.RequiredStaff
            });
        });

        app.MapGet("/api/shifts", async (
            IShiftRepository repository) =>
        {
            var shifts = await repository.GetAllAsync();

            return Results.Ok(shifts.Select(shift => new
            {
                shift.Id,
                shift.EmployeeId,
                shift.Date,
                shift.ShiftType,
                shift.RequiredSkill,
                shift.RequiredStaff
            }));
        });

        app.MapGet("/api/shifts/{id:guid}/assignment-analysis", async (
            Guid id,
            int? maxAssignmentsPerEmployee,
            IShiftRepository shiftRepository,
            IEmployeeRepository employeeRepository,
            ShiftAssignmentAnalysisService shiftAssignmentAnalysisService) =>
        {
            var shifts = await shiftRepository.GetAllAsync();
            var shift = shifts.FirstOrDefault(shift => shift.Id == id);

            if (shift == null)
            {
                return Results.NotFound(new
                {
                    error = "Shift was not found."
                });
            }

            var employees = await employeeRepository.GetAllAsync();

            var analysis = shiftAssignmentAnalysisService.Analyze(
                shift,
                employees.ToList(),
                shifts.ToList(),
                maxAssignmentsPerEmployee: maxAssignmentsPerEmployee ?? 5);

            return Results.Ok(analysis);
        });

        return app;
    }
}

public record CreateShiftRequest(
    DateOnly Date,
    ShiftType ShiftType,
    string RequiredSkill,
    int RequiredStaff = Shift.SupportedRequiredStaff,
    Guid? EmployeeId = null
);

public static class ShiftStaffingErrors
{
    public const string UnsupportedRequiredStaff =
        "RequiredStaff must be 1. Each shift is staffed by one employee; create one shift per required employee.";
}