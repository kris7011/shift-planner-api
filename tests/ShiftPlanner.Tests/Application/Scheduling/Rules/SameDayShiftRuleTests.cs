using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application.Scheduling.Rules;

public class SameDayShiftRuleTests
{
    [Fact]
    public void Evaluate_ReturnsFailed_WhenEmployeeAlreadyHasShiftSameDay()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1,
            employee.Id);

        var newShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Evening,
            "CT",
            1);

        var plannedShifts = new List<Shift> { existingShift };

        var rule = new SameDayShiftRule();

        var result = rule.Evaluate(
            employee,
            newShift,
            plannedShifts,
            maxAssignmentsPerEmployee: 5);

        Assert.False(result.Success);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public void Evaluate_ReturnsPassed_WhenEmployeeHasNoShiftSameDay()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingShift = new Shift(
            new DateOnly(2026, 5, 19),
            ShiftType.Day,
            "CT",
            1,
            employee.Id);

        var newShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Evening,
            "CT",
            1);

        var plannedShifts = new List<Shift> { existingShift };

        var rule = new SameDayShiftRule();

        var result = rule.Evaluate(
            employee,
            newShift,
            plannedShifts,
            maxAssignmentsPerEmployee: 5);

        Assert.True(result.Success);
        Assert.Null(result.FailureReason);
    }
}