using ShiftPlanner.Application.Employees;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling;

public class ScheduleGeneratorService : IScheduleGeneratorService
{
    private readonly IEmployeeLoadService _employeeLoadService;

    public ScheduleGeneratorService(IEmployeeLoadService employeeLoadService)
    {
        _employeeLoadService = employeeLoadService;
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