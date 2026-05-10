using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Shifts;
using ShiftPlanner.Domain.Employees;

namespace ShiftPlanner.Api.Employees;

public static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints(this WebApplication app)
    {
        app.MapPost("/api/employees", async (
            CreateEmployeeRequest request,
            IEmployeeRepository employeeRepository) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new
                {
                    error = "Employee name is required."
                });
            }

            if (request.Skills.Count == 0)
            {
                return Results.BadRequest(new
                {
                    error = "At least one skill is required."
                });
            }

            var employee = new Employee(request.Name, request.Skills);

            var createdEmployee = await employeeRepository.AddAsync(employee);

            return Results.Created($"/api/employees/{createdEmployee.Id}", new
            {
                createdEmployee.Id,
                createdEmployee.Name,
                createdEmployee.Skills
            });
        });

        app.MapGet("/api/employees", async (
            IEmployeeRepository employeeRepository) =>
        {
            var employees = await employeeRepository.GetAllAsync();

            return Results.Ok(employees.Select(employee => new
            {
                employee.Id,
                employee.Name,
                employee.Skills
            }));
        });

        app.MapGet("/api/employees/{id:guid}/load", async (
            Guid id,
            IShiftRepository shiftRepository,
            IEmployeeLoadService employeeLoadService) =>
        {
            var shifts = await shiftRepository.GetByEmployeeIdAsync(id);

            var totalLoad = employeeLoadService.CalculateLoad(shifts);

            return Results.Ok(new
            {
                employeeId = id,
                shiftCount = shifts.Count,
                totalLoad
            });
        });
    }
}

public class CreateEmployeeRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}