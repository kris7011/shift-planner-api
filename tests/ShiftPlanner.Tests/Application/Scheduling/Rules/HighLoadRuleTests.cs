using ShiftPlanner.Application.Employees;
using ShiftPlanner.Application.Scheduling.Rules;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;

namespace ShiftPlanner.Tests.Application.Scheduling.Rules;

public class HighLoadRuleTests
{
    [Fact]
    public void Evaluate_ReturnsFailed_WhenProjectedLoadIsHigh()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingNightShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Night,
            "CT",
            1,
            employee.Id);

        var newNightShift = new Shift(
            new DateOnly(2026, 5, 21),
            ShiftType.Night,
            "CT",
            1);

        var plannedShifts = new List<Shift>
        {
            existingNightShift
        };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();
        var rule = new HighLoadRule(loadService, statusService);

        var context = new SchedulingRuleContext
        {
            Employee = employee,
            Shift = newNightShift,
            PlannedShifts = plannedShifts,
            MaxAssignmentsPerEmployee = 5
        };

        var result = rule.Evaluate(context);

        Assert.False(result.Success);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public void Evaluate_ReturnsPassed_WhenProjectedLoadIsNotHigh()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingDayShift = new Shift(
            new DateOnly(2026, 5, 20),
            ShiftType.Day,
            "CT",
            1,
            employee.Id);

        var newDayShift = new Shift(
            new DateOnly(2026, 5, 21),
            ShiftType.Day,
            "CT",
            1);

        var plannedShifts = new List<Shift>
        {
            existingDayShift
        };

        var loadService = new EmployeeLoadService();
        var statusService = new EmployeeLoadStatusService();
        var rule = new HighLoadRule(loadService, statusService);

        var context = new SchedulingRuleContext
        {
            Employee = employee,
            Shift = newDayShift,
            PlannedShifts = plannedShifts,
            MaxAssignmentsPerEmployee = 5
        };

        var result = rule.Evaluate(context);

        Assert.True(result.Success);
        Assert.Null(result.FailureReason);
    }
}