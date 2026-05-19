namespace ShiftPlanner.Application.Scheduling.Overview;

public interface IScheduleOverviewService
{
    ScheduleOverviewResponse CreateOverview(
        int employeeCount,
        int highRiskEmployeeCount,
        List<Domain.Shifts.Shift> shifts,
        List<ScheduleAssignmentResult> scheduleResults);
}