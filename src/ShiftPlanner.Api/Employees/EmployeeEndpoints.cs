using ShiftPlanner.Application.Employees;
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
    }
}

public class CreateEmployeeRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}