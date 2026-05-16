using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Rules;

public class SameDayShiftRule : ISchedulingRule
{
    public bool CanAssign(
        Employee employee,
        Shift shift,
        List<Shift> plannedShifts,
        int maxAssignmentsPerEmployee)
    {
        var alreadyAssignedSameDay = plannedShifts.Any(existingShift =>
            existingShift.EmployeeId == employee.Id &&
            existingShift.Date == shift.Date);

        return !alreadyAssignedSameDay;
    }
}