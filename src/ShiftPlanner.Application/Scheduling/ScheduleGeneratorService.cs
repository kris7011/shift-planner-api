using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling;

public class ScheduleGeneratorService : IScheduleGeneratorService
{
    private readonly IEmployeeLoadService _employeeLoadService;
    private readonly IEmployeeLoadStatusService _loadStatusService;

    private readonly List<ISchedulingRule> _rules;

    public ScheduleGeneratorService(
        IEmployeeLoadService employeeLoadService,
        IEmployeeLoadStatusService loadStatusService)
    {
        _employeeLoadService = employeeLoadService;
        _loadStatusService = loadStatusService;

        _rules =
        [
            new SameDayShiftRule(),
            new NightToDayRule()
        ];
    }

    public List<ScheduleAssignmentResult> Generate(
        List<Employee> employees,
        List<Shift> openShifts,
        List<Shift> existingShifts,
        int maxAssignmentsPerEmployee)
    {
        var results = new List<ScheduleAssignmentResult>();
        var plannedShifts = new List<Shift>(existingShifts);

        foreach (var shift in openShifts)
        {
            var matchingEmployee = employees
                .Where(employee => employee.HasSkill(shift.RequiredSkill))
                .Where(employee =>
                {
                    var currentAssignmentCount = plannedShifts
                        .Count(existingShift => existingShift.EmployeeId == employee.Id);

                    if (currentAssignmentCount >= maxAssignmentsPerEmployee)
                    {
                        return false;
                    }

                    var passesRules = _rules.All(rule =>
                        rule.CanAssign(
                            employee,
                            shift,
                            plannedShifts,
                            maxAssignmentsPerEmployee));

                    if (!passesRules)
                    {
                        return false;
                    }

                    var employeeShifts = plannedShifts
                        .Where(existingShift => existingShift.EmployeeId == employee.Id)
                        .ToList();

                    var currentLoad = _employeeLoadService.CalculateLoad(employeeShifts);
                    var newShiftLoad = _employeeLoadService.CalculateLoad(new List<Shift> { shift });
                    var projectedLoad = currentLoad + newShiftLoad;

                    var projectedStatus = _loadStatusService.CalculateStatus(projectedLoad);

                    return projectedStatus != LoadStatus.High;
                })
                .OrderBy(employee =>
                {
                    var employeeShifts = plannedShifts
                        .Where(existingShift => existingShift.EmployeeId == employee.Id)
                        .ToList();

                    return _employeeLoadService.CalculateLoad(employeeShifts);
                })
                .FirstOrDefault();

            if (matchingEmployee != null)
            {
                shift.AssignToEmployee(matchingEmployee.Id);
                plannedShifts.Add(shift);
            }

            results.Add(new ScheduleAssignmentResult
            {
                ShiftId = shift.Id,
                EmployeeId = matchingEmployee?.Id,
                EmployeeName = matchingEmployee?.Name,
                RequiredSkill = shift.RequiredSkill,
                WasAssigned = matchingEmployee != null
            });
        }

        return results;
    }
}