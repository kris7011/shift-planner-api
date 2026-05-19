namespace ShiftPlanner.Application.Scheduling.Overview;

public class ScheduleOverviewService : IScheduleOverviewService
{
    public ScheduleOverviewResponse CreateOverview(
        int employeeCount,
        int highRiskEmployeeCount,
        List<Domain.Shifts.Shift> shifts,
        List<ScheduleAssignmentResult> scheduleResults)
    {
        var totalShifts = shifts.Count;
        var assignedShifts = shifts.Count(shift => shift.EmployeeId != null);
        var unassignedShifts = totalShifts - assignedShifts;

        var coverageRate = totalShifts == 0
            ? 0
            : Math.Round((decimal)assignedShifts / totalShifts * 100, 2);

        var unassignedShiftDetails = scheduleResults
            .Where(result => !result.WasAssigned)
            .Select(result =>
            {
                var shift = shifts.First(x => x.Id == result.ShiftId);

                return new UnassignedShiftOverview
                {
                    ShiftId = shift.Id,
                    Date = shift.Date,
                    ShiftType = shift.ShiftType,
                    RequiredSkill = shift.RequiredSkill,
                    FailureReasons = result.FailureReasons
                };
            })
            .ToList();

        var skillGaps = unassignedShiftDetails
            .GroupBy(shift => shift.RequiredSkill)
            .Select(group => new SkillGapOverview
            {
                RequiredSkill = group.Key,
                UnassignedShiftCount = group.Count()
            })
            .OrderByDescending(skillGap => skillGap.UnassignedShiftCount)
            .ThenBy(skillGap => skillGap.RequiredSkill)
            .ToList();

        var riskLevel = CalculateRiskLevel(
            unassignedShifts,
            skillGaps.Count,
            highRiskEmployeeCount);

        var riskSummary = new ScheduleRiskSummary
        {
            CoverageRisk = riskLevel,
            UnassignedShiftCount = unassignedShifts,
            SkillGapCount = skillGaps.Count,
            HighRiskEmployeeCount = highRiskEmployeeCount
        };

        return new ScheduleOverviewResponse
        {
            TotalShifts = totalShifts,
            AssignedShifts = assignedShifts,
            UnassignedShifts = unassignedShifts,
            CoverageRate = coverageRate,
            EmployeeCount = employeeCount,
            HighRiskEmployeeCount = highRiskEmployeeCount,
            UnassignedShiftDetails = unassignedShiftDetails,
            SkillGaps = skillGaps,
            RiskSummary = riskSummary
        };
    }

    private static ScheduleRiskLevel CalculateRiskLevel(
        int unassignedShiftCount,
        int skillGapCount,
        int highRiskEmployeeCount)
    {
        if (unassignedShiftCount > 0 && highRiskEmployeeCount > 0)
        {
            return ScheduleRiskLevel.High;
        }

        if (unassignedShiftCount > 0 || skillGapCount > 0)
        {
            return ScheduleRiskLevel.Medium;
        }

        return ScheduleRiskLevel.Low;
    }
}