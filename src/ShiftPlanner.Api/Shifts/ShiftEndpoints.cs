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
            var shift = new Shift(
                request.Date,
                request.ShiftType,
                request.RequiredSkill,
                request.RequiredStaff
            );

            var createdShift = await repository.CreateAsync(shift);

            return Results.Ok(new
            {
                createdShift.Id,
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
                shift.Date,
                shift.ShiftType,
                shift.RequiredSkill,
                shift.RequiredStaff
            }));
        });

        return app;
    }
}

public record CreateShiftRequest(
    DateOnly Date,
    ShiftType ShiftType,
    string RequiredSkill,
    int RequiredStaff
);