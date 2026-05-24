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

        app.MapGet("/api/employees/load-overview", async (
            IEmployeeRepository employeeRepository,
            IShiftRepository shiftRepository,
            EmployeeLoadOverviewService employeeLoadOverviewService) =>
        {
            var employees = await employeeRepository.GetAllAsync();
            var shifts = await shiftRepository.GetAllAsync();

            var overview = employeeLoadOverviewService.CreateOverview(
                employees.ToList(),
                shifts.ToList());

            return Results.Ok(overview);
        });

        app.MapGet("/api/employees/{id:guid}/load-details", async (
            Guid id,
            IEmployeeRepository employeeRepository,
            IShiftRepository shiftRepository,
            EmployeeLoadDetailsService employeeLoadDetailsService) =>
        {
            var employees = await employeeRepository.GetAllAsync();
            var employee = employees.FirstOrDefault(employee => employee.Id == id);

            if (employee == null)
            {
                return Results.NotFound(new
                {
                    error = "Employee was not found."
                });
            }

            var shifts = await shiftRepository.GetAllAsync();

            var details = employeeLoadDetailsService.CreateDetails(
                employee,
                shifts.ToList());

            return Results.Ok(details);
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

        app.MapGet("/api/employees/{id:guid}/overload-status", async (
            Guid id,
            IShiftRepository shiftRepository,
            IEmployeeLoadService employeeLoadService,
            IEmployeeLoadStatusService statusService) =>
        {
            var shifts = await shiftRepository.GetByEmployeeIdAsync(id);

            var totalLoad = employeeLoadService.CalculateLoad(shifts);
            var status = statusService.CalculateStatus(totalLoad);

            return Results.Ok(new
            {
                employeeId = id,
                shiftCount = shifts.Count,
                totalLoad,
                status = status.ToString()
            });
        });
    }
}

public class CreateEmployeeRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}