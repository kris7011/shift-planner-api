using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Api.LoadAnalysis;

public static class LoadAnalysisEndpoints
{
    public static void MapLoadAnalysisEndpoints(this WebApplication app)
    {
        app.MapPost(
            "/api/load-analysis",
            (
                LoadAnalysisRequest request,
                EmployeeLoadAnalysisService analysisService) =>
            {
                var employee = new Employee(
                    request.EmployeeName,
                    request.Skills);

                var shifts = new List<Shift>();

                foreach (var shiftRequest in request.Shifts)
                {
                    var shift = new Shift(
                        shiftRequest.Date,
                        shiftRequest.ShiftType,
                        shiftRequest.RequiredSkill,
                        shiftRequest.RequiredStaff);

                    if (shiftRequest.AssignEmployee)
                    {
                        shift.AssignEmployee(employee);
                    }

                    shifts.Add(shift);
                }

                var result = analysisService.Analyze(
                    employee,
                    shifts,
                    request.Threshold);

                return Results.Ok(result);
            });
    }
}