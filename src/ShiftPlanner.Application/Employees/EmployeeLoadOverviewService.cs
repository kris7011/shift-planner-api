using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Employees;

public class EmployeeLoadOverviewService
{
    private readonly IEmployeeLoadService _employeeLoadService;
    private readonly IEmployeeLoadStatusService _employeeLoadStatusService;

    public EmployeeLoadOverviewService(
        IEmployeeLoadService employeeLoadService,
        IEmployeeLoadStatusService employeeLoadStatusService)
    {
        _employeeLoadService = employeeLoadService;
        _employeeLoadStatusService = employeeLoadStatusService;
    }

    public List<EmployeeLoadOverviewItem> CreateOverview(
        List<Employee> employees,
        List<Shift> shifts)
    {
        return employees
            .Select(employee =>
            {
                var employeeShifts = shifts
                    .Where(shift => IsAssignedToEmployee(shift, employee))
                    .ToList();

                var totalLoad = _employeeLoadService.CalculateLoad(employeeShifts);
                var loadStatus = _employeeLoadStatusService.CalculateStatus(totalLoad);

                return new EmployeeLoadOverviewItem
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.Name,
                    Skills = employee.Skills,
                    TotalLoad = totalLoad,
                    LoadStatus = loadStatus.ToString(),
                    IsHighRisk = loadStatus.ToString() == "High"
                };
            })
            .OrderByDescending(employee => employee.TotalLoad)
            .ThenBy(employee => employee.EmployeeName)
            .ToList();
    }

    private static bool IsAssignedToEmployee(Shift shift, Employee employee)
    {
        if (shift.EmployeeId == employee.Id)
        {
            return true;
        }

        return shift.AssignedEmployees.Any(assignedEmployee =>
            assignedEmployee.Id == employee.Id);
    }
}