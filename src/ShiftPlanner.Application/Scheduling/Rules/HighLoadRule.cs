using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Rules;

public class HighLoadRule : ISchedulingRule
{
    private readonly IEmployeeLoadService _employeeLoadService;
    private readonly IEmployeeLoadStatusService _loadStatusService;

    public HighLoadRule(
        IEmployeeLoadService employeeLoadService,
        IEmployeeLoadStatusService loadStatusService)
    {
        _employeeLoadService = employeeLoadService;
        _loadStatusService = loadStatusService;
    }

    public bool CanAssign(
        Employee employee,
        Shift shift,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee)
    {
        var employeeShifts = plannedShifts
            .Where(existingShift => existingShift.EmployeeId == employee.Id)
            .ToList();

        var currentLoad = _employeeLoadService.CalculateLoad(employeeShifts);

        var newShiftLoad = _employeeLoadService.CalculateLoad(
            new List<Shift> { shift });

        var projectedLoad = currentLoad + newShiftLoad;

        var projectedStatus =
            _loadStatusService.CalculateStatus(projectedLoad);

        return projectedStatus != LoadStatus.High;
    }
}