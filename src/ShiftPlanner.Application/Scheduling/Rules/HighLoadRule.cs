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

    public SchedulingRuleResult Evaluate(SchedulingRuleContext context)
    {
        var employeeShifts = context.PlannedShifts
            .Where(existingShift => existingShift.EmployeeId == context.Employee.Id)
            .ToList();

        var currentLoad = _employeeLoadService.CalculateLoad(employeeShifts);
        var newShiftLoad = _employeeLoadService.CalculateLoad(new List<Shift> { context.Shift });
        var projectedLoad = currentLoad + newShiftLoad;

        var projectedStatus = _loadStatusService.CalculateStatus(projectedLoad);

        if (projectedStatus == LoadStatus.High)
        {
            return SchedulingRuleResult.Failed(
                "Employee projected workload would be too high.");
        }

        return SchedulingRuleResult.Passed();
    }
}