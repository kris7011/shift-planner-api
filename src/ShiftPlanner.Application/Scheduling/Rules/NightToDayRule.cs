using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Rules;

public class NightToDayRule : ISchedulingRule
{
    public bool CanAssign(
        Employee employee,
        Shift shift,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee)
    {
        var hasNightShiftBefore = plannedShifts.Any(existingShift =>
            existingShift.EmployeeId == employee.Id &&
            existingShift.ShiftType == ShiftType.Night &&
            existingShift.Date.AddDays(1) == shift.Date &&
            shift.ShiftType == ShiftType.Day);

        return !hasNightShiftBefore;
    }
}