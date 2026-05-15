using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling;

public class ScheduleGeneratorService : IScheduleGeneratorService
{
    public List<ScheduleAssignmentResult> Generate(
        List<Employee> employees,
        List<Shift> shifts)
    {
        var results = new List<ScheduleAssignmentResult>();

        foreach (var shift in shifts)
        {
            var matchingEmployee = employees
                .FirstOrDefault(employee => employee.HasSkill(shift.RequiredSkill));

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