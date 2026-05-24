using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Employees;

public class EmployeeLoadDetailsService
{
    private readonly IEmployeeLoadService _employeeLoadService;
    private readonly IEmployeeLoadStatusService _employeeLoadStatusService;
    private readonly ShiftLoadCalculator _shiftLoadCalculator;

    public EmployeeLoadDetailsService(
        IEmployeeLoadService employeeLoadService,
        IEmployeeLoadStatusService employeeLoadStatusService)
    {
        _employeeLoadService = employeeLoadService;
        _employeeLoadStatusService = employeeLoadStatusService;
        _shiftLoadCalculator = new ShiftLoadCalculator();
    }

    public EmployeeLoadDetailsResponse CreateDetails(
        Employee employee,
        List<Shift> shifts)
    {
        var employeeShifts = shifts
            .Where(shift => IsAssignedToEmployee(shift, employee))
            .OrderBy(shift => shift.Date)
            .ThenBy(shift => shift.ShiftType)
            .ToList();

        var totalLoad = _employeeLoadService.CalculateLoad(employeeShifts);
        var loadStatus = _employeeLoadStatusService.CalculateStatus(totalLoad);

        return new EmployeeLoadDetailsResponse
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            Skills = employee.Skills,
            TotalLoad = totalLoad,
            LoadStatus = loadStatus.ToString(),
            IsHighRisk = loadStatus.ToString() == "High",
            AssignedShifts = employeeShifts
                .Select(shift => new EmployeeAssignedShiftLoadItem
                {
                    ShiftId = shift.Id,
                    Date = shift.Date,
                    ShiftType = shift.ShiftType,
                    RequiredSkill = shift.RequiredSkill,
                    LoadScore = _shiftLoadCalculator.Calculate(shift)
                })
                .ToList()
        };
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