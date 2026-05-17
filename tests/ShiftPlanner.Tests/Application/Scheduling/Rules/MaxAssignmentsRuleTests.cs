using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application.Scheduling.Rules;

public class MaxAssignmentsRuleTests
{
    [Fact]
    public void Evaluate_ReturnsFailed_WhenEmployeeHasReachedMaxAssignments()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingShift1 = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1,
            employee.Id);

        var existingShift2 = new Shift(
            new DateOnly(2026, 5, 21),
            ShiftType.Evening,
            "CT",
            1,
            employee.Id);

        var newShift = new Shift(
            new DateOnly(2026, 5, 22),
            ShiftType.Day,
            "CT",
            1);

        var plannedShifts = new List<Shift>
        {
            existingShift1,
            existingShift2
        };

        var rule = new MaxAssignmentsRule();

        var context = new SchedulingRuleContext
        {
            Employee = employee,
            Shift = newShift,
            PlannedShifts = plannedShifts,
            MaxAssignmentsPerEmployee = 2
        };

        var result = rule.Evaluate(context);

        Assert.False(result.Success);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public void Evaluate_ReturnsPassed_WhenEmployeeIsBelowMaxAssignments()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1,
            employee.Id);

        var newShift = new Shift(
            new DateOnly(2026, 5, 21),
            ShiftType.Evening,
            "CT",
            1);

        var plannedShifts = new List<Shift>
        {
            existingShift
        };

        var rule = new MaxAssignmentsRule();

        var context = new SchedulingRuleContext
        {
            Employee = employee,
            Shift = newShift,
            PlannedShifts = plannedShifts,
            MaxAssignmentsPerEmployee = 2
        };

        var result = rule.Evaluate(context);

        Assert.True(result.Success);
        Assert.Null(result.FailureReason);
    }
}