using ShiftPlanner.Application.Scheduling.Overview;
using ShiftPlanner.Application.Scheduling.Simulation;
using ShiftPlanner.Domain.Employees;
using ShiftPlanner.Domain.Shifts;
using ShiftPlanner.Tests.Helpers;

namespace ShiftPlanner.Tests.Application.Scheduling.Simulation;

public class ScheduleSimulationServiceTests
{
    [Fact]
    public void Simulate_ReturnsHighRisk_WhenShiftCannotBeCovered()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var request = new SimulateScheduleRequest
        {
            Date = new DateOnly(2026, 5, 15),
            ShiftType = ShiftType.Night,
            RequiredSkill = "UL",
            RequiredStaff = 1,
            MaxAssignmentsPerEmployee = 5
        };

        var employees = new List<Employee> { employee };
        var existingShifts = new List<Shift>();

        var service = ScheduleSimulationServiceFactory.Create();

        var result = service.Simulate(
            request,
            employees,
            existingShifts);

        Assert.False(result.CanBeCovered);
        Assert.Equal("UL", result.RequiredSkill);
        Assert.Equal(ScheduleRiskLevel.High, result.RiskLevel);
        Assert.Null(result.SuggestedEmployeeId);
        Assert.Null(result.SuggestedEmployeeName);

        Assert.Contains(result.FailureReasons, reason =>
            reason.Contains("Missing required skill"));

        Assert.Contains("cannot be covered", result.ImpactSummary);
        Assert.Contains("UL", result.ImpactSummary);
    }

    [Fact]
    public void Simulate_ReturnsLowRiskAndSuggestedEmployee_WhenShiftCanBeCovered()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var request = new SimulateScheduleRequest
        {
            Date = new DateOnly(2026, 5, 15),
            ShiftType = ShiftType.Day,
            RequiredSkill = "CT",
            RequiredStaff = 1,
            MaxAssignmentsPerEmployee = 5
        };

        var employees = new List<Employee> { employee };
        var existingShifts = new List<Shift>();

        var service = ScheduleSimulationServiceFactory.Create();

        var result = service.Simulate(
            request,
            employees,
            existingShifts);

        Assert.True(result.CanBeCovered);
        Assert.Equal("CT", result.RequiredSkill);
        Assert.Equal(ScheduleRiskLevel.Low, result.RiskLevel);
        Assert.Equal(employee.Id, result.SuggestedEmployeeId);
        Assert.Equal(employee.Name, result.SuggestedEmployeeName);
        Assert.Empty(result.FailureReasons);

        Assert.Contains("can be covered", result.ImpactSummary);
        Assert.Contains(employee.Name, result.ImpactSummary);
    }

    [Fact]
    public void Simulate_ReturnsHighRisk_WhenDayShiftIsAfterExistingNightShift()
    {
        var employee = new Employee("Kris", new List<string> { "CT" });

        var existingNightShift = new Shift(
            new DateOnly(2026, 5, 14),
            ShiftType.Night,
            "CT",
            1,
            employee.Id);

        var request = new SimulateScheduleRequest
        {
            Date = new DateOnly(2026, 5, 15),
            ShiftType = ShiftType.Day,
            RequiredSkill = "CT",
            RequiredStaff = 1,
            MaxAssignmentsPerEmployee = 5
        };

        var employees = new List<Employee> { employee };
        var existingShifts = new List<Shift> { existingNightShift };

        var service = ScheduleSimulationServiceFactory.Create();

        var result = service.Simulate(
            request,
            employees,
            existingShifts);

        Assert.False(result.CanBeCovered);
        Assert.Equal("CT", result.RequiredSkill);
        Assert.Equal(ScheduleRiskLevel.High, result.RiskLevel);
        Assert.Null(result.SuggestedEmployeeId);
        Assert.Null(result.SuggestedEmployeeName);

        Assert.Contains(result.FailureReasons, reason =>
            reason.Contains("day shift", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("night shift", StringComparison.OrdinalIgnoreCase));

        Assert.Contains("cannot be covered", result.ImpactSummary);
        Assert.Contains("CT", result.ImpactSummary);
    }
}