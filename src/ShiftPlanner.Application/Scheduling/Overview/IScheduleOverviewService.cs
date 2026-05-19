using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Application.Scheduling.Overview;

public interface IScheduleOverviewService
{
    ScheduleOverviewResponse CreateOverview(
        List<Employee> employees,
        int highRiskEmployeeCount,
        List<Shift> shifts,
        List<ScheduleAssignmentResult> scheduleResults);
}