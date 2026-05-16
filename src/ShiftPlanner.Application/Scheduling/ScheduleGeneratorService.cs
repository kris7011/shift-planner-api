using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling;

public class ScheduleGeneratorService : IScheduleGeneratorService
{
    private readonly IEmployeeLoadService _employeeLoadService;
    private readonly IEmployeeLoadStatusService _loadStatusService;

    public ScheduleGeneratorService(
        IEmployeeLoadService employeeLoadService,
        IEmployeeLoadStatusService loadStatusService)
    {
        _employeeLoadService = employeeLoadService;
        _loadStatusService = loadStatusService;
    }

    public List<ScheduleAssignmentResult> Generate(
        List<Employee> employees,
        List<Shift> shifts)
    {
        var results = new List<ScheduleAssignmentResult>();

        foreach (var shift in shifts)
        {
            var matchingEmployee = employees
                .Where(employee => employee.HasSkill(shift.RequiredSkill))
                .Where(employee =>
                {
                    var employeeShifts = shifts
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
                    var employeeShifts = shifts
                        .Where(existingShift => existingShift.EmployeeId == employee.Id)
                        .ToList();

                    return _employeeLoadService.CalculateLoad(employeeShifts);
                })
                .FirstOrDefault();

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