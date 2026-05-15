using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling;

public interface IScheduleGeneratorService
{
    List<ScheduleAssignmentResult> Generate(
        List<Employee> employees,
        List<Shift> shifts);
}