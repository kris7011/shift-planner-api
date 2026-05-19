using ShiftPlanner.Application.Scheduling;
using ShiftPlanner.Application.Scheduling.Overview;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application.Scheduling.Overview;

public class ScheduleOverviewServiceTests
{
    [Fact]
    public void CreateOverview_ReturnsSkillCapacityAndUncoveredRequiredSkills_WhenSkillIsMissing()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var unassignedShift = new Shift(
            new DateOnly(2026, 5, 13),
            ShiftType.Night,
            "UL",
            1);

        var scheduleResult = new ScheduleAssignmentResult
        {
            ShiftId = unassignedShift.Id,
            EmployeeId = null,
            EmployeeName = null,
            RequiredSkill = "UL",
            WasAssigned = false,
            FailureReasons =
            [
                "Kris: Missing required skill 'UL'."
            ]
        };

        var employees = new List<Employee> { employee };
        var shifts = new List<Shift> { unassignedShift };
        var scheduleResults = new List<ScheduleAssignmentResult> { scheduleResult };

        var service = new ScheduleOverviewService();

        var result = service.CreateOverview(
            employees,
            highRiskEmployeeCount: 0,
            shifts,
            scheduleResults);

        Assert.Equal(1, result.TotalShifts);
        Assert.Equal(0, result.AssignedShifts);
        Assert.Equal(1, result.UnassignedShifts);
        Assert.Equal(0, result.CoverageRate);

        var skillCapacity = Assert.Single(result.SkillCapacity);
        Assert.Equal("CT", skillCapacity.Skill);
        Assert.Equal(1, skillCapacity.EmployeeCount);

        var uncoveredSkill = Assert.Single(result.UncoveredRequiredSkills);
        Assert.Equal("UL", uncoveredSkill.Skill);
        Assert.Equal(1, uncoveredSkill.RequiredByUnassignedShifts);
        Assert.Equal(0, uncoveredSkill.AvailableEmployees);

        Assert.Contains(result.RiskIndicators, indicator =>
            indicator.Type == "Capacity" &&
            indicator.Severity == ScheduleRiskLevel.High &&
            indicator.Message.Contains("UL"));
    }
}