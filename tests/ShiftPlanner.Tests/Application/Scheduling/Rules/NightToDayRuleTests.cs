using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application.Scheduling.Rules;

public class NightToDayRuleTests
{
    [Fact]
    public void Evaluate_ReturnsFailed_WhenEmployeeHasNightShiftBeforeDayShift()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingNightShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Night,
            "CT",
            1,
            employee.Id);

        var newDayShift = new Shift(
            new DateOnly(2026, 5, 21),
            ShiftType.Day,
            "CT",
            1);

        var plannedShifts = new List<Shift> { existingNightShift };

        var rule = new NightToDayRule();

        var result = rule.Evaluate(
            employee,
            newDayShift,
            plannedShifts,
            maxAssignmentsPerEmployee: 5);

        Assert.False(result.Success);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public void Evaluate_ReturnsPassed_WhenShiftIsNotDayAfterNightShift()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingNightShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Night,
            "CT",
            1,
            employee.Id);

        var newEveningShift = new Shift(
            new DateOnly(2026, 5, 21),
            ShiftType.Evening,
            "CT",
            1);

        var plannedShifts = new List<Shift> { existingNightShift };

        var rule = new NightToDayRule();

        var result = rule.Evaluate(
            employee,
            newEveningShift,
            plannedShifts,
            maxAssignmentsPerEmployee: 5);

        Assert.True(result.Success);
        Assert.Null(result.FailureReason);
    }
}